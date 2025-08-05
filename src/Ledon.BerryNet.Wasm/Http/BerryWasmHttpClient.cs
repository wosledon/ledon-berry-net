using Ledon.BerryNet.Http;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Wasm.Http;

/// <summary>
/// WASM HTTP客户端实现
/// </summary>
public class BerryWasmHttpClient : IBerryWasmHttpClient
{
    private readonly IBerryHttpClient _innerClient;
    private readonly ILogger<BerryWasmHttpClient>? _logger;

    public BerryWasmHttpClient(IBerryHttpClient innerClient, ILogger<BerryWasmHttpClient>? logger = null)
    {
        _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        _logger = logger;
        
        _logger?.LogDebug("BerryWasmHttpClient 已创建");
    }

    public IBerryHttpRequestBuilder CreateRequest()
    {
        _logger?.LogDebug("创建WASM HTTP请求构建器");
        return new BerryWasmRequestBuilder(_innerClient.CreateRequest(), _logger);
    }

    public IBerryWasmRequestBuilder Get(string url)
    {
        _logger?.LogDebug("创建WASM GET请求: {Url}", url);
        return new BerryWasmRequestBuilder(_innerClient.Get(url), _logger);
    }

    public IBerryWasmRequestBuilder Post(string url)
    {
        _logger?.LogDebug("创建WASM POST请求: {Url}", url);
        return new BerryWasmRequestBuilder(_innerClient.Post(url), _logger);
    }

    public IBerryWasmRequestBuilder Put(string url)
    {
        _logger?.LogDebug("创建WASM PUT请求: {Url}", url);
        return new BerryWasmRequestBuilder(_innerClient.Put(url), _logger);
    }

    public IBerryWasmRequestBuilder Delete(string url)
    {
        _logger?.LogDebug("创建WASM DELETE请求: {Url}", url);
        return new BerryWasmRequestBuilder(_innerClient.Delete(url), _logger);
    }

    public IBerryWasmRequestBuilder Patch(string url)
    {
        _logger?.LogDebug("创建WASM PATCH请求: {Url}", url);
        return new BerryWasmRequestBuilder(_innerClient.Patch(url), _logger);
    }

    public IBerryWasmRequestBuilder Head(string url)
    {
        _logger?.LogDebug("创建WASM HEAD请求: {Url}", url);
        var builder = _innerClient.CreateRequest().WithUrl(url).WithMethod(Ledon.BerryNet.HttpMethod.Head);
        return new BerryWasmRequestBuilder(builder, _logger);
    }

    public IBerryWasmRequestBuilder Options(string url)
    {
        _logger?.LogDebug("创建WASM OPTIONS请求: {Url}", url);
        var builder = _innerClient.CreateRequest().WithUrl(url).WithMethod(Ledon.BerryNet.HttpMethod.Options);
        return new BerryWasmRequestBuilder(builder, _logger);
    }

    // 显式接口实现
    IBerryHttpRequestBuilder IBerryHttpClient.Get(string url) => Get(url);
    IBerryHttpRequestBuilder IBerryHttpClient.Post(string url) => Post(url);
    IBerryHttpRequestBuilder IBerryHttpClient.Put(string url) => Put(url);
    IBerryHttpRequestBuilder IBerryHttpClient.Delete(string url) => Delete(url);
    IBerryHttpRequestBuilder IBerryHttpClient.Patch(string url) => Patch(url);

    public IBerryWasmRequestBuilder WithCorsMode(string mode)
    {
        _logger?.LogDebug("设置全局CORS模式: {Mode}", mode);
        return new BerryWasmRequestBuilder(_innerClient.CreateRequest(), _logger).WithCorsMode(mode);
    }

    public IBerryWasmRequestBuilder WithCacheMode(string mode)
    {
        _logger?.LogDebug("设置全局缓存模式: {Mode}", mode);
        return new BerryWasmRequestBuilder(_innerClient.CreateRequest(), _logger).WithCacheMode(mode);
    }

    public IBerryWasmRequestBuilder WithCredentials(bool include)
    {
        _logger?.LogDebug("设置全局凭证包含: {Include}", include);
        return new BerryWasmRequestBuilder(_innerClient.CreateRequest(), _logger).WithCredentials(include);
    }

    public IBerryWasmRequestBuilder WithProgressCallback(Action<long, long> progressCallback)
    {
        _logger?.LogDebug("设置全局进度回调");
        return new BerryWasmRequestBuilder(_innerClient.CreateRequest(), _logger).WithProgressCallback(progressCallback);
    }
}
