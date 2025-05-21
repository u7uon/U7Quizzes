using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.IRepository;

namespace U7Quizzes.Repository
{

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context; 
        }

        public async Task<UserProfile?> GetUserProfile(string UserID)
        {
            return await _context.User.Where(x => x.Id == UserID).Select(x => new UserProfile
            {
                UserID = x.Id,
                DisplayName = x.DisplayName,
                UserName = x.UserName 
            })
            .SingleOrDefaultAsync(); 
        }
    }
}