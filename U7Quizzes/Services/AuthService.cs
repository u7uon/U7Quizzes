using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.DTOs.Share;
using U7Quizzes.Extensions;
using U7Quizzes.IServices.Auth;
using U7Quizzes.Models;

namespace U7Quizzes.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;


        public AuthService(IConfiguration configuration, UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        public Task<ServiceResponse> ChangePassword(ResetPassDTO resetPassDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResponse> Login(LoginDTO request)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {UserName}", request.UserName);

                var user = await _userManager.FindByNameAsync(request.UserName);
                if (user is null)
                {
                    _logger.LogWarning("Đăng nhập thất bại: Tài khoản không tồn tại - {UserName}", request.UserName);
                    return new LoginResponse { IsSuccess = false, Message = "Tài khoản không tồn tại" };
                }

                var loginResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!loginResult.Succeeded)
                {
                    _logger.LogWarning("Đăng nhập thất bại: sai mật khẩu {UserId}", user.Id);
                    return new LoginResponse { IsSuccess = false, Message = "Sai mật khẩu" };
                }

                _logger.LogInformation("Đăng nhập thành công {UserId}", user.Id);

                var token = await _tokenService.GenerateToken(user, await _userManager.GetRolesAsync(user));

                return new LoginResponse { Token = token, IsSuccess = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi đăng nhập tài khoản {UserName}", request.UserName);
                return new LoginResponse { IsSuccess = false, Message = ex.Message };
            }
        }


        public Task Logout(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResponse> RefreshToken(string refreshToken)
        {
            try
            {
                var token = await _tokenService.CheckFreshToken(refreshToken); 
                // Kiểm tra refresh token từ cơ sở dữ liệu hoặc bộ nhớ
                if (token is null)
                {
                    return LoginFail("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                // Tìm người dùng theo UserID
                var user = await _userManager.FindByIdAsync(token.UserId);
                if (user == null)
                {
                    return LoginFail("Người dùng không tồn tại");
                }

                // Tạo lại access token và refresh token mới
                var newToken = await _tokenService.GenerateToken(user , await GetRoles(user) );


                return new LoginResponse
                {
                    Token = newToken,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return LoginFail("Đã xảy ra lỗi" + ex.Message);
            }
        }

        private LoginResponse LoginFail(string Message)
        {
            return new LoginResponse
            {
                IsSuccess = false,
                Message = Message
            }; 

        }


        private async Task<IList<string>> GetRoles(User user) => await _userManager.GetRolesAsync(user);

        public async Task<ServiceResponse> Resgister(RegisterDTO request)
        {
            if (request == null)
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ"
                };

            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = "Tên tài khoản đã tồn tại"
                };

            var newUser = new User
            {
                UserName = request.Username,
                DisplayName = request.DisplayName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(newUser, request.Password).ConfigureAwaitFalse();

            if (!result.Succeeded)
            {
                return new ServiceResponse
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            await _userManager.AddToRoleAsync(newUser, "User").ConfigureAwaitFalse();
            _logger.LogInformation($"Đăng kí thành công {newUser.Id}");

            return new ServiceResponse
            {
                IsSuccess = true,
                Message = "Đăng ký thành công"
            };
        }
    }
}
