using Parser.Entities;

namespace Parser.DataLoader
{
    internal sealed class ParsersPool : IAsyncDisposable
    {
        private readonly Stack<ParserWorker> _workers = new();
        private readonly List<ParserWorker> _workersPool = new();
        private readonly object _cycleLocker = new();
        private readonly object _disposeLocker = new();
        private static ParsersPool? _pool;

        /// <summary>
        /// Создает пул клиентов на основе переданного списка прокси и возвращает его
        /// </summary>
        /// <remarks>В случае если пул уже создан возвращает существующий</remarks>
        /// <param name="proxies">Список прокси</param>
        /// <param name="sessionId">ID сессии Avito.ru</param>
        public static ParsersPool CreateFixedPool(IEnumerable<ProxySettings>? proxies, string? sessionId)
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
                    var worker = new ParserWorker(proxy, sessionId);
                    _pool._workersPool.Add(worker);
                    _pool.AddWorker(worker);
                }
            }
            else
            {
                var worker = new ParserWorker(sessionId: sessionId);
                _pool._workersPool.Add(worker);
                _pool.AddWorker(worker);
            }

            return _pool;
        }

        /// <summary>
        /// Добавляет ссылку на клиент в пул
        /// </summary>
        private void AddWorker(ParserWorker worker)
            => _workers.Push(worker);

        /// <summary>
        /// Возвращает первый клиент в очереди
        /// </summary>
        public ParserWorker? GetWorker()
            => _workers.TryPop(out var freeWorker) ? freeWorker : null;

        /// <summary>
        /// Возвращает клиент в пул после ожидания
        /// </summary>
        public void ReleaseWorker(ParserWorker worker, int timeout = 5000)
        {
            var item = worker;

            Task.Run(async () =>
            {
                await Task.Delay(timeout);
                _workers.Push(item);
            });
        }

        /// <summary>
        /// Ожидает освобождение клиента
        /// </summary>
        public ParserWorker? WaitForFreeWorker(CancellationToken cts = new())
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

        private ParsersPool() { }


        /// <summary>
        /// Удаляет клиент и ссылку на него из пула
        /// </summary>
        /// <param name="worker"></param>
        public async Task DisposeWorkerAsync(ParserWorker worker)
        {
            _workersPool.Remove(worker);
            await worker.DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var worker in _workersPool)
            {
                await worker.DisposeAsync();
            }
        }
    }
}
