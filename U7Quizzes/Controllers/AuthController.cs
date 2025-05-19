using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IServices.Auth;

namespace U7Quizzes.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Tên đăng nhập hoặc mật khẩu không được để trống." });
            }

            var result = await _authService.Login(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.Error });
            }

            var refreshToken = result.Value.RefreshToken;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/auth"
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(new { AccessToken = result.Value.Accesstoken });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogWarning("Refresh token không tồn tại trong request.");
                    return Unauthorized(new { Message = "Token không tồn tại." });
                }

                var result = await _authService.RefreshToken(refreshToken);
                if (!result.IsSuccess)
                {
                    return Unauthorized(new { Message = result.Error });
                }

                Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7),
                    Path = "/api/auth"
                });

                return Ok(new { AccessToken = result.Value.Accesstoken });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi trong RefreshToken: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Đã xảy ra lỗi hệ thống." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO request)
        {
            
            
            if (request == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            var result = await _authService.Resgister(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = result.Error });
        }


        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshtoken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { Message = "Yêu câu không hợp lệ" });
                }

                await _authService.Logout(refreshToken);
                Response.Cookies.Delete("refreshtoken");

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi thực hiện yêu cầu: " + ex.Message });
            }
            
        }


        [HttpGet("check")]
        public async Task<IActionResult> check()
        {
            var a = new QuizFilter
            {
                Tags = new List<int> { 1, 2, 23, 3, 5 },
                Category = new List<int> { 5, 2, 23 },
                Keyword = "akldkasdkj"

            };
            string key = CacheKey.GenerateKey(a); 
            return Ok(key);
        }



    }
}