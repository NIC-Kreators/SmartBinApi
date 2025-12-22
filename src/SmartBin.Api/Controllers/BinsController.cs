using Bogus;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Api.Attributes;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;
using System;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BinsController : ControllerBase
{
    private readonly IBinService _binService;

    public BinsController(IBinService binService) => _binService = binService;

    // Поддерживает фильтрацию по статусу и минимальному уровню заполнения (в памяти)
    [HttpGet]
    public async Task<ActionResult<List<Bin>>> Get(
    [FromQuery] BinStatus? status = null, // Изменили string? на BinStatus?
    [FromQuery] int? minFillLevel = null)
    {
        var bins = await _binService.GetAllAsync();

        if (status.HasValue)
        {
            // Теперь сравнение типизированное и быстрое
            bins = bins.Where(b => b.Status == status.Value).ToList();
        }

        if (minFillLevel.HasValue)
        {
            bins = bins.Where(b => b.Telemetry != null && b.Telemetry.FillLevel >= minFillLevel.Value).ToList();
        }

        return Ok(bins);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Bin>> Get(string id)
    {
        var bin = await _binService.GetByIdAsync(id);
        if (bin == null) return NotFound();
        return Ok(bin);
    }

    [HttpPost]
    public async Task<ActionResult<Bin>> Post([FromBody] Bin bin)
    {
        try
        {
            var created = await _binService.CreateAsync(bin);
            return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Bin bin)
    {
        try
        {
            await _binService.UpdateAsync(id, bin);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _binService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    // Обновление телеметрии отдельным endpoint'ом
    [HttpPost("{id}/telemetry")]
    public async Task<IActionResult> PostTelemetry(string id, [FromBody] BinTelemetry telemetry)
    {
        try
        {
            telemetry.LastUpdated = telemetry.LastUpdated == default ? DateTime.UtcNow : telemetry.LastUpdated;
            await _binService.UpdateTelemetryAsync(id, telemetry);
            await _binService.UpdateTelemetryHistoryAsync(id, telemetry);
            // Проверка на аномалии
            if (telemetry.IsSmokeDetected)
            {
                var alert = new Alert
                {
                    BinId = id,
                    Type = AlertType.Smoke,
                    Severity = AlertSeverity.Critical,
                    Message = "Внимание! Обнаружено задымление в контейнере."
                };
                // await _alertService.CreateAsync(alert);
            }

            if (telemetry.FillLevel >= 90)
            {
                var alert = new Alert
                {
                    BinId = id,
                    Type = AlertType.Fullness,
                    Severity = telemetry.FillLevel >= 100 ? AlertSeverity.Critical : AlertSeverity.Warning,
                    Message = $"Контейнер заполнен на {telemetry.FillLevel}%"
                };
                // await _alertService.CreateAsync(alert);
            }
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    // Частичное обновление статуса бина через service.UpdateAsync
    public class UpdateStatusRequest
    {
        public BinStatus Status { get; set; }
    }


    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchStatus(string id, [FromBody] UpdateStatusRequest req)
    {
        try
        {
            var existing = await _binService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Status = req.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _binService.UpdateAsync(id, existing);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    [HttpPost("seed/{count}")]
    public async Task<IActionResult> SeedBins(int count = 10)
    {
        // 1. Настройка генератора телеметрии
        var telemetryFaker = new Faker<BinTelemetry>()
            .RuleFor(t => t.FillLevel, f => f.Random.Int(0, 100))
            .RuleFor(t => t.IsSmokeDetected, f => f.Random.Bool(0.05f)) // 5% шанс задымления
            .RuleFor(t => t.IsOverloaded, f => f.Random.Bool(0.1f))     // 10% шанс переполнения
            .RuleFor(t => t.LastUpdated, f => f.Date.Recent(1));

        // 2. Настройка генератора Bin
        var binFaker = new Faker<Bin>()
            .RuleFor(b => b.Type, f => f.PickRandom<BinType>())
            .RuleFor(b => b.Status, f => f.PickRandom<BinStatus>())
            // Генерация координат в пределах Алматы
            .RuleFor(b => b.Location, f => new GeoPoint(new double[]
            {
            f.Address.Longitude(76.80, 77.00), // Долгота
            f.Address.Latitude(43.20, 43.30)   // Широта
            }))
            .RuleFor(b => b.Telemetry, f => telemetryFaker.Generate())
            // Генерируем историю из 5 случайных записей
            .RuleFor(b => b.TelemetryHistory, f => telemetryFaker.Generate(5).ToArray())
            .RuleFor(b => b.CreatedAt, f => f.Date.Past(1))
            .RuleFor(b => b.UpdatedAt, f => DateTime.UtcNow);

        // 3. Генерация и сохранение
        var fakeBins = binFaker.Generate(count);

        foreach (var bin in fakeBins)
        {
            await _binService.CreateAsync(bin);
        }

        return Ok(new { message = $"Successfully seeded {count} bins in Almaty region", data = fakeBins });
    }


}