    using Microsoft.Identity.Client;
using U7Quizzes.Models;

namespace U7Quizzes.IRepository
{
    public interface ITokenRepository
    {
        public Task<RefreshToken> GetRefreshTokenByHash(string hashedToken);
        public Task<RefreshToken> GetFreshTokenByUserId(string UserID);
        Task CreateAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteAsync(RefreshToken refreshToken);
    }
}
