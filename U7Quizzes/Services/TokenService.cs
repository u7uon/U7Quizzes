using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using U7Quizzes.DTOs.Auth;
using U7Quizzes.IRepository;
using U7Quizzes.IServices.Auth;
using U7Quizzes.Models;
using Microsoft.Extensions.Logging;

namespace U7Quizzes.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ITokenRepository tokenRepository, ILogger<TokenService> logger)
        {
            _configuration = configuration;
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public async Task<RefreshToken?> CheckFreshToken(string refreshToken)
        {
            _logger.LogInformation("Checking refresh token");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh token is null or empty");
                return null;
            }

            // Hash token từ client để so sánh với DB (đã hash khi lưu)
            var hashedRefreshToken = HashToken(refreshToken);
            var token = await _tokenRepository.GetRefreshTokenByHash(hashedRefreshToken);

            if (token == null)
            {
                _logger.LogWarning("Refresh token không tồn tại trong DB");
                return null;
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token đã hết hạn");
                // Xóa token đã hết hạn
                await _tokenRepository.DeleteAsync(token);
                return null;
            }

            _logger.LogInformation("Refresh token hợp lệ cho user {UserId}", token.UserId);
            return token;
        }

        public async Task<TokenDTO> GenerateToken(User user, IList<string> Roles)
        {
            try
            {
                _logger.LogInformation("Generating token for user {UserId}, roles: {Roles}",
                    user.Id, string.Join(",", Roles));

                var accessToken = GenerateAccessToken(user, Roles);
                var refreshToken = GenerateRefreshToken();

                // Lưu refresh token (sẽ được hash trong method này)
                await SaveRefreshToken(refreshToken, user.Id);

                _logger.LogInformation("Tạo token thành công cho user {UserId}", user.Id);

                return new TokenDTO
                {
                    Accesstoken = accessToken,
                    RefreshToken = refreshToken // Trả token gốc cho client
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo token cho user {UserId}", user.Id);
                throw new Exception("Có lỗi xảy ra khi tạo token mới", ex);
            }
        }

        private async Task SaveRefreshToken(string token, string userId)
        {
            try
            {
                // Hash token trước khi lưu DB (bảo mật)
                var hashedRefreshToken = HashToken(token);

                // Tìm và xóa token cũ nếu có
                var existingToken = await _tokenRepository.GetFreshTokenByUserId(userId);
                if (existingToken != null)
                {
                    await _tokenRepository.DeleteAsync(existingToken);
                }

                var newToken = new RefreshToken()
                {
                    Token = hashedRefreshToken, // Lưu token đã hash
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                await _tokenRepository.CreateAsync(newToken);
                _logger.LogInformation("Đã lưu refresh token mới cho user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu refresh token cho user {UserId}", userId);
                throw;
            }
        }

        private string GenerateAccessToken(User user, IList<string> Roles)
        {
            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var accessTokenExpiryInMinutes = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryInMinutes"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(ClaimTypes.Role, Roles.Any() ? Roles.First() : "User")
            };

            // Thêm tất cả roles nếu có nhiều role
            foreach (var role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(accessTokenExpiryInMinutes),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task RevokeToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token không được để trống");
                }

                var hashedToken = HashToken(token);
                var existingToken = await _tokenRepository.GetRefreshTokenByHash(hashedToken);

                if (existingToken == null)
                {
                    _logger.LogWarning("Không tìm thấy token để revoke");
                    throw new Exception("Token không hợp lệ hoặc đã được thu hồi");
                }

                await _tokenRepository.DeleteAsync(existingToken);
                _logger.LogInformation("Đã thu hồi refresh token cho user {UserId}", existingToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thu hồi token");
                throw;
            }
        }
    }
}