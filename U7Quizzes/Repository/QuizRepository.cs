using AutoMapper;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IMapper _mapper;
        public QuizRepository(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<QuizDTO>> GetAllAsync()
        {
            return await _context.Quiz
                .Where(q => !q.IsDeleted)
                .Select(q => new QuizDTO
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Description = q.Description,
                    CoverImage = q.CoverImage,
                    IsPublic = q.IsPublic,
                    CreatedAt = q.CreatedAt
                }).ToListAsync();
        }

        public async Task<QuizDTO?> GetByIdAsync(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                .Include(q => q.QuizTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted);

            return quiz == null ? null : _mapper.Map<QuizDTO>(quiz);
        }
        public async Task<Quiz?> GetQuiz(int id)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                .Include(q => q.QuizTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted);

            return quiz ;
        }

        public async Task<Quiz> AddAsync(Quiz quiz)
        {
            _context.Quiz.Add(quiz);
            await _context.SaveChangesAsync(); 

            return quiz;
        }

        public async Task UpdateAsync(Quiz quiz)
        {
            _context.Quiz.Update(quiz);
           await _context.SaveChangesAsync(); 
        }

        public async Task DeleteAsync(Quiz quiz)
        {
            quiz.IsDeleted = true;
            quiz.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(quiz);
        }

        public async Task<List<QuizDTO>> GetByTagsName(string tagName)
        {
            return await _context.Quiz
            .Where(q => !q.IsDeleted && q.IsPublic && q.QuizTags
                .Any(qt => qt.Tag.Name.ToLower() == tagName.ToLower()))
            .Include(q => q.QuizTags).ThenInclude(qt => qt.Tag)
            .Select( x => new QuizDTO
            {
                QuizId = x.QuizId ,
                Title  = x.Title , 
                Description = x.Description , 
                CoverImage = x.CoverImage , 
            } )
            .ToListAsync();
        }
    }
}
