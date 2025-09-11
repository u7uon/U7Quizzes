using U7Quizzes.DTOs.Auth;
using U7Quizzes.DTOs.Share;
using U7Quizzes.Models;

namespace U7Quizzes.IServices.Auth
{
    public interface IAuthService
    {
        public Task<ServiceResponse<TokenDTO>> Login(LoginDTO request);

        public Task<ServiceResponse<TokenDTO>> Resgister(RegisterDTO request);

        public Task<ServiceResponse<TokenDTO>> RefreshToken(string refreshToken);

        public Task<ServiceResponse<LoginResponse>> ChangePassword(ResetPassDTO resetPassDTO);

        public Task<User> FindOrCreateGoogleUser(string email, string name); 
        public Task Logout(string refreshToken);

        public Task<TokenDTO> GenerateToken(User user);
    }
}
