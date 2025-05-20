using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.DTOs.Session; 

namespace U7Quizzes.IRepository
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        Task<SessionDTO> GetSesionByCode(string accessCode); 

    }
}