using U7Quizzes.DTOs.Quiz;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.IRepository
{
    public interface IQuestionsRepository : IGenericRepository<Question>
    {
        Task<List<QuestionGetDTO>> GetQuestionsByQuizId(int SessionId);
    }
}
