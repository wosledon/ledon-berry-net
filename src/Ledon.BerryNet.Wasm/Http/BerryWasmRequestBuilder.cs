using Ledon.BerryNet.Http;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Wasm.Http;

/// <summary>
/// WASM HTTP请求构建器实现
/// </summary>
public class BerryWasmRequestBuilder : IBerryWasmRequestBuilder
{
    private readonly IBerryHttpRequestBuilder _innerBuilder;
    private readonly ILogger? _logger;
    private string? _corsMode;
    private string? _cacheMode;
    private bool? _includeCredentials;
    private Action<long, long>? _progressCallback;
    private string? _referrerPolicy;
    private bool _streamingResponse;
    private readonly List<string> _compressionEncodings = new();

    public BerryWasmRequestBuilder(IBerryHttpRequestBuilder innerBuilder, ILogger? logger = null)
    {
        _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
        _logger = logger;
    }

    public IBerryHttpRequestBuilder WithUrl(string url) => _innerBuilder.WithUrl(url);
    public IBerryHttpRequestBuilder WithMethod(Ledon.BerryNet.HttpMethod method) => _innerBuilder.WithMethod(method);
    public IBerryHttpRequestBuilder WithHeader(string name, string value) => _innerBuilder.WithHeader(name, value);
    public IBerryHttpRequestBuilder WithAuthentication(string scheme, string token) => _innerBuilder.WithAuthentication(scheme, token);
    public IBerryHttpRequestBuilder WithBearerToken(string token) => _innerBuilder.WithBearerToken(token);
    
    public IBerryWasmRequestBuilder WithJsonBody<T>(T data) 
    { 
        _innerBuilder.WithJsonBody(data);
        return this;
    }

    public IBerryWasmRequestBuilder WithFormBody(Dictionary<string, string> formData)
    {
        _innerBuilder.WithFormBody(formData);
        return this;
    }

    public IBerryWasmRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _innerBuilder.WithTimeout(timeout);
        return this;
    }

    // 显式接口实现
    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithTimeout(TimeSpan timeout)
    {
        return WithTimeout(timeout);
    }

    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithJsonBody<T>(T data)
    {
        return WithJsonBody(data);
    }

    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithFormBody(Dictionary<string, string> formData)
    {
        return WithFormBody(formData);
    }
    
    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithGzipCompression()
    {
        return WithGzipCompression();
    }
    
    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithDeflateCompression()
    {
        return WithDeflateCompression();
    }
    
    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithBrotliCompression()
    {
        return WithBrotliCompression();
    }
    
    IBerryHttpRequestBuilder IBerryHttpRequestBuilder.WithCompression(string encodings)
    {
        return WithCompression(encodings);
    }
    public IBerryHttpRequestBuilder WithStringBody(string content, string mediaType = "text/plain") => _innerBuilder.WithStringBody(content, mediaType);
    public IBerryHttpRequestBuilder WithQueryParameter(string name, string value) => _innerBuilder.WithQueryParameter(name, value);

    public IBerryWasmRequestBuilder WithCorsMode(string mode)
    {
        _corsMode = mode;
        _logger?.LogDebug("设置CORS模式: {Mode}", mode);
        return this;
    }

    public IBerryWasmRequestBuilder WithCacheMode(string mode)
    {
        _cacheMode = mode;
        _logger?.LogDebug("设置缓存模式: {Mode}", mode);
        return this;
    }

    public IBerryWasmRequestBuilder WithCredentials(bool include)
    {
        _includeCredentials = include;
        _logger?.LogDebug("设置凭证包含: {Include}", include);
        return this;
    }

    public IBerryWasmRequestBuilder WithProgressCallback(Action<long, long> progressCallback)
    {
        _progressCallback = progressCallback;
        _logger?.LogDebug("设置进度回调");
        return this;
    }

    public IBerryWasmRequestBuilder WithReferrerPolicy(string policy)
    {
        _referrerPolicy = policy;
        _logger?.LogDebug("设置引用策略: {Policy}", policy);
        return this;
    }

    public IBerryWasmRequestBuilder WithStreamingResponse()
    {
        _streamingResponse = true;
        _logger?.LogDebug("启用流式响应");
        return this;
    }

    public IBerryWasmRequestBuilder WithCompression(params string[] encodings)
    {
        _compressionEncodings.Clear();
        _compressionEncodings.AddRange(encodings);
        _logger?.LogDebug("设置压缩编码: {Encodings}", string.Join(", ", encodings));
        return this;
    }
    
    public IBerryWasmRequestBuilder WithCompression(string encodings)
    {
        var encodingArray = encodings?.Split(',').Select(e => e.Trim()).ToArray() ?? Array.Empty<string>();
        return WithCompression(encodingArray);
    }

    public IBerryWasmRequestBuilder WithGzipCompression()
    {
        return WithCompression("gzip");
    }

    public IBerryWasmRequestBuilder WithBrotliCompression()
    {
        return WithCompression("br");
    }

    public IBerryWasmRequestBuilder WithDeflateCompression()
    {
        return WithCompression("deflate");
    }

    public async Task<IBerryHttpResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("执行WASM HTTP请求");
        
        // 应用WASM特定的配置
        ApplyWasmSpecificHeaders();
        
        try
        {
            var response = await _innerBuilder.ExecuteAsync(cancellationToken);
            
            _logger?.LogInformation("WASM HTTP请求完成，状态码: {StatusCode}", response.StatusCode);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "WASM HTTP请求失败");
            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("执行WASM HTTP请求，期望类型: {Type}", typeof(T).Name);
        
        var response = await ExecuteAsync(cancellationToken);
        
        if (!response.IsSuccessful)
        {
            _logger?.LogWarning("HTTP请求失败: {StatusCode} - {Content}", response.StatusCode, response.Content);
            throw new HttpRequestException($"HTTP请求失败: {response.StatusCode} - {response.Content}");
        }
        
        return response.GetContent<T>();
    }

    private void ApplyWasmSpecificHeaders()
    {
        // 在WASM环境中，某些配置需要通过特定的方式设置
        // 这里我们通过自定义头来传递WASM特定的配置信息
        
        if (!string.IsNullOrEmpty(_corsMode))
        {
            _innerBuilder.WithHeader("X-WASM-CORS-Mode", _corsMode);
        }
        
        if (!string.IsNullOrEmpty(_cacheMode))
        {
            _innerBuilder.WithHeader("X-WASM-Cache-Mode", _cacheMode);
        }
        
        if (_includeCredentials.HasValue)
        {
            _innerBuilder.WithHeader("X-WASM-Credentials", _includeCredentials.Value.ToString().ToLower());
        }
        
        if (!string.IsNullOrEmpty(_referrerPolicy))
        {
            _innerBuilder.WithHeader("X-WASM-Referrer-Policy", _referrerPolicy);
        }
        
        if (_streamingResponse)
        {
            _innerBuilder.WithHeader("X-WASM-Streaming", "true");
        }

        // 设置压缩支持
        if (_compressionEncodings.Count > 0)
        {
            var acceptEncoding = string.Join(", ", _compressionEncodings);
            _innerBuilder.WithHeader("Accept-Encoding", acceptEncoding);
            _logger?.LogDebug("设置Accept-Encoding头: {AcceptEncoding}", acceptEncoding);
        }
    }
}
