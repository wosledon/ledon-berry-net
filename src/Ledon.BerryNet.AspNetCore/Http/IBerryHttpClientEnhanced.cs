using Ledon.BerryNet.Http;
using Microsoft.AspNetCore.Http;

namespace Ledon.BerryNet.AspNetCore.Http;

/// <summary>
/// 增强的HTTP客户端接口，支持AspNetCore特性
/// </summary>
public interface IBerryHttpClientEnhanced : IBerryHttpClient
{
    /// <summary>
    /// 从当前HTTP上下文传递请求头
    /// </summary>
    IBerryHttpRequestBuilder PropagateHeaders(HttpContext httpContext, params string[] headerNames);
    
    /// <summary>
    /// 从当前HTTP上下文传递认证信息
    /// </summary>
    IBerryHttpRequestBuilder PropagateAuthentication(HttpContext httpContext);
    
    /// <summary>
    /// 从当前HTTP上下文传递相关ID（如TraceId）
    /// </summary>
    IBerryHttpRequestBuilder PropagateCorrelationId(HttpContext httpContext);
}

/// <summary>
/// 增强的HTTP请求构建器接口，支持AspNetCore特性
/// </summary>
public interface IBerryHttpRequestBuilderEnhanced : IBerryHttpRequestBuilder
{
    /// <summary>
    /// 传递指定的请求头
    /// </summary>
    IBerryHttpRequestBuilderEnhanced PropagateHeaders(HttpContext httpContext, params string[] headerNames);
    
    /// <summary>
    /// 传递认证信息
    /// </summary>
    IBerryHttpRequestBuilderEnhanced PropagateAuthentication(HttpContext httpContext);
    
    /// <summary>
    /// 传递相关ID
    /// </summary>
    IBerryHttpRequestBuilderEnhanced PropagateCorrelationId(HttpContext httpContext);
    
    /// <summary>
    /// 传递用户信息
    /// </summary>
    IBerryHttpRequestBuilderEnhanced PropagateUser(HttpContext httpContext);
}

/// <summary>
/// 增强的HTTP请求构建器实现
/// </summary>
public class BerryHttpRequestBuilderEnhanced : IBerryHttpRequestBuilderEnhanced
{
    private readonly IBerryHttpRequestBuilder _innerBuilder;

    public BerryHttpRequestBuilderEnhanced(IBerryHttpRequestBuilder innerBuilder)
    {
        _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
    }

    public IBerryHttpRequestBuilder WithUrl(string url) => _innerBuilder.WithUrl(url);
    public IBerryHttpRequestBuilder WithMethod(Ledon.BerryNet.HttpMethod method) => _innerBuilder.WithMethod(method);
    public IBerryHttpRequestBuilder WithHeader(string name, string value) => _innerBuilder.WithHeader(name, value);
    public IBerryHttpRequestBuilder WithAuthentication(string scheme, string token) => _innerBuilder.WithAuthentication(scheme, token);
    public IBerryHttpRequestBuilder WithBearerToken(string token) => _innerBuilder.WithBearerToken(token);
    public IBerryHttpRequestBuilder WithJsonBody<T>(T data) => _innerBuilder.WithJsonBody(data);
    public IBerryHttpRequestBuilder WithStringBody(string content, string mediaType = "text/plain") => _innerBuilder.WithStringBody(content, mediaType);
    public IBerryHttpRequestBuilder WithFormBody(Dictionary<string, string> formData) => _innerBuilder.WithFormBody(formData);
    public IBerryHttpRequestBuilder WithQueryParameter(string name, string value) => _innerBuilder.WithQueryParameter(name, value);
    public IBerryHttpRequestBuilder WithTimeout(TimeSpan timeout) => _innerBuilder.WithTimeout(timeout);
    public IBerryHttpRequestBuilder WithGzipCompression() => _innerBuilder.WithGzipCompression();
    public IBerryHttpRequestBuilder WithDeflateCompression() => _innerBuilder.WithDeflateCompression();
    public IBerryHttpRequestBuilder WithBrotliCompression() => _innerBuilder.WithBrotliCompression();
    public IBerryHttpRequestBuilder WithCompression(string encodings) => _innerBuilder.WithCompression(encodings);
    public Task<IBerryHttpResponse> ExecuteAsync(CancellationToken cancellationToken = default) => _innerBuilder.ExecuteAsync(cancellationToken);
    public Task<T> ExecuteAsync<T>(CancellationToken cancellationToken = default) => _innerBuilder.ExecuteAsync<T>(cancellationToken);

    public IBerryHttpRequestBuilderEnhanced PropagateHeaders(HttpContext httpContext, params string[] headerNames)
    {
        if (httpContext?.Request?.Headers == null)
            return this;

        foreach (var headerName in headerNames)
        {
            if (httpContext.Request.Headers.TryGetValue(headerName, out var headerValue))
            {
                _innerBuilder.WithHeader(headerName, headerValue.ToString());
            }
        }

        return this;
    }

    public IBerryHttpRequestBuilderEnhanced PropagateAuthentication(HttpContext httpContext)
    {
        if (httpContext?.Request?.Headers == null)
            return this;

        if (httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _innerBuilder.WithHeader("Authorization", authHeader.ToString());
        }

        return this;
    }

    public IBerryHttpRequestBuilderEnhanced PropagateCorrelationId(HttpContext httpContext)
    {
        if (httpContext?.Request?.Headers == null)
            return this;

        // 尝试获取常见的相关ID头
        var correlationHeaders = new[] { "X-Correlation-ID", "X-Request-ID", "X-Trace-ID", "Request-Id" };
        
        foreach (var header in correlationHeaders)
        {
            if (httpContext.Request.Headers.TryGetValue(header, out var value))
            {
                _innerBuilder.WithHeader(header, value.ToString());
                break; // 只传递第一个找到的
            }
        }

        // 如果没有找到，生成一个新的
        if (!correlationHeaders.Any(h => httpContext.Request.Headers.ContainsKey(h)))
        {
            _innerBuilder.WithHeader("X-Correlation-ID", Guid.NewGuid().ToString());
        }

        return this;
    }

    public IBerryHttpRequestBuilderEnhanced PropagateUser(HttpContext httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst("sub")?.Value ?? 
                        httpContext.User.FindFirst("id")?.Value ?? 
                        httpContext.User.Identity.Name;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _innerBuilder.WithHeader("X-User-ID", userId);
            }
        }

        return this;
    }
}
