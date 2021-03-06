using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.Entities;
using Parser.Extensions;

namespace Parser.DataLoader
{
    internal sealed class DataLoader : IAsyncDisposable
    {
        private readonly ParsersPool _workersPool;
        private readonly ILogger? _logger;

        public DataLoader(IEnumerable<ProxySettings>? proxies, ILogger? logger, string? sessionId)
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

            _logger?.LogInformation($"Start parsing {linksCollection.Length} links");

            var tasksPool = linksCollection
                .AsParallel()
                .Select(link =>
                    Task.Run(() =>
                        ParseLinkAsync(
                            link: link,
                            cts: cts), cts))
                .ToList();

            var results = await ThreadingEx.GetResults(tasksPool, timeout);

            _logger?.LogInformation($"Parsing completed in: {stopwatch.Elapsed}");
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
                _logger?.LogWarning($"An error has occurred while parsing link: {link}, tries limit exceeded");
                return null;
            }

            var worker = _workersPool.WaitForFreeWorker(cts);

            if (worker == null) return null;

            try
            {
                _logger?.LogInformation($"Founded free worker, starts parsing link {link}");
                var json = await worker.GetJsonFromLinkAsync(link).ConfigureAwait(false);

                _logger?.LogInformation($"Parsing Link {link} finished");
                _workersPool.ReleaseWorker(ref worker);
                return json;
            }
            catch (WorkerBlockedException)
            {
                _logger?.LogWarning($"An error has occurred while parsing link {link}, {worker.Name} is blocked");
                await _workersPool.DisposeWorkerAsync(worker);
                return await ParseLinkAsync(link, tries, cts);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"An error has occurred while parsing link: {link}", ex);
                return await ParseLinkAsync(link, tries, cts);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _workersPool.DisposeAsync();
        }
    }
}
