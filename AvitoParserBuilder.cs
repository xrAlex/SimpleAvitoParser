using Microsoft.Extensions.Logging;
using Parser.Entities;

namespace Parser
{
    public class AvitoParserBuilder
    {
        private readonly ParserSettings _settings = new ();
        /// <summary>
        /// Устанавливает задержку освобождения клиентов
        /// </summary>
        /// <remarks>Слишком большая задержка замедляет скорость сбора данных, слишком маленькая может вызвать блокировку IP</remarks>
        /// <param name="seconds">Значение задержки в секундах</param>
        public AvitoParserBuilder SetParsingDelay(int seconds)
        {
            _settings.ParsingDelay = seconds;
            return this;
        }

        /// <summary>
        /// Позволяет использовать IP клиента для парсинга совместо с Proxy
        /// </summary>
        public AvitoParserBuilder UseHybridMode()
        {
            _settings.HybridMode = true;
            return this;
        }

        /// <summary>
        /// Устанавливает доступный пул прокси для клиентов парсинга
        /// </summary>
        public AvitoParserBuilder SetProxies(List<ProxySettings> proxies)
        {
            _settings.Proxies = proxies;
            return this;
        }

        /// <summary>
        /// Парсер не будет проверять заблокирован ли клиент по IP
        /// </summary>
        public AvitoParserBuilder IgnoreIpBlock()
        {
            _settings.IgnoreIpBlock = true;
            return this;
        }

        /// <summary>
        /// Отключает проверку доступных для парсинга страниц
        /// </summary>
        public AvitoParserBuilder DisablePagesRangeValidation()
        {
            _settings.PageValidation = false;
            return this;
        }

        /// <summary>
        /// Активирует логирование парсинга
        /// </summary>
        public AvitoParserBuilder EnableLogs(ILogger logger)
        {
            _settings.Logger = logger;
            return this;
        }

        public AvitoParser Build()
        {
            return new AvitoParser(_settings);
        }
    }
}
