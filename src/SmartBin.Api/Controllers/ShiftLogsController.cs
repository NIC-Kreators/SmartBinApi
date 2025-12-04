using Microsoft.AspNetCore.Mvc;
using SmartBin.Api.Models;
using SmartBin.Api.Services;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftLogsController : ControllerBase
{
    private readonly IShiftLogService _shiftLogService;

    public ShiftLogsController(IShiftLogService shiftLogService) => _shiftLogService = shiftLogService;

    [HttpGet]
    public async Task<ActionResult<List<ShiftLog>>> Get()
    {
        var shifts = await _shiftLogService.GetAllAsync();
        return Ok(shifts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShiftLog>> Get(string id)
    {
        var shift = await _shiftLogService.GetByIdAsync(id);
        if (shift == null) return NotFound();
        return Ok(shift);
    }

    // Запустить смену
    public class StartShiftRequest
    {
        public string UserId { get; set; } = null!;
    }

    [HttpPost("start")]
    public async Task<ActionResult<ShiftLog>> Start([FromBody] StartShiftRequest req)
    {
        var created = await _shiftLogService.StartShiftAsync(req.UserId);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }

    // Завершить смену
    public class EndShiftRequest
    {
        public DateTime? EndedAt { get; set; }
        public IEnumerable<string>? CleanedBinIds { get; set; }
        public double DistanceKm { get; set; }
        public string? Route { get; set; }
    }

    [HttpPost("{id}/end")]
    public async Task<IActionResult> End(string id, [FromBody] EndShiftRequest req)
    {
        await _shiftLogService.EndShiftAsync(
            id,
            req.EndedAt ?? default,
            req.CleanedBinIds ?? Enumerable.Empty<string>(),
            req.DistanceKm,
            req.Route);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _shiftLogService.DeleteAsync(id);
        return NoContent();
    }
}