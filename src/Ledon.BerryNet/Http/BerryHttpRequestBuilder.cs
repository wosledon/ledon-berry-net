using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ledon.BerryNet.Http;

/// <summary>
/// HTTP请求构建器实现
/// </summary>
public class BerryHttpRequestBuilder : IBerryHttpRequestBuilder
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _url;
    private System.Net.Http.HttpMethod _method = System.Net.Http.HttpMethod.Get;
    private readonly Dictionary<string, string> _headers = new();
    private readonly Dictionary<string, string> _queryParameters = new();
    private HttpContent? _content;
    private TimeSpan? _timeout;

    public BerryHttpRequestBuilder(HttpClient httpClient, JsonSerializerOptions? jsonOptions = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public IBerryHttpRequestBuilder WithUrl(string url)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        return this;
    }

    public IBerryHttpRequestBuilder WithMethod(HttpMethod method)
    {
        _method = method switch
        {
            HttpMethod.Get => System.Net.Http.HttpMethod.Get,
            HttpMethod.Post => System.Net.Http.HttpMethod.Post,
            HttpMethod.Put => System.Net.Http.HttpMethod.Put,
            HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
            HttpMethod.Patch => System.Net.Http.HttpMethod.Patch,
            HttpMethod.Head => System.Net.Http.HttpMethod.Head,
            HttpMethod.Options => System.Net.Http.HttpMethod.Options,
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };
        return this;
    }

    public IBerryHttpRequestBuilder WithHeader(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("请求头名称不能为空", nameof(name));
        
        _headers[name] = value;
        return this;
    }

    public IBerryHttpRequestBuilder WithAuthentication(string scheme, string token)
    {
        if (string.IsNullOrWhiteSpace(scheme))
            throw new ArgumentException("认证方案不能为空", nameof(scheme));
        
        _headers["Authorization"] = $"{scheme} {token}";
        return this;
    }

    public IBerryHttpRequestBuilder WithBearerToken(string token)
    {
        return WithAuthentication("Bearer", token);
    }

    public IBerryHttpRequestBuilder WithJsonBody<T>(T data)
    {
        if (data == null)
        {
            _content = null;
            return this;
        }

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        _content = new StringContent(json, Encoding.UTF8, "application/json");
        return this;
    }

    public IBerryHttpRequestBuilder WithStringBody(string content, string mediaType = "text/plain")
    {
        _content = new StringContent(content ?? string.Empty, Encoding.UTF8, mediaType);
        return this;
    }

    public IBerryHttpRequestBuilder WithFormBody(Dictionary<string, string> formData)
    {
        if (formData == null || !formData.Any())
        {
            _content = null;
            return this;
        }

        _content = new FormUrlEncodedContent(formData);
        return this;
    }

    public IBerryHttpRequestBuilder WithQueryParameter(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("查询参数名称不能为空", nameof(name));
        
        _queryParameters[name] = value;
        return this;
    }

    public IBerryHttpRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public IBerryHttpRequestBuilder WithGzipCompression()
    {
        return WithCompression("gzip");
    }

    public IBerryHttpRequestBuilder WithDeflateCompression()
    {
        return WithCompression("deflate");
    }

    public IBerryHttpRequestBuilder WithBrotliCompression()
    {
        return WithCompression("br");
    }

    public IBerryHttpRequestBuilder WithCompression(string encodings)
    {
        if (!string.IsNullOrWhiteSpace(encodings))
        {
            _headers["Accept-Encoding"] = encodings;
        }
        return this;
    }

    public async Task<IBerryHttpResponse> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var request = BuildHttpRequestMessage();
        
        using var cts = CreateCancellationTokenSource(cancellationToken);
        
        try
        {
            var response = await _httpClient.SendAsync(request, cts.Token);
            var content = await response.Content.ReadAsStringAsync();
            
            return new BerryHttpResponse(response, content, _jsonOptions);
        }
        catch (TaskCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException("HTTP请求超时");
        }
    }

    public async Task<T> ExecuteAsync<T>(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteAsync(cancellationToken);
        
        if (!response.IsSuccessful)
        {
            throw new HttpRequestException($"HTTP请求失败: {response.StatusCode} - {response.Content}");
        }
        
        return response.GetContent<T>();
    }

    private HttpRequestMessage BuildHttpRequestMessage()
    {
        if (string.IsNullOrWhiteSpace(_url))
            throw new InvalidOperationException("请求URL不能为空");

        var finalUrl = BuildUrlWithQueryParameters();
        var request = new HttpRequestMessage(_method, finalUrl);

        // 添加请求头
        foreach (var header in _headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // 设置请求体
        if (_content != null)
        {
            request.Content = _content;
        }

        return request;
    }

    private string BuildUrlWithQueryParameters()
    {
        if (!_queryParameters.Any())
            return _url!;

        var uriBuilder = new UriBuilder(_url!);
        
        // 解析现有查询参数
        var existingParams = new List<KeyValuePair<string, string>>();
        if (!string.IsNullOrEmpty(uriBuilder.Query))
        {
            var queryString = uriBuilder.Query.TrimStart('?');
            var pairs = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=', 2);
                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                existingParams.Add(new KeyValuePair<string, string>(key, value));
            }
        }
        
        // 添加新的查询参数
        foreach (var param in _queryParameters)
        {
            existingParams.Add(new KeyValuePair<string, string>(param.Key, param.Value));
        }
        
        // 构建最终查询字符串
        if (existingParams.Any())
        {
            var queryPairs = existingParams.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            uriBuilder.Query = string.Join("&", queryPairs);
        }
        
        return uriBuilder.ToString();
    }

    private CancellationTokenSource CreateCancellationTokenSource(CancellationToken cancellationToken)
    {
        if (_timeout.HasValue)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout.Value);
            return cts;
        }
        
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }
}
