using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;
using U7Quizzes.AppData;
using U7Quizzes.Caching;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Response;
using U7Quizzes.DTOs.Share;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.Services
{
    public class QuizService : IQuizService
    {
        private const int PageSize = 10;
        private readonly IQuizRepository _repo;
        private readonly IMapper _mapper;
        private readonly ApplicationDBContext _context;
        private readonly IImageService _imageService;
        private readonly ICachingService _cache;

        public QuizService(IQuizRepository repo, IMapper mapper, ApplicationDBContext context, IImageService imageService, ICachingService cache)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
            _imageService = imageService;

            _cache = cache;
        }

        public async Task<List<QuizDTO>> GetAllAsync()
        {
            var quizzes = await _repo.GetAllAsync();
            return _mapper.Map<List<QuizDTO>>(quizzes);
        }

        public async Task<QuizDTO?> GetByIdAsync(int id)
        {
            var cacheQuiz = await _cache.Get<QuizDTO>($"quiz:id:{id}"); 

            if(cacheQuiz == null)
            {
                var reposQuiz =  await _repo.GetByIdAsync(id);
                if (reposQuiz != null)
                    await _cache.Set<QuizDTO>(reposQuiz,$"quiz:id:{id}");

                return reposQuiz; 
            }

            return cacheQuiz;
        }

        public async Task<ServiceResponse<QuizDTO>> CreateAsync(QuizCreateDTO dto, string creatorId, IFormFile image)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Tạo quiz gốc
                    var quiz = _mapper.Map<Quiz>(dto);
                    quiz.CreatorId = creatorId;

                    // 2. Gán Categories
                    quiz.QuizCategories = dto.CategoryIds
                        .Select(id => new QuizCategory { CategoryId = id }).ToList();
                    quiz.CoverImage = await _imageService.UploadsAsync(image); 

                    // 3. Gán Tags
                    quiz.QuizTags = dto.TagIds
                        .Select(id => new QuizTag { TagId = id }).ToList();

                    // 4. Map & validate Questions
                    quiz.Questions = dto.Questions.Select(q =>
                    {
                        ValidateQuestionLogic(q);
                        var question = _mapper.Map<Question>(q);
                        question.Answers = q.Answers.Select(a => _mapper.Map<Answer>(a)).ToList();
                        return question;
                    }).ToList();

                    await _repo.AddAsync(quiz);
                    await transaction.CommitAsync();
                   // await _cache.Set<QuizDTO>(_mapper.Map<QuizDTO>(quiz), $"quiz:id:{quiz.QuizId}");

                    return ServiceResponse<QuizDTO>.Success(new QuizDTO(), "Thêm quiz thành công");
                }

                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return ServiceResponse<QuizDTO>.Failure("Thêm quiz thất bại : " + ex.Message);

                }
            }

        }

        public async Task<bool> UpdateAsync(QuizUpdateDTO dto)
        {
            using (var _transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var quiz = await _repo.GetQuiz(dto.QuizId);
                    if (quiz == null) return false;


                    _mapper.Map(dto, quiz);
                    quiz.UpdatedAt = DateTime.UtcNow;

                    var oldQuestions = _context.Question
                        .Where(q => q.QuizId == quiz.QuizId);

                    _context.Question.RemoveRange(oldQuestions);

                    quiz.Questions = dto.Questions.Select(q =>
                    {
                        ValidateQuestionLogic(q);
                        var question = _mapper.Map<Question>(q);
                        question.Answers = q.Answers.Select(a => _mapper.Map<Answer>(a)).ToList();
                        return question;
                    }).ToList();

                    // ✅ Gán lại danh mục & tag mới 
                    quiz.QuizCategories = dto.CategoryIds
                        .Select(id => new QuizCategory { QuizId = quiz.QuizId, CategoryId = id }).ToList();

                    quiz.QuizTags = dto.TagIds
                        .Select(id => new QuizTag { QuizId = quiz.QuizId, TagId = id }).ToList();

                    await _repo.UpdateAsync(quiz);
                    await _transaction.CommitAsync();

                    await _cache.Remove($"quiz:id:{dto.QuizId}");

                    return true;
                }
                catch (Exception)
                {
                    await _transaction.RollbackAsync();
                    return false;
                }

            }
        }
        public async Task<bool> DeleteAsync(int id)
        {   
            var quiz = await _repo.GetQuiz(id);
            if (quiz == null) return false;

            quiz.IsDeleted = true;
            quiz.DeletedAt = DateTime.UtcNow; 

            await _repo.UpdateAsync(quiz);

            await _cache.Remove($"quiz:id:{id}");

            return true;
        }
        private void ValidateQuestionLogic(QuestionCreateDTO q)
        {
            if (q.Type == QuestionType.SingleChoice && q.Answers.Count(a => a.IsCorrect) != 1)
                throw new Exception("Câu hỏi 'chọn 1' phải có đúng 1 đáp án đúng");

            if (q.Type == QuestionType.TrueFalse && q.Answers.Count != 2)
                throw new Exception("Câu hỏi đúng/sai phải có 2 đáp án");

            if (q.Type == QuestionType.ShortAnswer && q.Answers.Count != 1)
                throw new Exception("Câu hỏi trả lời ngắn chỉ có 1 đáp án đúng");

            if (q.Type == QuestionType.MultipleChoice && q.Answers.All(a => !a.IsCorrect))
                throw new Exception("Câu hỏi chọn nhiều phải có ít nhất 1 đáp án đúng");
        }

        public async Task<PagedResult<QuizSearch>> GetByTagName(QuizFilter filter)
        {
            string key = _cache.GenerateKey(filter);
            
            var data = await _cache.Get<PagedResult<QuizSearch>>(key);
            if (data != null)
            {
                return data;
            }

            else
            {
                var query = _repo.Search(filter.Keyword);
                var count = await query.CountAsync();
                var filteredData = await Filter(query, filter);

                var result = new PagedResult<QuizSearch>()
                {
                    Data = filteredData,
                    CurrentPage = filter.CurrentPage,
                    MaxPage = (int)Math.Ceiling((double)count / PageSize)
                };

                if (filteredData != null )
                {
                    await _cache.Set(result, key);
                }
                return result;
                
            }
            

        }
        private async Task<List<QuizSearch>> Filter(IQueryable<Quiz> query, QuizFilter filter)
        {
            if (filter.Tags is { Count: > 0 })
            {
                query = query.Where(q =>
                    q.QuizTags.Any(qt => filter.Tags.Contains(qt.TagId)));
            }

            if (filter.Category is { Count: > 0 })
            {
                query = query.Where(q =>
                    q.QuizCategories.Any(qc => filter.Category.Contains(qc.CategoryId)));
            }

            return await query.Select(x => new QuizSearch
            {
                QuizId = x.QuizId,
                Title = x.Title,
                ImageUrl = x.CoverImage,
                TotalAttempts = x.Sessions != null ? x.Sessions.Count : 0,
                QuestionCount = x.Questions != null ? x.Questions.Count : 0
            }).Skip(PageSize * (filter.CurrentPage - 1))
            .Take(PageSize)
            .ToListAsync();
        }




    }
}


