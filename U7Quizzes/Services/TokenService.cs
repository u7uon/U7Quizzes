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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

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
            _logger.LogInformation("Checking refresh token: {RefreshToken}", refreshToken);

            var hashedRefreshToken = HashToken(refreshToken);
            var token = await _tokenRepository.GetRefreshTokenByHash(hashedRefreshToken);

            if (token == null || string.IsNullOrEmpty(token.Token))
            {
                _logger.LogWarning("Refresh token không tồn tại hoặc rỗng.");
                return null;
            }

            if (token.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token đã hết hạn.");
                return null;
            }

            _logger.LogInformation("Refresh token hợp lệ cho user {UserId}", token.UserId);
            return token;
        }

        public async Task<TokenDTO> GenerateToken(User user, IList<string> Roles)
        {
            try
            {
                _logger.LogInformation("Generating token for user {UserId}, roles: {Roles}", user.Id, string.Join(",", Roles));

                var accessToken = GenerateAccessToken(user, Roles);
                var refreshToken = GenerateRefreshToken();

                await SaveRefreshToken(refreshToken, user.Id);

                _logger.LogInformation("Tạo token thành công cho  {UserId}", user.Id);

                return new TokenDTO
                {
                    Accesstoken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi  khi tạo token {UserId}", user.Id);
                throw new Exception("Có lỗi xảy ra khi tạo token mới", ex);
            }
        }

        private async Task SaveRefreshToken(string token, string userId)
        {
            var hashedRefreshToken = HashToken(token);
            var existingToken = await _tokenRepository.GetFreshTokenByUserId(userId).ConfigureAwait(false);

            if (existingToken != null)
            {
                await _tokenRepository.DeleteAsync(existingToken).ConfigureAwait(false);
            }

            var newToken = new RefreshToken()
            {
                Token = hashedRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"])),
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _tokenRepository.CreateAsync(newToken).ConfigureAwait(false);
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
                new Claim(ClaimTypes.GivenName , user.DisplayName),
                new Claim(ClaimTypes.Role, Roles.Any() ? Roles[0] : "User")
            };

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

        public async Task RevokeToken(string Token)
        {
            var hashedToken = HashToken(Token);

            var existingToken = await _tokenRepository.GetRefreshTokenByHash(hashedToken);

            if (existingToken is null || string.IsNullOrEmpty(existingToken.Token))
                throw new Exception("Token không hợp lệ");

            await _tokenRepository.DeleteAsync(existingToken); 

        }
    }
}