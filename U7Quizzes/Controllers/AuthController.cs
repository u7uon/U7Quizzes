using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Security;
using System.Security.Claims;
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
            var refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };

            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(15),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", refreshToken, refreshTokenCookieOptions);
            Response.Cookies.Append("access_token", result.Value.Accesstoken, accessTokenCookieOptions);

            return Ok();
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
                    return Unauthorized();
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
                return StatusCode(StatusCodes.Status500InternalServerError);
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


        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Auth", null, Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()   
        {
            var result = await HttpContext.AuthenticateAsync("Google");
            if (!result.Succeeded)
            {
                return BadRequest("Google authentication failed.");
            }

            var claims = result.Principal.Identities
                .FirstOrDefault()?.Claims.Select(c => new { c.Type, c.Value });

            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            // Kiểm tra nếu user đã tồn tại, nếu chưa thì tạo user mới
            var user = await _authService.FindOrCreateGoogleUser(email, name);

            // Tạo JWT token cho user
            var token = await _authService.GenerateToken(user);

            Response.Cookies.Append("access_token", token.Accesstoken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15),

                Path = "/"
            });

            Response.Cookies.Append("refreshToken", token.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            });

            return Redirect("http://localhost:5260/"); 
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var user = HttpContext.User;
            _logger.LogDebug("Getting user info"); 
            return Ok(new
            {
                Email = user.FindFirstValue(ClaimTypes.Email),
                Name = user.FindFirstValue(ClaimTypes.Name)
            });
        }

    }
}