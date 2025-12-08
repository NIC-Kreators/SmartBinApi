using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartBin.Application.Services;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartBin.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        // ⚠️ В реальном проекте замените это на репозиторий IRepository<RefreshToken> для работы с БД
        private static readonly ConcurrentDictionary<string, string> _refreshTokens = new();

        private readonly IConfiguration _configuration;
        private readonly byte[] _secretKeyBytes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Получаем секретный ключ из конфигурации
            var secret = _configuration["Jwt:Key"]
                         ?? throw new InvalidOperationException("Jwt:Key not configured.");

            _secretKeyBytes = Encoding.ASCII.GetBytes(secret);
        }

        private string GenerateAccessToken(string userId, string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = new SymmetricSecurityKey(_secretKeyBytes);
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.Name, userName)
                // Добавьте другие Claims (роли и т.д.)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access Token на 15 минут
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = credentials
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            // Генерация простого GUID как Refresh Token
            // В продакшене лучше использовать криптографически сильные случайные строки
            return Guid.NewGuid().ToString("N");
        }

        public async Task<TokenPair> GenerateTokenPairAsync(string userId, string userName)
        {
            var accessToken = GenerateAccessToken(userId, userName);
            var refreshToken = GenerateRefreshToken();

            // ⚠️ В продакшене: сохранение Refresh Token в БД с его сроком действия (3 месяца)
            // Здесь мы просто сохраняем токен в in-memory хранилище
            _refreshTokens.AddOrUpdate(userId, refreshToken, (key, oldValue) => refreshToken);

            return new TokenPair(accessToken, refreshToken);
        }

        public async Task<bool> IsRefreshTokenValidAsync(string userId, string refreshToken)
        {
            // ⚠️ В продакшене: запрос к БД для проверки, совпадает ли токен 
            // и не истек ли его срок действия (3 месяца)
            if (_refreshTokens.TryGetValue(userId, out var storedToken))
            {
                return storedToken == refreshToken;
            }
            return false;
        }

        public async Task RemoveRefreshTokenAsync(string userId, string refreshToken)
        {
            // ⚠️ В продакшене: удаление токена из БД
            _refreshTokens.TryRemove(userId, out _);
        }
    }
}
