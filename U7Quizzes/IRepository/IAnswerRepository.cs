using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Models;

namespace U7Quizzes.IRepository
{
    public interface IAnswerRepository
    {
        public Task<AnswerResponse?> GetAnswerById(int Id);

        public Task<List<AnswerResponse>> GetAnswersByRange(int[] answerIds); 
    }
}