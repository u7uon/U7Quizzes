using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class SessionRepository : GenericRepository<Session>, ISessionRepository
    {
        public SessionRepository(ApplicationDBContext _context) : base(_context)
        {
            
        }

        public async Task<SessionDTO?> GetSesionByCode(string accessCode)
        {
            return await _dbSet
            .Where( x=> x.AccessCode == accessCode && !x.IsDeleted )
            .Select( x => new SessionDTO
            {
                QuizId = x.QuizId,
                AccessCode = x.AccessCode,
                Status = x.Status,
                HostName = x.Host.DisplayName
            }
            ).SingleOrDefaultAsync()  ; 
        }
    }
}