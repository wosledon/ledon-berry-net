using Ledon.BerryNet.Http;

namespace Ledon.BerryNet.AspNetCore.Examples;

/// <summary>
/// 示例：类型化HTTP客户端
/// </summary>
public class WeatherApiClient
{
    private readonly IBerryHttpClient _httpClient;

    public WeatherApiClient(IBerryHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 获取天气信息
    /// </summary>
    public async Task<WeatherInfo?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        return await _httpClient
            .Get($"/weather")
            .WithQueryParameter("q", city)
            .WithQueryParameter("appid", "your-api-key")
            .ExecuteAsync<WeatherInfo>(cancellationToken);
    }

    /// <summary>
    /// 获取多个城市天气信息
    /// </summary>
    public async Task<List<WeatherInfo>> GetMultipleWeatherAsync(IEnumerable<string> cities, CancellationToken cancellationToken = default)
    {
        var tasks = cities.Select(city => GetWeatherAsync(city, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).Cast<WeatherInfo>().ToList();
    }
}

/// <summary>
/// 天气信息模型
/// </summary>
public class WeatherInfo
{
    public string? Name { get; set; }
    public MainInfo? Main { get; set; }
    public List<WeatherDetail>? Weather { get; set; }
}

public class MainInfo
{
    public double Temp { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Pressure { get; set; }
    public int Humidity { get; set; }
}

public class WeatherDetail
{
    public int Id { get; set; }
    public string? Main { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
}
