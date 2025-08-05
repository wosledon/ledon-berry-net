using System.Text.Json;
using Ledon.BerryNet.AspNetCore.Options;
using Ledon.BerryNet.Http;
using Microsoft.Extensions.Options;

namespace Ledon.BerryNet.AspNetCore.Http;

/// <summary>
/// 增强的HTTP客户端工厂，支持AspNetCore配置
/// </summary>
public class BerryHttpClientFactoryEnhanced : IBerryHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<BerryHttpClientOptions> _optionsMonitor;
    private readonly Dictionary<string, BerryHttpClientOptions> _namedOptions;

    public BerryHttpClientFactoryEnhanced(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<BerryHttpClientOptions> optionsMonitor)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _namedOptions = new Dictionary<string, BerryHttpClientOptions>();
    }

    /// <summary>
    /// 配置命名客户端选项
    /// </summary>
    public BerryHttpClientFactoryEnhanced ConfigureNamedClient(string name, BerryHttpClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        _namedOptions[name] = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    public IBerryHttpClient CreateClient()
    {
        return CreateClient("Default");
    }

    public IBerryHttpClient CreateClient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));

        var httpClient = _httpClientFactory.CreateClient(name);
        var options = GetOptions(name);
        
        // 配置HttpClient
        ConfigureHttpClient(httpClient, options);
        
        // 创建JsonSerializerOptions
        var jsonOptions = options.JsonSerializerOptions ?? CreateDefaultJsonOptions();
        
        return new BerryHttpClient(httpClient, jsonOptions);
    }

    private BerryHttpClientOptions GetOptions(string name)
    {
        // 首先检查命名选项
        if (_namedOptions.TryGetValue(name, out var namedOptions))
        {
            return namedOptions;
        }
        
        // 然后检查IOptionsMonitor
        try
        {
            return _optionsMonitor.Get(name);
        }
        catch
        {
            // 如果获取失败，返回默认选项
            return new BerryHttpClientOptions();
        }
    }

    private static void ConfigureHttpClient(HttpClient httpClient, BerryHttpClientOptions options)
    {
        // 设置基础地址
        if (!string.IsNullOrWhiteSpace(options.BaseAddress))
        {
            httpClient.BaseAddress = new Uri(options.BaseAddress);
        }
        
        // 设置超时时间
        if (options.Timeout.HasValue)
        {
            httpClient.Timeout = options.Timeout.Value;
        }
        
        // 添加默认请求头
        foreach (var header in options.DefaultHeaders)
        {
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
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
