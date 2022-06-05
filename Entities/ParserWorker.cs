using System.Web;
using Newtonsoft.Json.Linq;
using Parser.DataLoader;
using Parser.DataPaths;
using Parser.Extensions;

namespace Parser.Entities
{
    internal sealed class ParserWorker : IAsyncDisposable
    {
        private BrowserClient Parser { get; }
        public string Name { get; }

        /// <summary>
        /// Получает данные Json по указанной ссылке
        /// </summary>
        public async Task<JToken?> GetJsonFromLinkAsync(string link)
        {
            try
            {
                var scripts = await Parser.GetScriptsFormPageAsync(link).ConfigureAwait(false);

                if (scripts == null) return null;

                var pageInitialData = GetPageInitialData(scripts);
                var pageDataJson = DecodeJson(pageInitialData);

                if (pageDataJson == null) return null;

                var json = GetPageInitialDataToken(pageDataJson);
                return json;
            }
            catch (WorkerBlockedException)
            {
                throw;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Ищет среди скриптов скрипт содержащий данные инициализации страницы
        /// </summary>
        /// <param name="scripts"></param>
        /// <returns></returns>
        private static string? GetPageInitialData(IEnumerable<object> scripts)
        {
            return scripts
                .AsParallel()
                .Select(script => script.ToString())
                .Where(scriptText => !string.IsNullOrEmpty(scriptText))
                .FirstOrDefault(scriptText => scriptText!
                    .Contains(DataJsonPath.Instance.PageScript));
        }

        /// <summary>
        /// Декодирует и сериализует строку в <see cref="JObject"/>
        /// </summary>
        private static JObject? DecodeJson(string? rawValue)
        {
            try
            {
                var normalizedValue = rawValue?
                    .Split(";")[0]
                    .Split("=")[1]
                    .Trim()
                    .Trim('"');

                var decodedData = HttpUtility.UrlDecode(normalizedValue);
                var jsonObject = JObject.Parse(decodedData);

                return jsonObject;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Ищет в <see cref="JObject"/> строку инициализации страницы
        /// </summary>
        /// <param name="json"></param>
        /// <returns>В случае успеха возвращает <see cref="JToken"/> скрипта инициализации страницы</returns>
        private static JToken? GetPageInitialDataToken(JObject json)
        {
            foreach (var (key, value) in json)
            {
                if (!key.Contains("single-page")) continue;

                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        public ParserWorker(ProxySettings? proxy = null, string? sessionId = null)
        {
            Parser = new BrowserClient(proxy, sessionId);
            Name = proxy == null ? "User worker" : $"Proxy worker [{proxy.Ip}:{proxy.Port}]";
        }

        public async ValueTask DisposeAsync()
        {
            await Parser.DisposeAsync();
        }
    }
}
