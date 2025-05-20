
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
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

        public SessionService(ISessionRepository seRepos, IQuizRepository quizRepository , ApplicationDBContext context)
        {
            _context = context; 
            _seRepos = seRepos;
            _qRepos = quizRepository;
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
                QuizId = newSession.QuizId,
                AccessCode = accessCode,
                Status = newSession.Status,
                HostName = "newSession.Host.DisplayName"               
            }; 
        }
 
        private async Task<string> GenerateCode() {
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