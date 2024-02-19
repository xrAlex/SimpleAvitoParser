using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Parser.DataPaths;
using Parser.Entities;
using Parser.Extensions;
using System;

namespace Parser;

public sealed partial class AvitoParser
{
    /// <summary>
    /// Собирает все данные объявлений со страницы
    /// </summary>
    private IEnumerable<Advertisement>? GetAdsFromPage(JToken? pageData)
    {
        var items = pageData?.SelectToken("data.catalog.items");
        return items?.Select(CollectAdsData).OfType<Advertisement>().ToList();
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
            city: GetLocation(adsToken),
            deliveryTime: GetDeliveryTime(adsToken)
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
    /// Генерирует коллекцию объявлений
    /// </summary>
    private IEnumerable<Advertisement>? GetAdsFromPages(JToken? pageData)
    {
        if (pageData == null)
        {
            return null;
        }

        var adsData = GetAdsFromPage(pageData);
        var result = new HashSet<Advertisement>(adsData).ToArray();

        return result.Length> 0 ? result : null;
    }

    /// <summary>
    /// Формирует список ссылок на страницы
    /// </summary>
    private IEnumerable<string> GetAllPagesLinks(string baseLink, int? startPage = null, int? endPage = null)
    {
        _logger?.LogInformation($"Form pages links for {baseLink}");
        baseLink = NormalizeLink(baseLink);

        var links = new List<string>();
        var delimiter = baseLink.Contains('?') ? "&p=" : "?p="; ;
        int? lastPage = null;

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

        var currentPage = startPage ?? 1;

        if (endPage == null)
        {
            endPage = lastPage ?? 100;
        }
        else
        {
            if (endPage > lastPage)
            {
                endPage = lastPage;
                _logger?.LogInformation($"Wrong pages range, valid is {startPage}-{endPage}");
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
    /// TODO https://www.avito.ru/velikie_luki/avtomobili?radius=200&s=104&searchRadius=200 преобразуется в https://www.avito.ru/velikie_luki/avtomobili?s=104, потерян радиус
    private static string NormalizeLink(string link)
    {
        // Разбиваем URL на базовую часть и параметры
        var parts = link.Split('?');
        if (parts.Length < 2) return link; // Если нет параметров, возвращаем исходный URL

        var baseUrl = parts[0];
        var queryString = parts[1];
        var queryParams = queryString.Split('&');

        var resultQueryString = "";
        var sParamFound = false;

        foreach (var param in queryParams)
        {
            if (param.StartsWith("s="))
            {
                if (!sParamFound)
                {
                    // Добавляем параметр s=104 только один раз
                    resultQueryString += "s=104&";
                    sParamFound = true;
                }
            }
            else if (!param.StartsWith("p="))
            {
                // Добавляем все параметры, кроме p=
                resultQueryString += param + "&";
            }
        }

        if (resultQueryString.EndsWith("&"))
        {
            // Убираем последний амперсанд
            resultQueryString = resultQueryString.Remove(resultQueryString.Length - 1);
        }

        // Собираем URL обратно
        return baseUrl + "?" + resultQueryString;
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