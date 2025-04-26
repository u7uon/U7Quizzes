using Azure.Core;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.Models;

namespace U7Quizzes.IServices.Auth
{
    public interface ITokenService
    {
        public Task<TokenDTO> GenerateToken(User user , IList<string> Roles);

      
        public Task<RefreshToken?> CheckFreshToken(string freshToken);

    }
}
