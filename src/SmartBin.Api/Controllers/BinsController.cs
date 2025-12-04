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

    [HttpGet]
    public async Task<ActionResult<List<Bin>>> Get()
    {
        var bins = await _binService.GetAllAsync();
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
        var created = await _binService.CreateAsync(bin);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Bin bin)
    {
        await _binService.UpdateAsync(id, bin);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _binService.DeleteAsync(id);
        return NoContent();
    }

    // Новый endpoint для телеметрии
    [HttpPost("{id}/telemetry")]
    public async Task<IActionResult> PostTelemetry(string id, [FromBody] BinTelemetry telemetry)
    {
        await _binService.UpdateTelemetryAsync(id, telemetry);
        return NoContent();
    }
}