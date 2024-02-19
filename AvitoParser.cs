using Microsoft.Extensions.Logging;
using Parser.Entities;

namespace Parser;

public sealed partial class AvitoParser : IDisposable
{
    private readonly ILogger? _logger;
    private readonly DataLoader.DataLoader _loader;
    private readonly bool _pageValidation;
    public event EventHandler<Tuple<IEnumerable<Advertisement>?, object?>>? LinkParsed;

    internal AvitoParser(ParserSettings settings)
    {
        _pageValidation = settings.PageValidation;
        _logger = settings.Logger;
        _loader = new DataLoader.DataLoader(settings.Proxies, _logger,
            settings.ParsingDelay, settings.HybridMode, settings.IgnoreIpBlock);
    }

    /// <summary>
    /// Собирает данные о всех объявления по указанной ссылке
    /// </summary>
    /// <param name="link">Ссылка на Avito.ru с настроенными параметрами поиска</param>
    /// <param name="startPage">Номер страницы с которой начинать парсинг</param>
    /// <param name="endPage">Номер конечной страницы парсинга</param>
    /// <remarks>
    /// Если не указан номер конечной страницы парсинга, то парсинг будет производится до конца списка объявлений
    /// </remarks>
    /// <returns>Результат парсинга предсталвенный списком <see cref="Advertisement"/></returns>
    public async Task<IEnumerable<Advertisement>?> ParseLinkAsync(string link, int? startPage = null, int? endPage = null, CancellationToken cts = default)
    {
        var pages = GetAllPagesLinks(link, startPage, endPage);
        var data = await _loader.ParseLinksAsync(pages, cts).ConfigureAwait(false);
        var ads = GetAdsFromPages(data);
        return ads;
    }

    public async Task<IEnumerable<Advertisement>?> ParseLinkAsync(string link, bool normalizeLinks = true, CancellationToken cts = default)
    {
        if (normalizeLinks)
        {
            link = NormalizeLink(link);
        }

        var data = await _loader.ParseLinkAsync(link, cts: cts).ConfigureAwait(false);
        var ads = GetAdsFromPages(data);
        return ads;
    }

    public async Task<IEnumerable<Advertisement>?> ParseLinksAsync(IEnumerable<string> links, bool normalizeLinks = true, CancellationToken cts = default)
    {
        links = normalizeLinks 
            ? links.Select(NormalizeLink) 
            : links;
            
        var data = await _loader.ParseLinksAsync(links, cts).ConfigureAwait(false);
        var ads = GetAdsFromPages(data);
        return ads;
    }

    public void Dispose()
    {
        _loader.Dispose();
    }
}