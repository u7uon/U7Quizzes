using U7Quizzes.DTOs.Auth;
using U7Quizzes.DTOs.Share;

namespace U7Quizzes.IServices.Auth
{
    public interface IAuthService
    {
        public Task<LoginResponse> Login(LoginDTO request);

        public Task<ServiceResponse> Resgister(RegisterDTO request);

        public Task<LoginResponse> RefreshToken(string refreshToken);

        public Task<ServiceResponse> ChangePassword(ResetPassDTO resetPassDTO);


        public Task Logout(string userId);
    }
}
