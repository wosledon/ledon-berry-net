using Ledon.BerryNet.Http;
using Microsoft.AspNetCore.Http;

namespace Ledon.BerryNet.AspNetCore.Http;

/// <summary>
/// 增强的HTTP客户端实现，支持AspNetCore特性
/// </summary>
public class BerryHttpClientEnhanced : IBerryHttpClientEnhanced
{
    private readonly IBerryHttpClient _innerClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BerryHttpClientEnhanced(IBerryHttpClient innerClient, IHttpContextAccessor httpContextAccessor)
    {
        _innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public IBerryHttpRequestBuilder CreateRequest()
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.CreateRequest());
    }

    public IBerryHttpRequestBuilder Get(string url)
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.Get(url));
    }

    public IBerryHttpRequestBuilder Post(string url)
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.Post(url));
    }

    public IBerryHttpRequestBuilder Put(string url)
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.Put(url));
    }

    public IBerryHttpRequestBuilder Delete(string url)
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.Delete(url));
    }

    public IBerryHttpRequestBuilder Patch(string url)
    {
        return new BerryHttpRequestBuilderEnhanced(_innerClient.Patch(url));
    }

    public IBerryHttpRequestBuilder PropagateHeaders(HttpContext httpContext, params string[] headerNames)
    {
        var builder = new BerryHttpRequestBuilderEnhanced(_innerClient.CreateRequest());
        return builder.PropagateHeaders(httpContext, headerNames);
    }

    public IBerryHttpRequestBuilder PropagateAuthentication(HttpContext httpContext)
    {
        var builder = new BerryHttpRequestBuilderEnhanced(_innerClient.CreateRequest());
        return builder.PropagateAuthentication(httpContext);
    }

    public IBerryHttpRequestBuilder PropagateCorrelationId(HttpContext httpContext)
    {
        var builder = new BerryHttpRequestBuilderEnhanced(_innerClient.CreateRequest());
        return builder.PropagateCorrelationId(httpContext);
    }
}
