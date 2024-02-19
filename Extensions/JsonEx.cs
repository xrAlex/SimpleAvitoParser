using Newtonsoft.Json;

namespace Parser.Extensions;

internal static class JsonEx
{

    /// <summary>
    /// Сериализует данные объекта в файл
    /// </summary>
    /// <param name="serializationObject">Объект сериализации</param>
    /// <param name="path">Путь сохранения файла</param>
    /// <returns>Результат выполнения операции</returns>
    public static bool TrySaveToFile(object serializationObject, string path)
    {
        try
        {
            var serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented
            };

            using var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs);

            serializer.Serialize(writer, serializationObject);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Десериализует объект из файла и наполняет его
    /// </summary>
    /// <param name="value">Объект который требуется наполнить</param>
    /// <param name="path">Путь к файлу из которого надо наполнить объект</param>
    /// <returns>Результат выполнения операции</returns>
    public static bool TryLoadFromFile(object value, string path)
    {
        try
        {
            var serializer = new JsonSerializer();
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);

            serializer.Populate(reader, value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}