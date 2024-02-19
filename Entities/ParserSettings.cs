using Microsoft.Extensions.Logging;

namespace Parser.Entities;

internal class ParserSettings
{
    public ILogger? Logger { get; set; }
    public List<ProxySettings>? Proxies { get; set; }
    public int ParsingDelay { get; set; } = 5000;
    public bool PageValidation { get; set; } = true;
    public bool IgnoreIpBlock { get; set; }
    public bool HybridMode { get; set; }
}