using Microsoft.AspNetCore.Mvc;
using SmartBin.Api.Models;
using SmartBin.Api.Services;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CleaningLogsController : ControllerBase
{
    private readonly ICleaningLogService _cleaningLogService;

    public CleaningLogsController(ICleaningLogService cleaningLogService) =>
        _cleaningLogService = cleaningLogService;

    [HttpGet]
    public async Task<ActionResult<List<CleaningLog>>> Get()
    {
        var logs = await _cleaningLogService.GetAllAsync();
        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CleaningLog>> Get(string id)
    {
        var log = await _cleaningLogService.GetByIdAsync(id);
        if (log == null) return NotFound();
        return Ok(log);
    }

    [HttpPost]
    public async Task<ActionResult<CleaningLog>> Post([FromBody] CleaningLog log)
    {
        var created = await _cleaningLogService.CreateAsync(log);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _cleaningLogService.DeleteAsync(id);
        return NoContent();
    }

    // Доменный endpoint: log cleaning (создаёт запись уборки и обновляет бин)
    public class LogCleaningRequest
    {
        public string BinId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public int RemovedKg { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("log")]
    public async Task<ActionResult<CleaningLog>> LogCleaning([FromBody] LogCleaningRequest req)
    {
        var created = await _cleaningLogService.LogCleaningAsync(req.BinId, req.UserId, req.RemovedKg, req.Notes);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }
}