using System.Net;
using System.Text.Json;

namespace Ledon.BerryNet.Http;

/// <summary>
/// HTTP客户端工厂接口
/// </summary>
public interface IBerryHttpClientFactory
{
    /// <summary>
    /// 创建HTTP客户端
    /// </summary>
    IBerryHttpClient CreateClient();
    
    /// <summary>
    /// 创建命名的HTTP客户端
    /// </summary>
    IBerryHttpClient CreateClient(string name);
}

/// <summary>
/// HTTP客户端工厂实现
/// </summary>
public class BerryHttpClientFactory : IBerryHttpClientFactory
{
    private readonly JsonSerializerOptions _defaultJsonOptions;
    private readonly Dictionary<string, JsonSerializerOptions> _namedJsonOptions;
    private readonly Dictionary<string, Func<HttpClient>> _namedClientFactories;

    /// <summary>
    /// 构造函数
    /// </summary>
    public BerryHttpClientFactory(JsonSerializerOptions? defaultJsonOptions = null)
    {
        _defaultJsonOptions = defaultJsonOptions ?? CreateDefaultJsonOptions();
        _namedJsonOptions = new Dictionary<string, JsonSerializerOptions>();
        _namedClientFactories = new Dictionary<string, Func<HttpClient>>();
    }

    /// <summary>
    /// 为命名客户端配置JSON选项
    /// </summary>
    public BerryHttpClientFactory ConfigureJsonOptions(string name, JsonSerializerOptions jsonOptions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        _namedJsonOptions[name] = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        return this;
    }

    /// <summary>
    /// 为命名客户端配置HttpClient工厂
    /// </summary>
    public BerryHttpClientFactory ConfigureClient(string name, Func<HttpClient> clientFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        _namedClientFactories[name] = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        return this;
    }

    public IBerryHttpClient CreateClient()
    {
        return new BerryHttpClient(_defaultJsonOptions);
    }
    
    /// <summary>
    /// 创建支持压缩的HTTP客户端
    /// </summary>
    public IBerryHttpClient CreateClientWithCompression()
    {
        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        var httpClient = new HttpClient(handler);
        return new BerryHttpClient(httpClient, _defaultJsonOptions);
    }
    
    /// <summary>
    /// 创建命名的支持压缩的HTTP客户端
    /// </summary>
    public IBerryHttpClient CreateClientWithCompression(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));

        var jsonOptions = _namedJsonOptions.TryGetValue(name, out var options) ? options : _defaultJsonOptions;
        
        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        };
        var httpClient = new HttpClient(handler);
        return new BerryHttpClient(httpClient, jsonOptions);
    }

    public IBerryHttpClient CreateClient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));

        var jsonOptions = _namedJsonOptions.TryGetValue(name, out var options) ? options : _defaultJsonOptions;
        
        if (_namedClientFactories.TryGetValue(name, out var clientFactory))
        {
            var httpClient = clientFactory();
            return new BerryHttpClient(httpClient, jsonOptions);
        }
        
        return new BerryHttpClient(jsonOptions);
    }

    private static JsonSerializerOptions CreateDefaultJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }
}
