using U7Quizzes.DTOs.Auth;

namespace U7Quizzes.IRepository
{
    public interface IUserRepository
    {
        Task<UserProfile> GetUserProfile(string UserID); 
    }
}