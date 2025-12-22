using SmartBin.Application.GenericRepository;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
namespace SmartBin.Infrastructure.Services
{
    public class AlertService : IAlertService
    {
        private readonly IRepository<Alert> _repository;

        public AlertService(IRepository<Alert> repository)
        {
            _repository = repository;
        }

        public async Task<List<Alert>> GetAllAsync()
        {
            // Используем метод из твоего репозитория
            return await _repository.GetAllAsync();
        }

        public async Task<List<Alert>> GetActiveAlertsAsync()
        {
            // Используем AsQueryable для фильтрации на стороне базы (или в памяти, если провайдер позволяет)
            return _repository.AsQueryable()
                .Where(a => !a.IsResolved)
                .ToList();
        }

        public async Task<List<Alert>> GetByBinIdAsync(string binId)
        {
            return _repository.AsQueryable()
                .Where(a => a.BinId == binId)
                .ToList();
        }

        public async Task<Alert> CreateAsync(Alert alert)
        {
            if (alert.CreatedAt == default) alert.CreatedAt = DateTime.UtcNow;

            // В твоем интерфейсе InsertOne — синхронный метод
            _repository.InsertOne(alert);

            return await Task.FromResult(alert);
        }

        public async Task ResolveAlertAsync(string id)
        {
            var alert = await _repository.FindById(id);
            if (alert == null) throw new KeyNotFoundException($"Alert with ID {id} not found.");

            alert.IsResolved = true;
            alert.ResolvedAt = DateTime.UtcNow;

            // В твоем интерфейсе ReplaceOne — синхронный метод
            _repository.ReplaceOne(alert);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            // В интерфейсе метод называется DeleteById
            _repository.DeleteById(id);
            await Task.CompletedTask;
        }
    }
}
