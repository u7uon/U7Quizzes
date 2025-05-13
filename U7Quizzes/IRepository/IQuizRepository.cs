using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Models;

namespace U7Quizzes.IRepository
{
    public interface IQuizRepository
    {
        Task<List<QuizDTO>> GetByTagsName(string tagName);  
        Task<List<QuizDTO>> GetAllAsync();
        Task<QuizDTO?> GetByIdAsync(int id);
        Task<Quiz> GetQuiz(int id);
        Task<Quiz> AddAsync(Quiz quiz);
        Task UpdateAsync(Quiz quiz);
        Task DeleteAsync(Quiz quiz);
    }
}
