using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API
{
    internal class JsonStreamingResultExecutor<T> : IJsonStreamingResultExecutor<T>
    {
        private const int BufferSizeThreshold = 4 * 1024 * 1024;

        public async Task ExecuteAsync(ActionContext context, JsonStreamingResult<T> result)
        {
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = $"{MediaTypeNames.Application.Json};charset={Encoding.UTF8.WebName}";

            // https://github.com/dotnet/runtime/issues/38055
            // Once that's implemented, this can be simplified and optimized a lot
            if (result.Data is IAsyncEnumerable<T> asyncEnumerable)
            {
                using MemoryStream memoryStream = new MemoryStream();
                Utf8JsonWriter writer = new Utf8JsonWriter(memoryStream);

                try
                {
                    writer.WriteStartArray();

                    await foreach (T value in asyncEnumerable.ConfigureAwait(false))
                    {
                        JsonSerializer.Serialize(writer, value, result.SerializerOptions);

                        if (memoryStream.Length >= BufferSizeThreshold)
                        {
                            memoryStream.Position = 0;
                            await memoryStream.CopyToAsync(response.Body, context.HttpContext.RequestAborted).ConfigureAwait(false);
                            memoryStream.Position = 0;
                            memoryStream.SetLength(0);
                        }
                    }

                    writer.WriteEndArray();
                }
                finally
                {
                    // flush the array-end and close
                    await writer.DisposeAsync().ConfigureAwait(false);
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(response.Body, context.HttpContext.RequestAborted).ConfigureAwait(false);
                }
            }
            else
            {
                await JsonSerializer.SerializeAsync(response.Body, (IEnumerable<T>)result.Data, result.SerializerOptions).ConfigureAwait(false);
            }
        }
    }
}
