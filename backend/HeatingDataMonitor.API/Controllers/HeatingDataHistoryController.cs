using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HeatingDataMonitor.History;
using HeatingDataMonitor.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeatingDataMonitor.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeatingDataHistoryController : ControllerBase
    {
        private readonly HeatingDataDbContext _dbContext;

        public HeatingDataHistoryController(HeatingDataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HeatingData>>> Get([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest();

            return await _dbContext.HeatingData
                                    .Where(d => d.ReceivedTime_UTC >= from && d.ReceivedTime_UTC <= to)
                                    .ToListAsync();
        }

        [HttpGet("MainTemperatures")]
        public async Task<IActionResult> GetMainTemperatures([FromQuery] DateTime from, [FromQuery] DateTime to)
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
                                    .Where(d => d.ReceivedTime_UTC >= from && d.ReceivedTime_UTC <= to)
                                    .Select(selector)
                                    .ToListAsync();

            return Ok(data);
        }

        // Might be cool but I don't know how to implement it. Also, we don't need that currently.
        //[HttpGet()]
        //public async Task<IActionResult> GetMainTemperatures([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] IEnumerable<string> columns)
        //{
        //    if (from > to)
        //        return BadRequest();

        //    var data = await _dbContext.HeatingData
        //                            .Where(d => d.Zeit >= from && d.Zeit <= to)
        //                            .Select(?GetSelector?(columns))
        //                            .ToListAsync();

        //    return Ok(data);
        //}
    }
}
