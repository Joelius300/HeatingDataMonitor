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
using Microsoft.Extensions.Options;
using NodaTime;

namespace HeatingDataMonitor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeatingDataHistoryController : ControllerBase
    {
        private readonly HeatingDataDbContext _dbContext;
        private readonly HeatingDataCacheService _cacheService;
        private readonly JsonOptions _jsonOptions;

        public HeatingDataHistoryController(HeatingDataDbContext dbContext, HeatingDataCacheService cacheService, IOptions<JsonOptions> jsonOptions)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
            _jsonOptions = jsonOptions.Value;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            var data = _dbContext.HeatingData
                                 .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                 .OrderBy(d => d.ReceivedTime);

            return JsonStreamingResult.Create(data, _jsonOptions.JsonSerializerOptions);
        }

        [HttpGet("MainTemperatures")]
        public IActionResult GetMainTemperatures([FromQuery] Instant from, [FromQuery] Instant to)
        {
            if (from > to)
                return BadRequest();

            var data = _dbContext.HeatingData
                                 .Where(d => d.ReceivedTime >= from && d.ReceivedTime <= to)
                                 .OrderBy(d => d.ReceivedTime)
                                 .Select(d => new
                                 {
                                     d.ReceivedTime,
                                     d.Kessel,
                                     d.Boiler_1,
                                     d.Puffer_Oben,
                                     d.Puffer_Unten,
                                     d.Abgas
                                 });

            return JsonStreamingResult.Create(data, _jsonOptions.JsonSerializerOptions);
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
