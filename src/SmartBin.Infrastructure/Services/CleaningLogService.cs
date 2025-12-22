using MongoDB.Bson;
using SmartBin.Application.GenericRepository;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;

namespace SmartBin.Infrastructure.Services;

public class CleaningLogService : ICleaningLogService
{
    private readonly IRepository<CleaningLog> _repo;
    private readonly IRepository<Bin> _binRepo;

    public CleaningLogService(IRepository<CleaningLog> repo, IRepository<Bin> binRepo)
    {
        _repo = repo;
        _binRepo = binRepo;
    }

    public async Task<List<CleaningLog>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<CleaningLog?> GetByIdAsync(string id)
    {
        return await _repo.FindById(id);
    }

    public async Task<CleaningLog> CreateAsync(CleaningLog log)
    {
        log.CreatedAt = DateTime.UtcNow;
        log.UpdatedAt = log.CreatedAt;

        _repo.InsertOne(log);
        return await Task.FromResult(log);
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repo.FindById(id);
        if (existing == null)
            throw new KeyNotFoundException($"CleaningLog '{id}' not found.");

        _repo.DeleteById(id);
        await Task.CompletedTask;
    }

    public async Task<CleaningLog> LogCleaningAsync(string binId, string userId, int removedKg, string? notes = null)
    {
        // Валидация: бин и пользователь (пользователь проверяется только наличием бинта здесь)
        var bin = await _binRepo.FindById(binId);
        if (bin == null)
            throw new KeyNotFoundException($"Bin '{binId}' not found.");

        var cleaning = new CleaningLog
        {
            BinId = ObjectId.Parse(binId),
            UserId = ObjectId.Parse(userId),
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow,
            RemovedWeightKg = removedKg,
            Notes = notes ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repo.InsertOne(cleaning);

        // Пример: обновление статуса бина после уборки
        bin.Status = BinStatus.Active;
        bin.UpdatedAt = DateTime.UtcNow;
        _binRepo.ReplaceOne(bin);

        return await Task.FromResult(cleaning);
    }
}