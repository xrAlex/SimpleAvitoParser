using Newtonsoft.Json.Linq;
using Parser.DataPaths;
using Parser.Extensions;

namespace Parser;

public sealed partial class AvitoParser
{
    private static string? GetLocation(JToken page) 
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.LocationNode);

    private static string? GetTitle(JToken page) 
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.Title);

    private static string? GetLink(JToken page) 
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.Link);

    private static long? GetIdentifier(JToken page) 
        => ParsingEx.GetJsonValue<long?>(page, DataJsonPath.Instance.AdsId);

    private static string? GetMainImageLink(JToken page) 
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.MainImageLink);

    private static int? GetCost(JToken page) 
        => ParsingEx.GetJsonValue<int?>(page, DataJsonPath.Instance.Cost);

    private static bool? GetSale(JToken page) 
        => ParsingEx.GetJsonValue<bool?>(page, DataJsonPath.Instance.Sale);

    private static string? GetCategory(JToken page) 
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.Category);

    private static string? GetDescription(JToken page)
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.Description);

    private static string? GetLastUpdate(JToken page)
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.LastUpdate);

    private static DateTime? GetCreationDate(JToken page)
    {
        var value = ParsingEx.GetJsonValue<long?>(page, DataJsonPath.Instance.CreationTime);
        return DateTimeEx.GetTimeFromUnix(value);
    }

    private static string? GetDetailedLocation(JToken page)
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.LocationDetailed);

    private static double? GetUserRating(JToken page)
        => ParsingEx.GetJsonValue<double?>(page, DataJsonPath.Instance.UserRating);

    private static string? GetDeliveryTime(JToken page)
        => ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.Delivery);

    
    private static int? GetClosedAdsCount(JToken page)
    {
        var value = ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.ClosedAds);
        return ParsingEx.GetOnlyNumberFromText(value);
    }

    private static int? GetReviewsCount(JToken page)
    {
        var value = ParsingEx.GetJsonValue<string?>(page, DataJsonPath.Instance.ReviewsCount);
        return ParsingEx.GetOnlyNumberFromText(value);
    }
}