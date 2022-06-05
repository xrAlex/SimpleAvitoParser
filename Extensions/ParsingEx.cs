using Newtonsoft.Json.Linq;

namespace Parser.Extensions
{
    internal static class ParsingEx
    {
        /// <summary>
        /// Возвращает числовые значения из строки
        /// </summary>
        public static int? GetOnlyNumberFromText(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var normalizedText = string.Concat(text.Where(char.IsDigit));

            if (int.TryParse(normalizedText, out var number))
            {
                return number;
            }

            return null;
        }

        /// <summary>
        /// Получает значение поля Json
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="json">Json объект в котором ищется значение</param>
        /// <param name="paths">Список Json путей в которых изется значение</param>
        /// <returns>Первое найденное значение Json</returns>
        public static T? GetJsonValue<T>(JToken? json, IEnumerable<string> paths)
        {
            if (json == null) return default;

            foreach (var path in paths)
            {
                var target = json.SelectToken(path, false);

                if (target != null)
                {
                    var value = target.Value<T>();

                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return default;
        }
    }
}
