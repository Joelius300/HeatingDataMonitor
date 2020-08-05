using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API
{
    public class JsonStreamingResult<T> : IActionResult
    {
        public object Data { get; }
        public JsonSerializerOptions SerializerOptions { get; set; }

        public JsonStreamingResult(IEnumerable<T> data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public JsonStreamingResult(IAsyncEnumerable<T> data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            IJsonStreamingResultExecutor<T> executor = context.HttpContext.RequestServices.GetRequiredService<IJsonStreamingResultExecutor<T>>();

            return executor.ExecuteAsync(context, this);
        }
    }

    /// <summary>
    /// Contains methods for creating instances of <see cref="JsonStreamingResult{T}"/>.
    /// They are for convenience but also for anonymous types because there it's necessary
    /// to infer the generic type since you can't specify it.
    /// </summary>
    public static class JsonStreamingResult
    {
        public static JsonStreamingResult<T> Create<T>(IEnumerable<T> value, JsonSerializerOptions serializerOptions = null) => new JsonStreamingResult<T>(value)
        {
            SerializerOptions = serializerOptions
        };

        public static JsonStreamingResult<T> Create<T>(IAsyncEnumerable<T> value, JsonSerializerOptions serializerOptions = null) => new JsonStreamingResult<T>(value)
        {
            SerializerOptions = serializerOptions
        };
    }
}
