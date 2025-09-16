using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;


namespace U7Quizzes.IServices
{
    public interface ISessionService
    {
        Task<SessionDTO> CreateSession(CreateSessionDTO request);

        Task<ParticipantDTO> JoinSession(ParticipantDTO request, string accessCode);

        Task<List<ParticipantDTO>> GetParticipants(string AccessCode);

        Task<List<QuestionGetDTO>> StartSession(int sessionId, string UserID);

        Task EndSession(int sessionId);

        Task PauseSession(int sessionId);


        Task ResumeSession(int sessionId);
          
        Task FinishSession(int sessionId);
        
    }
}