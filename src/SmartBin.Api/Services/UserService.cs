using SmartBin.Api.GenericRepository;
using SmartBin.Api.Models;

namespace SmartBin.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;

        public UserService(IRepository<User> repository)
        {
            _repository = repository;
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
    }
}
