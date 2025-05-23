using U7Quizzes.DTOs.Quiz;
using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.Repository;

namespace U7Quizzes.IRepository
{
    public interface IQuizRepository :  IGenericRepository<Quiz>
    {
        IQueryable<Quiz> Search(string key);  
        Task<List<QuizDTO>> GetAllAsync();
        Task<QuizDTO?> GetByIdAsync(int id);
        Task<Quiz?> GetQuiz(int id);

        Task<List<QuestionGetDTO>> GetQuestions(int quizId); 
       
    }
}
