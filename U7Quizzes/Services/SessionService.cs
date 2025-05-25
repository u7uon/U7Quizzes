
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApplicationDBContext _context;
        private readonly ISessionRepository _seRepos;
        private readonly IQuizRepository _qRepos;
        private readonly IQuestionsRepository _quesRepos;

        private readonly IMapper _map; 
        private readonly IUserRepository _userRepos;

        public SessionService(ISessionRepository seRepos, IQuizRepository quizRepository, ApplicationDBContext context, IUserRepository userRepos,IMapper map, IQuestionsRepository quesRepos)
        {
            _context = context;
            _seRepos = seRepos;
            _qRepos = quizRepository;
            _userRepos = userRepos;
            _map = map;
            _quesRepos = quesRepos;
        }

        public async Task<SessionDTO> CreateSession(CreateSessionDTO request)
        {
            var quiz = await _qRepos.GetByIdAsync(request.QuizId);

            if (quiz == null)
                throw new NullReferenceException("Quiz does not exists ");

            var accessCode = await GenerateCode();

            var newSession = new Session
            {
                QuizId = request.QuizId,
                HostId = request.HostId,
                AccessCode = accessCode,
                Status = SessionStatus.Waiting,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                QrCodeUrl = $"https://yourdomain.com/join/{accessCode}"
            };

            await _seRepos.AddAsync(newSession);
            await _qRepos.SaveChangesAsync();


            return new SessionDTO
            {
                SessionID = newSession.SessionId,
                QuizId = newSession.QuizId,
                AccessCode = accessCode,
                Status = newSession.Status,
                HostName = "newSession.Host.DisplayName"
            };
        }

        public async Task<List<ParticipantDTO>> GetParticipants(string AccessCode)
        {
            return await _seRepos.GetParticipants(AccessCode);
        }

        public async Task<ParticipantDTO> JoinSession(ParticipantDTO request, string accessCode)
        {

            var session = await _seRepos.GetSesionByCode(accessCode);

            if (session == null)
                throw new Exception("session not exist");


            if (session.SessionStatus != SessionStatus.Waiting.ToString())
            {
                throw new Exception("Can't join this session");
            }

            var newParticipant = new Participant()
            {
                SessionId = session.SessionID
            };

            if (request.UserID != null)
            {
                var user = await _userRepos.GetUserProfile(request.UserID);

                if (user == null) throw new Exception("User does not exists");

                newParticipant.Nickname = user.DisplayName;
                newParticipant.UserId = user.UserID;
            }

            else
            {
                newParticipant.Nickname = request.DisplayName;
            }

            await _context.Participant.AddAsync(newParticipant);
            await _context.SaveChangesAsync();


            return _map.Map<ParticipantDTO>(newParticipant); 


        }

        public async Task<List<QuestionGetDTO>> StartSession(int sessionId, string UserId)
        {
            var session = await _seRepos.GetSessionByID(sessionId);

            if (session == null)
                throw new NullReferenceException("Session not found");

            if (!session.HostId.Equals(UserId))
                throw new UnauthorizedAccessException("User is not allowed to access this session");


            session.Status = SessionStatus.Active;
            session.StartTime = DateTime.UtcNow; 

            await _seRepos.UpdateAsync(session);
            await _seRepos.SaveChangesAsync();


            var questions = await _quesRepos.GetQuestionsByQuizId(session.QuizId); 


            return questions;

        }

        private async Task<string> GenerateCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rd = new Random();
            string code;
            bool isExist = false;


            do
            {
                code = new string(Enumerable.Repeat(chars, 6).Select(s => s[rd.Next(chars.Length)]).ToArray());

                isExist = await _context.Session.AnyAsync(x => x.AccessCode == code && !x.IsDeleted);

            }
            while (isExist);
            return code;

        }

    }
}