using SmartBin.Api.GenericRepository;
using SmartBin.Api.Models;

namespace SmartBin.Api.Services;

public class BinService : IBinService
{
    private readonly IRepository<Bin> _repository;

    public BinService(IRepository<Bin> repository)
    {
        _repository = repository;
    }

    public async Task<List<Bin>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Bin?> GetByIdAsync(string id)
    {
        return await _repository.FindById(id);
    }

    public async Task<Bin> CreateAsync(Bin bin)
    {
        bin.CreatedAt = DateTime.UtcNow;
        bin.UpdatedAt = bin.CreatedAt;
        _repository.InsertOne(bin); // соответствует текущему API репозитория
        return await Task.FromResult(bin);
    }

    public async Task UpdateAsync(string id, Bin bin)
    {
        var existing = await _repository.FindById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Bin '{id}' not found.");

        bin.Id = existing.Id;
        bin.CreatedAt = existing.CreatedAt;
        bin.UpdatedAt = DateTime.UtcNow;

        _repository.ReplaceOne(bin);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repository.FindById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Bin '{id}' not found.");

        _repository.DeleteById(id);
        await Task.CompletedTask;
    }
}