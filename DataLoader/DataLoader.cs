using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Parser.Entities;
using Parser.Extensions;
using xrAsyncLogger;

namespace Parser.DataLoader
{
    internal sealed class DataLoader : IAsyncDisposable
    {
        private readonly ParsersPool _workersPool;
        private readonly Logger? _logger;

        public DataLoader(IEnumerable<ProxySettings>? proxies, Logger? logger, string? sessionId)
        {
            _logger = logger;
            _workersPool = ParsersPool.CreateFixedPool(proxies, sessionId);
        }

        /// <summary>
        /// Собирает все данные с переданных ссылок в формат Json
        /// </summary>
        /// <param name="links">Список страниц</param>
        /// <returns>Список данных с указанных ссылок в формате Json</returns>
        internal async Task<IEnumerable<JToken?>> ParseLinksAsync(IEnumerable<string> links, CancellationToken cts = new())
        {
            const int linkParsingTimeLimit = 30000;

            var linksCollection = links.ToArray();
            var timeout = linksCollection.Length * linkParsingTimeLimit;
            var stopwatch = Stopwatch.StartNew();

            _logger?.Info($"Start parsing {linksCollection.Length} links");

            var tasksPool = linksCollection
                .AsParallel()
                .Select(link =>
                    Task.Run(() =>
                        ParseLinkAsync(
                            link: link,
                            cts: cts), cts))
                .ToList();

            var results = await ThreadingEx.GetResults(tasksPool, timeout);

            _logger?.Info($"Parsing completed in: {stopwatch.Elapsed}");
            return results;
        }

        /// <summary>
        /// Cобирает все данные с переданной ссылок в формат Json
        /// </summary>
        /// <param name="link">Ссылка на страницу</param>
        /// <param name="tries">Текущая попытка парсинга страницы</param>
        /// <returns>Список данных в формате Json</returns>
        public async Task<JToken?> ParseLinkAsync(string link, int tries = 0, CancellationToken cts = new())
        {
            tries++;

            if (tries > 3)
            {
                _logger?.Warn($"An error has occurred while parsing link: {link}, tries limit exceeded");
                return null;
            }

            var worker = _workersPool.WaitForFreeWorker(cts);

            if (worker == null) return null;

            try
            {
                _logger?.Info($"Founded free worker, starts parsing link {link}");
                var json = await worker.GetJsonFromLinkAsync(link).ConfigureAwait(false);

                _logger?.Info($"Parsing Link {link} finished");
                _workersPool.ReleaseWorker(ref worker);
                return json;
            }
            catch (WorkerBlockedException)
            {
                _logger?.Warn($"An error has occurred while parsing link {link}, {worker.Name} is blocked");
                await _workersPool.DisposeWorkerAsync(worker);
                return await ParseLinkAsync(link, tries, cts);
            }
            catch (Exception ex)
            {
                _logger?.Warn($"An error has occurred while parsing link: {link}", ex);
                return await ParseLinkAsync(link, tries, cts);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _workersPool.DisposeAsync();
            _logger?.Dispose();
        }
    }
}
