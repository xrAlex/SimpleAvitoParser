# SimpleAvitoParser
Простой парсер для Avito.ru

Для быстрой проверки работы парсера используйте следующий код:
```csharp

  using var parser = new AvitoParserBuilder()
    .Build();

  var result = await parser.Parse("https://www.avito.ru/velikie_luki/avtomobili?cd=1&radius=200");
        
  foreach (var ads in result)
  {
    Console.WriteLine(ads.Title);
  }

```
Пример использования многопоточного режима парсинга с прокси
```csharp

  var proxy = new List<ProxySettings>();
  proxy.Add(new ProxySettings("31.134.9.78", "8000", "login", "pass"));
  proxy.Add(new ProxySettings("31.134.14.107", "8000", "login", "pass"));
  proxy.Add(new ProxySettings("31.134.3.13", "8000", "login", "pass"));
            
  using var parser = new AvitoParserBuilder()
    .SetProxies(proxy)
    .Build();
                
  var result = await parser.Parse("https://www.avito.ru/velikie_luki/avtomobili?cd=1&radius=200");
        
  foreach (var ads in result)
  {
    Console.WriteLine(ads.Title);
  }

```
