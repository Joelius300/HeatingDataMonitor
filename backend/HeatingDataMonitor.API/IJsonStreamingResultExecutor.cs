using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API
{
    // This exists so an open generic type can be registered for automatic resolution
    public interface IJsonStreamingResultExecutor<T> : IActionResultExecutor<JsonStreamingResult<T>>
    {
    }
}
