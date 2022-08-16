using Parser.Extensions;

namespace Parser.DataPaths
{
    public class DataJsonPath
    {
        public static DataJsonPath Instance { get; } = LoadOrDefault();

        public string PageScript { get; } = "window.__initialData__";

        public string[] Title { get; } =
        {
            "iva.TitleStep[0].payload.title",
            "title"
        };

        public string[] MainImageLink { get;} =
        {
            "images[0].catalog_vip",
            "images[0].catalog"
        };

        public string[] Cost { get; } =
        {
            "iva.PriceStep[0].payload.priceDetailed.value",
            "priceDetailed.value"
        };

        public string[] Sale { get; } =
        {
            "iva.PriceStep[0].payload.priceDetailed.wasLowered",
        };

        public string[] Description { get; } =
        {
            "iva.DescriptionStep[0].payload.description",
            "description",
        };

        public string[] Category { get; } =
        {
            "iva.FirstLineStep[0].payload.value",
            "category.name"
        };

        public string[] LastUpdate { get; } =
        {
            "iva.DateInfoStep[0].payload.relative"
        };

        public string[] CreationTime { get; } =
        {
            "sortTimeStamp"
        };

        public string[] ReviewsCount { get; } =
        {
            "rating.summary"
        };

        public string[] UserRating { get; } =
        {
            "rating.score"
        };

        public string[] ClosedAds { get; } =
        {
            "closedItemsText"
        };

        public string[] LocationNode { get; } =
        {
            "iva.GeoStep[0].payload.geoForItems.addressLocality",
            "location.name",
        };

        public string[] LocationDetailed { get; } =
        {
            "iva.GeoStep[0].payload.geoForItems.formattedAddress",
            "geo.geoReferences[0].content"
        };

        public string[] AdsId { get; } =
        {
            "id",
            "iva.DescriptionStep[0].payload.debug.id",
        };

        public string[] Link { get; } =
        {
            "iva.DescriptionStep[0].payload.urlPath",
            "iva.TitleStep[0].payload.urlPath",
            "urlPath"
        };

        public string[] TotalElements { get; } =
        {
            "data.totalElements"
        };

        public string[] ItemsOnPage { get; } =
        {
            "data.itemsOnPage"
        };

        public string[] ItemsOnMainPage { get; } =
        {
            "data.itemsOnPageMainSection"
        };

        public static DataJsonPath LoadOrDefault()
        {
            var path = ".\\Paths.json";
            var instance = new DataJsonPath();
            var dataLoaded = JsonEx.TryLoadFromFile(instance, path);

            if (!dataLoaded)
            {
                JsonEx.TrySaveToFile(instance, path);
            }

            return instance;
        }

        private DataJsonPath(){}
    }
}
