using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ledon.BerryNet.Http;

/// <summary>
/// HTTP响应实现
/// </summary>
public class BerryHttpResponse : IBerryHttpResponse
{
    private readonly HttpResponseMessage _response;
    private readonly string _content;
    private readonly JsonSerializerOptions _jsonOptions;

    public BerryHttpResponse(HttpResponseMessage response, string content, JsonSerializerOptions? jsonOptions = null)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
        _content = content ?? string.Empty;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public int StatusCode => (int)_response.StatusCode;
    
    public bool IsSuccessful => _response.IsSuccessStatusCode;
    
    public HttpResponseHeaders Headers => _response.Headers;
    
    public string Content => _content;

    public T GetContent<T>()
    {
        if (string.IsNullOrEmpty(_content))
        {
            return default(T)!;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)_content;
            }

            return JsonSerializer.Deserialize<T>(_content, _jsonOptions)!;
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"无法将响应内容反序列化为类型 {typeof(T).Name}");
        }
    }

    public byte[] GetBytes()
    {
        return Encoding.UTF8.GetBytes(_content);
    }
}
