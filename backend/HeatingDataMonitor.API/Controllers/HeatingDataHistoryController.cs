using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeatingDataMonitor.API.Service;
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
        private readonly HeatingDataDbContext _dbContext;
        private readonly HeatingDataCacheService _cacheService;

        public HeatingDataHistoryController(HeatingDataDbContext dbContext, HeatingDataCacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            return Ok(_dbContext.HeatingData
                                .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                .OrderBy(d => d.ReceivedTime));
        }

        [HttpGet("MainTemperatures")]
        public IActionResult GetMainTemperatures([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            Expression<Func<HeatingData, object>> selector = d => new
            {
                d.ReceivedTime,
                d.Kessel,
                d.Boiler_1,
                d.Puffer_Oben,
                d.Puffer_Unten
            };

            var data = _dbContext.HeatingData
                                 .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                 .OrderBy(d => d.ReceivedTime)
                                 .Select(selector);

            return Ok(data);
        }

        [HttpGet("CachedValues")]
        public IActionResult GetCachedValues([FromQuery] int count)
        {
            if (count <= 0)
                return BadRequest();
            
            return Ok(_cacheService.GetCache(count));
        }
    }
}
