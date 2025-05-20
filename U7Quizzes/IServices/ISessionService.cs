using U7Quizzes.DTOs.Session;


namespace U7Quizzes.IServices
{
    public interface ISessionService
    {
        Task<SessionDTO> CreateSession(CreateSessionDTO request ); 

        
    }
}