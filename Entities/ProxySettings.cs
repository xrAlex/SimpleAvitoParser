namespace Parser.Entities;

public sealed class ProxySettings
{
    /// <summary>
    /// IP адрес прокси
    /// </summary>
    public string Ip { get; }

    /// <summary>
    /// PORT прокси
    /// </summary>
    public string Port { get; }

    /// <summary>
    /// Логин пркоси
    /// </summary>
    public string? Login { get; }

    /// <summary>
    /// Пароль прокси
    /// </summary>
    public string? Pass { get; }

    public ProxySettings(string ip, string port, string? login = null, string? pass = null)
    {
        Ip = ip;
        Port = port;
        Login = login;
        Pass = pass;
    }
}