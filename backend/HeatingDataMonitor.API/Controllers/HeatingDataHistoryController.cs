using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeatingDataMonitor.History;
using HeatingDataMonitor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace HeatingDataMonitor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeatingDataHistoryController : ControllerBase
    {
        private const int MaxFetchCount = 1000;
        private readonly HeatingDataDbContext _dbContext;

        public HeatingDataHistoryController(HeatingDataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HeatingData>>> Get([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            return await _dbContext.HeatingData
                                   .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                   .ToListAsync();
        }

        [HttpGet("MainTemperatures")]
        public async Task<IActionResult> GetMainTemperatures([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            Expression<Func<HeatingData, object>> selector = d => new
            {
                d.Kessel,
                d.Boiler_1,
                d.Puffer_Oben,
                d.Puffer_Unten
            };

            var data = await _dbContext.HeatingData
                                       .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                       .Select(selector)
                                       .ToListAsync();

            return Ok(data);
        }

        [HttpGet("LatestKesselValues")]
        public async Task<IActionResult> GetLatestKesselValues([FromQuery] int count)
        {
            if (count > MaxFetchCount)
                return BadRequest($"The maximum number of values you're allowed to fetch is {MaxFetchCount}.");

            var data = await _dbContext.HeatingData
                                       .OrderBy(d => d.ReceivedTime)
                                       .Select(d => d.Kessel)
                                       .Take(count)
                                       .ToListAsync();

            return Ok(data);
        }
    }
}
