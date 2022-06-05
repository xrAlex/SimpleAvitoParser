namespace Parser.Extensions
{
    internal static class DateTimeEx
    {
        /// <summary>
        /// Получает дату из UNIX формата
        /// </summary>
        public static DateTime? GetTimeFromUnix(long? unixTimeStamp)
        {
            try
            {
                if (unixTimeStamp == null) return null;

                var strTimeStamp = unixTimeStamp.ToString();
                var normalizedStamp = strTimeStamp?[..^3];
                var date = new DateTime(1970, 1, 1, 0, 0, 0, 0);

                return date.AddSeconds(Convert.ToDouble(normalizedStamp));
            }
            catch
            {
                return null;
            }
        }
    }
}
