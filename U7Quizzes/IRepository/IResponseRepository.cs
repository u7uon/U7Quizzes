using U7Quizzes.Models;
using U7Quizzes.IServices;

namespace U7Quizzes.IRepository
{
    public interface IResponseRepository : IGenericRepository<Response>
    {
        Task<bool> IsResponseSubmitted(int participantId, int questionId); 
    }
}