using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class QuestionRepository :  GenericRepository<Question> , IQuestionsRepository
    {
        private readonly IMapper _map;
        public QuestionRepository(ApplicationDBContext _context , IMapper map) : base(_context)
        {
            _map = map;
        }

        public async Task<List<QuestionGetDTO>> GetQuestionsByQuizId(int quizId)
        {
            return await _dbSet.Where(x => x.QuizId == quizId).ProjectTo<QuestionGetDTO>(_map.ConfigurationProvider).ToListAsync().ConfigureAwaitFalse() ; 
        }
    }
}
