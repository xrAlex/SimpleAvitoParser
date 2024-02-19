using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.Entities;
using Parser.Extensions;

namespace Parser.DataLoader;

internal sealed class DataLoader : IDisposable
{
    private readonly ParsersFixedPool _workersPool;
    private readonly ILogger? _logger;
    private readonly bool _ignoreIpBlock;
    public event EventHandler<Tuple<JToken?, object?>>? LinkParsed;

    public DataLoader(IEnumerable<ProxySettings>? proxies, ILogger? logger, int? delay = null, bool hybridMode = false, bool ignoreIpBlock = false)
    {
        _logger = logger;
        _ignoreIpBlock = ignoreIpBlock;

        var workers = new List<ParserWorker>();
        if (proxies != null)
        {
            if (hybridMode)
            {
                workers.Add(new ParserWorker());
            }

            workers.AddRange(proxies.Select(proxy => new ParserWorker(proxy)));
        }
        else
        {
            workers.Add(new ParserWorker());
            //workers.Add(new ParserWorker());
            //workers.Add(new ParserWorker());
        }

        _workersPool = new ParsersFixedPool(workers);
    }

    /// <summary>
    /// Собирает все данные с переданных ссылок в формат Json
    /// </summary>
    /// <param name="links">Список страниц</param>
    /// <returns>Список данных с указанных ссылок в формате Json</returns>
    internal async Task<IEnumerable<JToken?>> ParseLinksAsync(IEnumerable<string> links, CancellationToken cts = default)
    {
        const int linkParsingTimeLimit = 30000;
        var linksCollection = links.ToArray();
        var timeout = linksCollection.Length * linkParsingTimeLimit;
        var stopwatch = Stopwatch.StartNew();
        var semaphore = new SemaphoreSlim(3); // Разрешить до 3 задач одновременно

        _logger?.LogInformation($"Start parsing {linksCollection.Length} links");

        var tasksPool = new List<Task<JToken?>>();

        foreach (var link in linksCollection)
        {
            await semaphore.WaitAsync(cts); // Ожидание доступа к семафору
            tasksPool.Add(Task.Run(async () =>
            {
                try
                {
                    return await ParseLinkAsync(link: link, cts: cts);
                }
                finally
                {
                    semaphore.Release(); // Освобождение семафора после завершения задачи
                }
            }, cts));
        }

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
    public async Task<JToken?> ParseLinkAsync(string link, int tries = 0, CancellationToken cts = default)
    {
        tries++;

        if (tries > 3)
        {
            _logger?.LogWarning($"An error has occurred while parsing link: {link}, tries limit exceeded");
            return null;
        }

        var worker = await _workersPool.GetParserAsync(cts);

        if (worker == null) return null;

        try
        {
            _logger?.LogInformation($"Founded free worker, starts parsing link {link}");
            var json = await worker.GetJsonFromLinkAsync(link).ConfigureAwait(false);

            _logger?.LogInformation($"Parsing Link {link} finished");
            await _workersPool.ReturnParserToPoolAsync(worker);
            return json;
        }
        catch (WorkerBlockedException)
        {
            _logger?.LogWarning($"An error has occurred while parsing link {link}, {worker.Name} is blocked");

            if (!_ignoreIpBlock)
            {
                await _workersPool.ReturnParserToPoolAsync(worker, true);
            }

            return await ParseLinkAsync(link, tries, cts);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"An error has occurred while parsing link: {link}", ex);
            return await ParseLinkAsync(link, tries, cts);
        }
    }

    public void Dispose()
    {
        _workersPool.Dispose();
    }
}