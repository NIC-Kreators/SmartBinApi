using SmartBin.Api.Models;

namespace SmartBin.Api.Services
{
    public interface ICleaningLogService
    {
        Task<List<CleaningLog>> GetAllAsync();
        Task<CleaningLog?> GetByIdAsync(string id);
        Task<CleaningLog> CreateAsync(CleaningLog log);
        Task DeleteAsync(string id);

        // Доменная операция: создать запись уборки и выполнить сопутствующие проверки/выровнять метаданные
        Task<CleaningLog> LogCleaningAsync(string binId, string userId, int removedKg, string? notes = null);
    }
}