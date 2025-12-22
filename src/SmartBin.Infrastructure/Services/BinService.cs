using SmartBin.Application.GenericRepository;
using SmartBin.Domain.Models;
using SmartBin.Application.Services;

namespace SmartBin.Infrastructure.Services;

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

    // Реализация метода интерфейса для обновления телеметрии
    public async Task UpdateTelemetryAsync(string binId, BinTelemetry telemetry)
    {
        var existing = await _repository.FindById(binId);
        if (existing == null)
            throw new KeyNotFoundException($"Bin '{binId}' not found.");

        telemetry.LastUpdated = telemetry.LastUpdated == default ? DateTime.UtcNow : telemetry.LastUpdated;
        existing.Telemetry = telemetry;
        existing.UpdatedAt = DateTime.UtcNow;

        _repository.ReplaceOne(existing);
        await Task.CompletedTask;
    }


    // Реализация метода интерфейса для обновления истории телеметрии
    public async Task UpdateTelemetryHistoryAsync(string binId, BinTelemetry telemetry)
    {
        // 1. Поиск существующей записи
        var existing = await _repository.FindById(binId);
        if (existing == null)
            throw new KeyNotFoundException($"Bin '{binId}' not found.");

        // 2. Подготовка данных телеметрии
        telemetry.LastUpdated = telemetry.LastUpdated == default ? DateTime.UtcNow : telemetry.LastUpdated;

        // 3. Добавление в историю
        // Приводим массив к списку для удобного добавления, либо создаем новый, если история пуста
        var historyList = existing.TelemetryHistory?.ToList() ?? new List<BinTelemetry>();
        historyList.Add(telemetry);

        // 4. Обновление объекта
        existing.TelemetryHistory = historyList.ToArray();
        existing.UpdatedAt = DateTime.UtcNow;

        // 5. Сохранение изменений
        _repository.ReplaceOne(existing);
        await Task.CompletedTask;
    }
}