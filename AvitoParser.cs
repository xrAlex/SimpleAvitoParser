using Parser.Entities;
using xrAsyncLogger;

namespace Parser
{
    public sealed partial class AvitoParser : IAsyncDisposable
    {
        private readonly Logger? _logger;
        private readonly DataLoader.DataLoader _loader;
        public event EventHandler<IEnumerable<Advertisement>?>? ParsingFinished;

        internal AvitoParser(string? sessionId, int parsingDelay,
            Logger? logger = null, IEnumerable<ProxySettings>? proxies = null)
        {
            _logger = logger;
            _loader = new DataLoader.DataLoader(proxies, _logger, sessionId);
        }

        /// <summary>
        /// Собирает данные о всех объявления по указанной ссылке
        /// </summary>
        /// <param name="link">Ссылка на Avito.ru с настроенными параметрами поиска</param>
        /// <param name="startPage">Номер страницы с которой начинать парсинг</param>
        /// <param name="endPage">Номер конечной страницы парсинга</param>
        /// <remarks>
        /// Если не указан номер конечной страницы парсинга, то парсинг будет производится до конца списка объявлений,
        /// по завершении работы метода вызывается событие <see cref="ParsingFinished"/>
        /// </remarks>
        /// <returns>Результат парсинга предсталвенный списком <see cref="Advertisement"/></returns>
        public async Task<IEnumerable<Advertisement>?> Parse(string link, int? startPage = null, int? endPage = null, CancellationToken cts = new())
        {
            var pages = GetAllPagesLinks(link, startPage, endPage);
            var data = await _loader.ParseLinksAsync(pages, cts).ConfigureAwait(false);
            var ads = GetAdsFromPages(data);
            ParsingFinished?.Invoke(this, ads);
            return ads;
        }

        public async ValueTask DisposeAsync()
        {
            _logger?.Dispose();
            await _loader.DisposeAsync();
        }
    }
}
