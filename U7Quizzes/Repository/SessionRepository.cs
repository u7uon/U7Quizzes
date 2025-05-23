using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Session;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        public SessionRepository(ApplicationDBContext _context) : base(_context)
        {
            
        }

        public async Task<List<ParticipantDTO>> GetParticipants(string accessCode)
        {
            var session = await _dbSet
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.AccessCode == accessCode && !s.IsDeleted)
                .ConfigureAwaitFalse();

            if (session == null)
                throw new Exception("Session not found");

            return session.Participants
                .Where(p => !p.IsDeleted)
                .Select(p => new ParticipantDTO
                {
                    UserID = p.UserId,
                    DisplayName = p.Nickname
                })
                .ToList();
        }

        public async Task<SessionDTO?> GetSesionByCode(string accessCode)
        {
            return await _dbSet
            .Where(x => x.AccessCode == accessCode && !x.IsDeleted)
            .Select(x => new SessionDTO
            {
                SessionID = x.SessionId,
                QuizId = x.QuizId,
                AccessCode = x.AccessCode,
                Status = x.Status,
                HostName = x.Host.DisplayName
            }
            ).SingleOrDefaultAsync()
            .ConfigureAwaitFalse()   ; 
        }

        public async Task<Session> GetSessionByID(int SessionId)
        {
            return await _dbSet.FindAsync(SessionId) ;
        }
    }
}