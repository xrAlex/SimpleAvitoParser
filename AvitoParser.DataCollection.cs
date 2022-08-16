using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.DataPaths;
using Parser.Entities;
using Parser.Extensions;

namespace Parser
{
    public sealed partial class AvitoParser
    {
        /// <summary>
        /// Собирает все данные объявлений со страницы
        /// </summary>
        private IEnumerable<Advertisement> GetAdsFromPage(JToken? pageData)
        {
            var items = pageData?.SelectToken("data.catalog.items");
            
            if (items == null) return null;

            var adsCollection = new List<Advertisement>();
            
            foreach (var item in items)
            {
                var data = CollectAdsData(item);
                if (data != null)
                {
                    adsCollection.Add(data);
                }
            }

            return adsCollection;
        }

        /// <summary>
        /// Формирует объявление <see cref="Advertisement"/> но основе информации из Json
        /// </summary>
        private Advertisement? CollectAdsData(JToken adsToken)
        {
            var title = GetTitle(adsToken);
            var link = GetLink(adsToken);
            var identifier = GetIdentifier(adsToken);

            if (title == null || link == null || identifier == null)
            {
                _logger?.LogWarning($"An error occurred when parsing ads data: title '{title}', link: '{link}', identifier {identifier}");
                return null;
            }

            var owner = new User
            (
                traderRating: GetUserRating(adsToken),
                reviewsCount: GetReviewsCount(adsToken),
                closedAds: GetClosedAdsCount(adsToken)
            );

            var ads = new Advertisement
            (
                title: title,
                identifier: identifier.Value,
                cost: GetCost(adsToken),
                link: link,
                location: GetLocation(adsToken),
                owner: owner,
                lastUpdate: GetLastUpdate(adsToken),
                imageLink: GetMainImageLink(adsToken),
                description: GetDescription(adsToken),
                detailedLocation: GetDetailedLocation(adsToken),
                category: GetCategory(adsToken),
                sale: GetSale(adsToken),
                creationTime: GetCreationDate(adsToken),
                city: GetLocation(adsToken)
            );
            return ads;
        }

        /// <summary>
        /// Генерирует коллекцию объявлений из коллекции страниц
        /// </summary>
        private IEnumerable<Advertisement>? GetAdsFromPages(IEnumerable<JToken?> pagesData)
        {
            var adsData = pagesData.AsParallel().SelectMany(GetAdsFromPage);
            var result = new HashSet<Advertisement>(adsData).ToList();

            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Формирует список ссылок на страницы
        /// </summary>
        private IEnumerable<string> GetAllPagesLinks(string baseLink, int? startPage = null, int? endPage = null)
        {
            _logger?.LogInformation($"Form pages links for {baseLink}");
            baseLink = NormalizeLink(baseLink);
            startPage ??= 1;
            var links = new List<string>();
            var delimiter = "?p=";
            int? lastPage = null;

            if (baseLink.Contains('?'))
            {
                delimiter = "&p=";
            }

            if (endPage != null && endPage == 1)
            {
                links.Add($"{baseLink}{delimiter}1");
                _logger?.LogInformation($"Working on {links.Count} pages");
                return links;
            }

            if (_pageValidation)
            {
                lastPage = GetLastPageNumber(baseLink);
            }

            var currentPage = startPage;

            if (endPage == null)
            {
                endPage = lastPage ?? 100;
            }
            else
            {
                if (lastPage != null)
                {
                    if (endPage > lastPage)
                    {
                        endPage = lastPage;
                        _logger?.LogInformation($"Wrong pages range, valid is {startPage}-{endPage}");
                    }
                }
            }

            while (currentPage <= endPage)
            {
                links.Add($"{baseLink}{delimiter}{currentPage}");
                currentPage++;
            }

            _logger?.LogInformation($"Working on {links.Count} pages");

            return links;
        }

        /// <summary>
        /// Нормализует ссылку для корректного парсинга
        /// </summary>
        /// <param name="link"></param>
        private static string NormalizeLink(string link)
        {
            var args = link.Split("?");
            var normalized = args[0] + "?" + "s=104";

            if (args.Length <= 1) return normalized;

            var argsArray = args[1].Split("&");

            foreach (var arg in argsArray)
            {
                if (arg.Contains("p=")) continue;
                if (arg.Contains("s=")) continue;

                normalized += $"&{arg}";
            }

            return normalized;
        }

        /// <summary>
        /// Получает номер последней страницы из начальной ссылки
        /// </summary>
        private int? GetLastPageNumber(string startLink)
        {
            var jsonData = _loader.ParseLinkAsync(startLink).Result;

            if (jsonData == null) return null;

            var adsCount = ParsingEx.GetJsonValue<int?>(jsonData, DataJsonPath.Instance.TotalElements);
            var elementsOnPage = ParsingEx.GetJsonValue<int?>(jsonData, DataJsonPath.Instance.ItemsOnPage);
            var elementsOnMainPage = ParsingEx.GetJsonValue<int?>(jsonData, DataJsonPath.Instance.ItemsOnMainPage);

            if (elementsOnPage == null || adsCount == null || elementsOnMainPage == null) return null;

            if (elementsOnMainPage < elementsOnPage)
            {
                return 1;
            }

            var pages = adsCount / elementsOnPage;
            return pages;
        }
    }
}

