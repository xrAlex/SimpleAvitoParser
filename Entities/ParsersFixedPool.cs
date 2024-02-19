using Parser.DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Entities
{
    public sealed class ParsersFixedPool : IDisposable
    {
        private readonly Queue<ParserWorker> _availableParsers;
        private readonly SemaphoreSlim _poolSemaphore;
        private bool _disposed;
        private readonly TimeSpan _returnDelay;

        public ParsersFixedPool(IEnumerable<ParserWorker> clients, TimeSpan returnDelay = default)
        {
            _availableParsers = new Queue<ParserWorker>(clients);
            _poolSemaphore = new SemaphoreSlim(_availableParsers.Count, _availableParsers.Count);
            _returnDelay = returnDelay == default ? TimeSpan.FromSeconds(5) : returnDelay;
        }

        public async Task<ParserWorker> GetParserAsync(CancellationToken cancellationToken = default)
        {
            await _poolSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ParsersFixedPool), "This pool has been disposed.");
            }

            lock (_availableParsers)
            {
                // Теперь этот вызов блокируется до тех пор, пока не будет доступен объект.
                // Предполагается, что объект всегда будет доступен после ожидания семафора,
                // так как семафор ограничивает доступ к количеству доступных объектов.
                return _availableParsers.Dequeue();
            }
        }

        public async Task ReturnParserToPoolAsync(ParserWorker obj, bool dispose = false)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Returned object cannot be null.");
            }

            if (dispose || _disposed)
            {
                DisposeObject(obj);
                return;
            }

            // Implement delay before returning the object to the pool
            if (_returnDelay > TimeSpan.Zero)
            {
                await Task.Delay(_returnDelay);
            }

            lock (_availableParsers)
            {
                _availableParsers.Enqueue(obj);
            }
            _poolSemaphore.Release();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            lock (_availableParsers)
            {
                while (_availableParsers.Count > 0)
                {
                    var obj = _availableParsers.Dequeue();
                    DisposeObject(obj);
                }
            }
            _poolSemaphore.Dispose();
        }

        private static void DisposeObject(ParserWorker client)
        {
            client?.Dispose();
        }
    }
}
