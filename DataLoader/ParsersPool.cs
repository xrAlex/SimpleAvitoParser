using Parser.Entities;

namespace Parser.DataLoader
{
    internal sealed class ParsersPool : IAsyncDisposable
    {
        private readonly Stack<ParserWorker> _workers = new();
        private readonly object _cycleLocker = new();
        private static ParsersPool? _pool;

        /// <summary>
        /// Создает пул клиентов на основе переданного списка прокси и возвращает его
        /// </summary>
        /// <remarks>В случае если пул уже создан возвращает существующий</remarks>
        /// <param name="proxies">Список прокси</param>
        public static ParsersPool CreateFixedPool(IEnumerable<ProxySettings>? proxies, bool useClientWorker = false)
        {
            if (_pool != null)
            {
                return _pool;
            }

            _pool = new ParsersPool();

            if (proxies != null)
            {
                foreach (var proxy in proxies)
                {
                    var proxyWorker = new ParserWorker(proxy);
                    _pool.AddWorker(ref proxyWorker);
                }

                if (useClientWorker)
                {
                    var worker = new ParserWorker();
                    _pool.AddWorker(ref worker);
                }
            }
            else
            {
                var worker = new ParserWorker();
                _pool.AddWorker(ref worker);
            }

            return _pool;
        }

        /// <summary>
        /// Добавляет ссылку на клиент в пул
        /// </summary>
        private void AddWorker(ref ParserWorker worker)
        {
            _workers.Push(worker);
        }

        /// <summary>
        /// Возвращает первый клиент в очереди
        /// </summary>
        public ParserWorker? GetWorker()
            => _workers.TryPop(out var freeWorker) ? freeWorker : null;

        public void ReleaseWorker(ref ParserWorker worker, int delay)
        {
            var workerToRelease = worker;
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                _workers.Push(workerToRelease);
            });
        }

        /// <summary>
        /// Ожидает освобождение клиента
        /// </summary>
        public ParserWorker? WaitForFreeWorker(CancellationToken cts = default)
        {
            lock (_cycleLocker)
            {
                while (!cts.IsCancellationRequested)
                {
                    var worker = GetWorker();
                    if (worker != null)
                    {
                        return worker;
                    }
                    Thread.Sleep(100);
                }

                return null;
            }
        }


        /// <summary>
        /// Удаляет клиент и ссылку на него из пула
        /// </summary>
        /// <param name="worker"></param>
        public async Task DisposeWorkerAsync(ParserWorker worker)
        {
            await worker.DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var worker in _workers)
            {
                await DisposeWorkerAsync(worker);
            }
        }

        private ParsersPool() { }
    }
}
