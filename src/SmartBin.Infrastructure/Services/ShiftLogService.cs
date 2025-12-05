using MongoDB.Bson;
using SmartBin.Application.GenericRepository;
using SmartBin.Domain.Models;
using SmartBin.Application.Services;


namespace SmartBin.Infrastructure.Services;

public class ShiftLogService : IShiftLogService
{
    private readonly IRepository<ShiftLog> _repo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Bin> _binRepo;

    public ShiftLogService(IRepository<ShiftLog> repo, IRepository<User> userRepo, IRepository<Bin> binRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
        _binRepo = binRepo;
    }

    public async Task<List<ShiftLog>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<ShiftLog?> GetByIdAsync(string id)
    {
        return await _repo.FindById(id);
    }

    public async Task<ShiftLog> StartShiftAsync(string userId)
    {
        // Проверка пользователя (опционально)
        var user = await _userRepo.FindById(userId);
        if (user == null)
            throw new KeyNotFoundException($"User '{userId}' not found.");

        var shift = new ShiftLog
        {
            UserId = ObjectId.Parse(userId),
            StartedAt = DateTime.UtcNow,
            EndedAt = DateTime.MinValue,
            CleanedBins = new List<ObjectId>(),
            DistanceTravelledKm = 0,
            Route = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repo.InsertOne(shift);
        return await Task.FromResult(shift);
    }

    public async Task EndShiftAsync(string shiftId, DateTime endedAt, IEnumerable<string> cleanedBinIds, double distanceKm, string? route = null)
    {
        var shift = await _repo.FindById(shiftId);
        if (shift == null)
            throw new KeyNotFoundException($"Shift '{shiftId}' not found.");

        // Конвертация cleanedBinIds в ObjectId и проверка существования бинтов (опционально)
        var cleanedObjectIds = new List<ObjectId>();
        foreach (var binId in cleanedBinIds ?? Enumerable.Empty<string>())
        {
            var bin = await _binRepo.FindById(binId);
            if (bin != null)
            {
                cleanedObjectIds.Add(ObjectId.Parse(binId));
            }
        }

        shift.EndedAt = endedAt == default ? DateTime.UtcNow : endedAt;
        shift.CleanedBins = cleanedObjectIds;
        shift.DistanceTravelledKm = distanceKm;
        shift.Route = route ?? shift.Route;
        shift.UpdatedAt = DateTime.UtcNow;

        _repo.ReplaceOne(shift);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(string id)
    {
        var existing = await _repo.FindById(id);
        if (existing == null)
            throw new KeyNotFoundException($"ShiftLog '{id}' not found.");

        _repo.DeleteById(id);
        await Task.CompletedTask;
    }
}