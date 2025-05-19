using AutoMapper;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class QuizRepository : GenericRepository<Quiz>, IQuizRepository
    {
        private readonly IMapper _mapper;
        public QuizRepository(ApplicationDBContext context, IMapper mapper) : base(context)
        {

            _mapper = mapper;
        }

        public async Task<List<QuizDTO>> GetAllAsync()
        {
            return await _dbSet
                .Where(q => !q.IsDeleted)
                .Select(q => new QuizDTO
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Description = q.Description,
                    CoverImage = q.CoverImage,
                    IsPublic = q.IsPublic,
                    CreatedAt = q.CreatedAt
                }).ToListAsync()
                .ConfigureAwaitFalse();
        }

        public async Task<QuizDTO?> GetByIdAsync(int id)
        {
            var quiz = await _dbSet
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                .Include(q => q.QuizTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted)
                .ConfigureAwaitFalse();

            return quiz == null ? null : _mapper.Map<QuizDTO>(quiz);
        }
        public async Task<Quiz?> GetQuiz(int id)
        {
            var quiz = await _dbSet
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(q => q.QuizCategories)
                    .ThenInclude(qc => qc.Category)
                .Include(q => q.QuizTags)
                    .ThenInclude(qt => qt.Tag)
                .FirstOrDefaultAsync(q => q.QuizId == id && !q.IsDeleted).ConfigureAwait(false);

            return quiz;
        }

        public IQueryable<Quiz> Search(string? key)
        {
            var query = _dbSet
                .Where(q => !q.IsDeleted && q.IsPublic);

            if (!string.IsNullOrWhiteSpace(key))
            {
                var loweredKey = key.Trim().ToLower();
                query = query.Where(q => q.Title.ToLower().Contains(loweredKey));
            }

            return query;
        }

    }
}
