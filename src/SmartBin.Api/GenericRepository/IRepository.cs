using MongoDB.Driver;

namespace SmartBin.Api.GenericRepository
{
    public interface IRepository<T> where T : IEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(ObjectId id);
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(ObjectId id);
    }
}
