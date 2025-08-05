using System.Text.Json;
using Ledon.BerryNet.Http;
using Ledon.BerryNet.Wasm.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ledon.BerryNet.Wasm.Http;

/// <summary>
/// WASM HTTP客户端工厂接口
/// </summary>
public interface IBerryWasmHttpClientFactory : IBerryHttpClientFactory
{
    /// <summary>
    /// 创建WASM增强的HTTP客户端
    /// </summary>
    new IBerryWasmHttpClient CreateClient();
    
    /// <summary>
    /// 创建命名的WASM增强HTTP客户端
    /// </summary>
    new IBerryWasmHttpClient CreateClient(string name);
}

/// <summary>
/// WASM HTTP客户端工厂实现
/// </summary>
public class BerryWasmHttpClientFactory : IBerryWasmHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<BerryWasmHttpClientOptions> _optionsMonitor;
    private readonly ILogger<BerryWasmHttpClientFactory>? _logger;
    private readonly Dictionary<string, BerryWasmHttpClientOptions> _namedOptions;

    public BerryWasmHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<BerryWasmHttpClientOptions> optionsMonitor,
        ILogger<BerryWasmHttpClientFactory>? logger = null)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        _logger = logger;
        _namedOptions = new Dictionary<string, BerryWasmHttpClientOptions>();
        
        _logger?.LogDebug("BerryWasmHttpClientFactory 已创建");
    }

    /// <summary>
    /// 配置命名客户端选项
    /// </summary>
    public BerryWasmHttpClientFactory ConfigureNamedClient(string name, BerryWasmHttpClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        _namedOptions[name] = options ?? throw new ArgumentNullException(nameof(options));
        _logger?.LogDebug("配置命名客户端: {Name}", name);
        return this;
    }

    public IBerryWasmHttpClient CreateClient()
    {
        return CreateWasmClient("Default");
    }

    public IBerryWasmHttpClient CreateClient(string name)
    {
        return CreateWasmClient(name);
    }

    private IBerryWasmHttpClient CreateWasmClient(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));

        _logger?.LogDebug("创建WASM HTTP客户端: {Name}", name);

        var httpClient = _httpClientFactory.CreateClient(name);
        var options = GetOptions(name);
        
        // 配置HttpClient
        ConfigureHttpClient(httpClient, options);
        
        // 创建JsonSerializerOptions
        var jsonOptions = options.JsonSerializerOptions ?? CreateDefaultJsonOptions();
        
        // 创建基础客户端
        var baseClient = new BerryHttpClient(httpClient, jsonOptions);
        
        // 创建WASM增强客户端
        var wasmClient = new BerryWasmHttpClient(baseClient, _logger as ILogger<BerryWasmHttpClient>);
        
        return wasmClient;
    }

    private BerryWasmHttpClientOptions GetOptions(string name)
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
            return new BerryWasmHttpClientOptions();
        }
    }

    private void ConfigureHttpClient(HttpClient httpClient, BerryWasmHttpClientOptions options)
    {
        _logger?.LogDebug("配置HttpClient，BaseAddress: {BaseAddress}", options.BaseAddress);
        
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
            if (httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value))
            {
                _logger?.LogDebug("添加默认请求头: {Key} = {Value}", header.Key, header.Value);
            }
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

    // 显式实现基础接口
    IBerryHttpClient IBerryHttpClientFactory.CreateClient() => CreateClient();
    IBerryHttpClient IBerryHttpClientFactory.CreateClient(string name) => CreateClient(name);
}
