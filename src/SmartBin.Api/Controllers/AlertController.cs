using Microsoft.AspNetCore.Mvc;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    // Получить вообще все алерты из базы
    [HttpGet]
    public async Task<ActionResult<List<Alert>>> GetAll()
    {
        var alerts = await _alertService.GetAllAsync();
        return Ok(alerts);
    }

    // Получить только активные (нерешенные) аномалии
    [HttpGet("active")]
    public async Task<ActionResult<List<Alert>>> GetActive()
    {
        var alerts = await _alertService.GetActiveAlertsAsync();
        return Ok(alerts);
    }

    // Получить историю аномалий для конкретной мусорки
    [HttpGet("bin/{binId}")]
    public async Task<ActionResult<List<Alert>>> GetByBin(string binId)
    {
        var alerts = await _alertService.GetByBinIdAsync(binId);
        if (alerts == null || !alerts.Any())
        {
            return NotFound($"No alerts found for bin with ID {binId}");
        }
        return Ok(alerts);
    }

    // Пометить аномалию как исправленную
    [HttpPatch("{id}/resolve")]
    public async Task<IActionResult> Resolve(string id)
    {
        try
        {
            await _alertService.ResolveAlertAsync(id);
            return NoContent(); // 204 No Content - успешно обновили, возвращать нечего
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Alert with ID {id} not found");
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }

    // Удалить запись об аномалии
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _alertService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(detail: ex.Message);
        }
    }
}