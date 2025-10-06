using U7Quizzes.IServices;
using U7Quizzes.Models;
using U7Quizzes.DTOs.Session; 

namespace U7Quizzes.IRepository
{
    public interface ISessionRepository : IGenericRepository<Session>
    {
        Task<Session> GetSessionByID(int SessionId); 

        Task<SessionDTO> GetSesionByCode(string accessCode);

        Task<List<ParticipantDTO>> GetParticipants(string accessCode);

        Task<Session> GetSessionByHostConnectionId(string connectionId);

    }
}