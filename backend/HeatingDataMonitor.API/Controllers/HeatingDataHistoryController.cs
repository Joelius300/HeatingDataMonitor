using HeatingDataMonitor.API.Service;
using HeatingDataMonitor.Database;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace HeatingDataMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HeatingDataHistoryController : ControllerBase
{
    private readonly IHeatingDataRepository _repository;
    private readonly HeatingDataCacheService _cacheService;

    public HeatingDataHistoryController(IHeatingDataRepository repository, HeatingDataCacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
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

    [HttpGet("CachedValues")]
    public IActionResult GetCachedValues([FromQuery] int count)
    {
        if (count <= 0)
            return BadRequest();

        return Ok(_cacheService.GetCache(count));
    }
}
