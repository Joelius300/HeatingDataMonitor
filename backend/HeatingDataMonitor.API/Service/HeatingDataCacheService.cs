using HeatingDataMonitor.Model;
using HeatingDataMonitor.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeatingDataMonitor.API.Service
{
    public class HeatingDataCacheService : IHostedService, IDisposable
    {
        private readonly IHeatingDataReceiver _heatingDataReceiver;
        private readonly EventHandler<HeatingData> _receivedHandler;
        private readonly CacheOptions _options;
        private readonly List<HeatingData> _cache;

        public HeatingDataCacheService(IHeatingDataReceiver heatingDataReceiver, IOptions<CacheOptions> options)
        {
            _heatingDataReceiver = heatingDataReceiver;
            _options = options.Value;
            _cache = new List<HeatingData>(_options.MaxSize / 4);
            _receivedHandler = (o, e) => AddValue(e);
        }

        private void AddValue(HeatingData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            lock (_cache)
            {
                if (_cache.Count >= _options.MaxSize)
                {
                    _cache.RemoveAt(0);
                }

                _cache.Add(data);
            }
        }

        public IEnumerable<HeatingData> GetCache(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            lock (_cache)
            {
                if (count >= _cache.Count)
                {
                    return _cache;
                }
                else
                {
                    int toSkip = _cache.Count - count;
                    return _cache.Skip(toSkip);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _heatingDataReceiver.DataReceived += _receivedHandler;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _heatingDataReceiver.DataReceived -= _receivedHandler;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopAsync(CancellationToken.None);
        }
    }
}
