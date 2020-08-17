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
        public async Task ExecuteAsync(ActionContext context, JsonStreamingResult<T> result)
        {
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = $"{MediaTypeNames.Application.Json};charset={Encoding.UTF8.WebName}";

            // https://github.com/dotnet/runtime/issues/38055
            // Once that's implemented, this can be simplified and optimized a lot
            if (result.Data is IAsyncEnumerable<T> asyncEnumerable)
            {
                Utf8JsonWriter writer = new Utf8JsonWriter(response.Body);
                try
                {
                    writer.WriteStartArray(); // won't cause a flush

                    await foreach (T value in asyncEnumerable)
                    {
                        /* It's important that this method calls SerializeAsync instead of Serialize
                         * because the network stream only accepts async writes.
                         * Unfortunately, there's no SerializeAsync method which takes a Utf8JsonWriter
                         * and thus a new one is created for every single value which is quite a few
                         * in our scenario. There's also a buffer created on every single pass.
                         * See here: https://github.com/dotnet/runtime/blob/fe41d3c762695cfeb364a8a0b61e2e376d22da5a/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializer.Write.Stream.cs#L107-L108
                         * I believe that might be the reason the performance isn't improved but in fact worse.
                         */
                        await JsonSerializer.SerializeAsync(response.Body, value, result.SerializerOptions).ConfigureAwait(false);
                    }

                    // Make 100% sure the array-end can't be the trigger for a synchronous
                    // flush which would result in an exception.
                    await writer.FlushAsync().ConfigureAwait(false);

                    // won't flush because everything was just flushed
                    writer.WriteEndArray();
                }
                finally
                {
                    // flush the array-end and close
                    await writer.DisposeAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await JsonSerializer.SerializeAsync(response.Body, (IEnumerable<T>)result.Data, result.SerializerOptions).ConfigureAwait(false);
            }
        }
    }
}
