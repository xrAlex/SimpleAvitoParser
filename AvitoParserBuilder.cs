using Microsoft.Extensions.Logging;
using Parser.Entities;

namespace Parser
{
    public class AvitoParserBuilder
    {
        private ILogger? _logger;
        private List<ProxySettings>? _proxies;
        private int _parsingDelay = 5000;
        private string? _sessionId;

        /// <summary>
        /// Устанавливает задержку освобождения клиентов
        /// </summary>
        /// <remarks>Слишком большая задержка замедляет скорость сбора данных, слишком маленькая может вызвать блокировку IP</remarks>
        /// <param name="seconds">Значение задержки в секундах</param>
        public AvitoParserBuilder SetParsingDelay(int seconds)
        {
            _parsingDelay = seconds;
            return this;
        }

        /// <summary>
        /// Позволяет использовать IP клиента для парсинга совместо с Proxy
        /// </summary>
        public AvitoParserBuilder UseHybridMode()
        {
            // TODO: Реализовать
            return this;
        }

        /// <summary>
        /// Устанавливает дял каждого клиента ID сессии Avito
        /// </summary>
        /// <remarks>ID сессии позволяет частично обойти блокировки и получать больше данных со страницы</remarks>
        public AvitoParserBuilder SetSessionId(string id)
        {
            _sessionId = id;
            return this;
        }

        /// <summary>
        /// Устанавливает доступный пул прокси для клиентов парсинга
        /// </summary>
        /// <returns></returns>
        public AvitoParserBuilder SetProxies(List<ProxySettings> proxies)
        {
            _proxies = proxies;
            return this;
        }

        /// <summary>
        /// Производит логирование информации парсера
        /// </summary>
        public AvitoParserBuilder EnableLogs(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public AvitoParser Build()
        {
            return new AvitoParser(_sessionId, _parsingDelay, _logger, _proxies);
        }
    }
}
