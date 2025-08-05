using System.Text.Json;

namespace Ledon.BerryNet.Http;

/// <summary>
/// HTTP客户端实现
/// </summary>
public class BerryHttpClient : IBerryHttpClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly bool _disposeHttpClient;

    /// <summary>
    /// 构造函数 - 创建新的HttpClient实例
    /// </summary>
    public BerryHttpClient(JsonSerializerOptions? jsonOptions = null)
    {
        _httpClient = new HttpClient();
        _jsonOptions = jsonOptions ?? CreateDefaultJsonOptions();
        _disposeHttpClient = true;
    }

    /// <summary>
    /// 构造函数 - 使用现有的HttpClient实例
    /// </summary>
    public BerryHttpClient(HttpClient httpClient, JsonSerializerOptions? jsonOptions = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = jsonOptions ?? CreateDefaultJsonOptions();
        _disposeHttpClient = false;
    }

    public IBerryHttpRequestBuilder CreateRequest()
    {
        return new BerryHttpRequestBuilder(_httpClient, _jsonOptions);
    }

    public IBerryHttpRequestBuilder Get(string url)
    {
        return CreateRequest()
            .WithUrl(url)
            .WithMethod(HttpMethod.Get);
    }

    public IBerryHttpRequestBuilder Post(string url)
    {
        return CreateRequest()
            .WithUrl(url)
            .WithMethod(HttpMethod.Post);
    }

    public IBerryHttpRequestBuilder Put(string url)
    {
        return CreateRequest()
            .WithUrl(url)
            .WithMethod(HttpMethod.Put);
    }

    public IBerryHttpRequestBuilder Delete(string url)
    {
        return CreateRequest()
            .WithUrl(url)
            .WithMethod(HttpMethod.Delete);
    }

    public IBerryHttpRequestBuilder Patch(string url)
    {
        return CreateRequest()
            .WithUrl(url)
            .WithMethod(HttpMethod.Patch);
    }

    private static JsonSerializerOptions CreateDefaultJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
