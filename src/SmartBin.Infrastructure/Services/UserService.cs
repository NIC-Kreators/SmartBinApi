using SmartBin.Application.GenericRepository;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;
using SmartBin.Domain.Models.Dto;
using System.Linq.Expressions;
using System.Security.Authentication;

namespace SmartBin.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher _passwordHasher;
        public UserService(
            IRepository<User> repository,
            IJwtService jwtService,
            IPasswordHasher passwordHasher) // Внедрение Hash- и JWT-сервисов
        {
            _repository = repository;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _repository.FindById(id);
        }

        public async Task<User> CreateAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = user.CreatedAt;
            _repository.InsertOne(user); // текущая реализация InsertOne — синхронная/void в репозитории
            return await Task.FromResult(user);
        }

        public async Task UpdateAsync(string id, User user)
        {
            var existing = await _repository.FindById(id);
            if (existing == null)
                throw new KeyNotFoundException($"User '{id}' not found.");

            user.Id = existing.Id;
            user.CreatedAt = existing.CreatedAt;
            user.UpdatedAt = DateTime.UtcNow;

            _repository.ReplaceOne(user);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            var existing = await _repository.FindById(id);
            if (existing == null)
                throw new KeyNotFoundException($"User '{id}' not found.");

            _repository.DeleteById(id);
            await Task.CompletedTask;
        }
        public async Task<TokenPair> RegisterAsync(UserRegistrationDto registrationDto)
        {
            // 1. Проверка существования пользователя (предполагаем, что Nickname уникален)
            // Используем FindOne(Expression) из вашего репозитория.
            Expression<Func<User, bool>> filter = u => u.Nickname == registrationDto.Nickname;
            var existingUser = await _repository.FindOne(filter);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with nickname '{registrationDto.Nickname}' already exists.");
            }

            // 2. Хеширование пароля
            string hashedPassword = _passwordHasher.HashPassword(registrationDto.Password);

            // 3. Создание новой сущности
            var newUser = new User
            {
                // Nickname используется как логин в этом примере
                Nickname = registrationDto.Nickname,
                FullName = registrationDto.FullName, // Предполагаем, что FullName есть в DTO
                Role = "User", // Устанавливаем роль по умолчанию
                PasswordHash = hashedPassword,
                PasswordRecreationRequired = false,
                PasswordLastChangedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 4. Сохранение в БД
            _repository.InsertOne(newUser);

            // 5. Генерация токенов (Используем строковое представление ObjectId)
            // Мы должны конвертировать ObjectId в string для использования в Claims JWT
            return await _jwtService.GenerateTokenPairAsync(newUser.Id.ToString(), newUser.Nickname);
        }

        public async Task<TokenPair> LoginAsync(string nickname, string password)
        {
            // 1. Поиск пользователя по Nickname
            Expression<Func<User, bool>> filter = u => u.Nickname == nickname;
            var user = await _repository.FindOne(filter);

            // 2. Проверка существования и пароля
            if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                // Использование generic-исключения для безопасности
                throw new AuthenticationException("Invalid nickname or password.");
            }

            // 3. Генерация токенов
            return await _jwtService.GenerateTokenPairAsync(user.Id.ToString(), user.Nickname);
        }

    }
}
