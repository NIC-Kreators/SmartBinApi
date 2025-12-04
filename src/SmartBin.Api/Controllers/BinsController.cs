using Microsoft.AspNetCore.Mvc;
using SmartBin.Api.Models;
using SmartBin.Api.Services;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BinsController : ControllerBase
{
    private readonly IBinService _binService;

    public BinsController(IBinService binService) => _binService = binService;

    // Поддерживает фильтрацию по статусу и минимальному уровню заполнения (в памяти)
    [HttpGet]
    public async Task<ActionResult<List<Bin>>> Get([FromQuery] string? status = null, [FromQuery] int? minFillLevel = null)
    {
        var bins = await _binService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(status))
            bins = bins.Where(b => string.Equals(b.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();

        if (minFillLevel.HasValue)
            bins = bins.Where(b => b.Telemetry != null && b.Telemetry.FillLevel >= minFillLevel.Value).ToList();

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
        public string Status { get; set; } = null!;
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
}