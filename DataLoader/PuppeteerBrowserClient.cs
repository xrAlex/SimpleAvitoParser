using Parser.Entities;
using Parser.Extensions;
using PuppeteerSharp;

namespace Parser.DataLoader
{
    internal sealed class BrowserClient : IAsyncDisposable
    {
        private readonly Browser _client;
        private readonly Credentials? _proxyCredentials;
        private readonly CookieParam? _sessionId;

        public BrowserClient(ProxySettings? proxy = null, string? sessionId = null)
        {
            using var browserFetcher = new BrowserFetcher();
            var download = browserFetcher.DownloadAsync().Result;
   
            var launchArgs = new List<string>
            {
                "--no-sandbox",
                "--disable-infobars",
                "--disable-setuid-sandbox",
                "--ignore-certificate-errors",
                "--disable-background-mode",
                "--disable-extensions",
                "--disable-plugins",
                "--disable-plugins-discovery",
                "--disable-notifications",
                "--disable-translate",
                "--mute-audio",
                "--no-referrers"
            };

            if (proxy != null)
            {
                launchArgs.Add($"--proxy-server={proxy.Ip}:{proxy.Port}");

                _proxyCredentials = new Credentials()
                {
                    Username = proxy.Login,
                    Password = proxy.Pass
                };
            }

            if (sessionId != null)
            {
                _sessionId = new CookieParam
                {
                    Domain = ".avito.ru",
                    Name = "sessid",
                    Value = sessionId,
                    HttpOnly = true
                };
            }

            _client = Puppeteer.LaunchAsync(
                new LaunchOptions
                {
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    Args = launchArgs.ToArray()
                }).Result;
        }

        /// <summary>
        /// Возвращает все скрипты со страницы
        /// </summary>
        public async Task<List<object>?> GetScriptsFormPageAsync(string link)
        {
            var page = await _client.NewPageAsync().ConfigureAwait(false);

            try
            {
                await page.SetUserAgentAsync(GetRandomAgent()).ConfigureAwait(false);

                if (_proxyCredentials != null)
                {
                    await page.AuthenticateAsync(_proxyCredentials).ConfigureAwait(false);
                }

                if (_sessionId != null)
                {
                    await page.SetCookieAsync(_sessionId).ConfigureAwait(false);
                }

                await page.GoToAsync(link).ConfigureAwait(false);
                await ThrowWhenClientBlocked(page);
                var data = await GetScriptsData(page);
                await page.CloseAsync().ConfigureAwait(false);
                return data;
            }
            catch
            {
                await page.CloseAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// При блокировке клиента выбрасывает исключение
        /// </summary>
        /// <returns></returns>
        private static async Task ThrowWhenClientBlocked(Page page)
        {
            var blockingElement = await page.QuerySelectorAsync("body > div > div > h1");

            if (blockingElement != null)
            {
                var innerTextHandle = await blockingElement.GetPropertyAsync("innerText");
                var innerText = await innerTextHandle.JsonValueAsync<string>();

                if (innerText.Contains("Доступ ограничен"))
                {
                    throw new WorkerBlockedException();
                }
            }
        }

        /// <summary>
        /// Получает содержимое всех скриптов со страницы
        /// </summary>
        private static async Task<List<object>?> GetScriptsData(Page page)
        {
            var scripts = await page.QuerySelectorAllAsync("script");
            var scriptsData = new List<object>();

            if (scripts == null) return null;

            foreach (var script in scripts)
            {
                var innerTextHandle = await script.GetPropertyAsync("innerText");
                var innerText = await innerTextHandle.JsonValueAsync<string>();
                scriptsData.Add(innerText);
            }

            return scriptsData;
        }

        /// <summary>
        /// Возращает случайный User Agent из списка
        /// </summary>
        private static string GetRandomAgent()
        {
            string[] clients =
            {
                "Mozilla / 5.0(Windows NT 6.1; Win64; x64; rv: 47.0) Gecko / 20100101 Firefox / 47.0",
                "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 92.0.4515.131 Safari / 537.36",
                "Mozilla / 5.0(Windows NT 10.0; WOW64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 92.0.4515.131 Safari / 537.36",
                "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 90.0) Gecko / 20100101 Firefox / 90.0",
                "Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 78.0) Gecko / 20100101 Firefox / 78.0"
            };

            return clients[new Random().Next(0, clients.Length)];
        }

        public async ValueTask DisposeAsync()
        {
           await _client.CloseAsync();
           await _client.DisposeAsync();
        }
    }
}
