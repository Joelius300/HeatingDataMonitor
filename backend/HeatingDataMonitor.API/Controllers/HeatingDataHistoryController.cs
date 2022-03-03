using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using HeatingDataMonitor.Database.Read;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace HeatingDataMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeatingDataHistoryController : ControllerBase
{
    private readonly IHeatingDataRepository _repository;

    public HeatingDataHistoryController(IHeatingDataRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Instant from, [FromQuery] Instant to)
    {
        if (from > to)
            return BadRequest();

        return Ok(await _repository.FetchAsync(from, to));
    }

    [HttpGet("MainTemperatures")]
    public async Task<IActionResult> GetMainTemperatures([FromQuery] Instant from, [FromQuery] Instant to)
    {
        if (from > to)
            return BadRequest();

        return Ok(await _repository.FetchMainTemperaturesAsync(from, to));
    }

    [HttpGet("Latest")]
    public async Task<IActionResult> GetCachedValues([FromQuery] int count)
    {
        if (count <= 0)
            return BadRequest();

        return Ok(await _repository.FetchLatestAsync(count));
    }
}
