using Ledon.BerryNet.Wasm.Http;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Wasm.Examples;

/// <summary>
/// Blazor WebAssembly 组件中的使用示例
/// </summary>
public class BlazorUsageExamples
{
    private readonly IBerryWasmHttpClient _httpClient;
    private readonly ILogger<BlazorUsageExamples> _logger;

    public BlazorUsageExamples(IBerryWasmHttpClient httpClient, ILogger<BlazorUsageExamples> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 在Blazor组件中加载数据
    /// </summary>
    public async Task<List<WeatherForecast>?> LoadWeatherDataAsync()
    {
        try
        {
            _logger.LogInformation("开始加载天气数据");

            // 使用WASM特定的功能
            var builder = _httpClient.Get("/api/weather");
            var wasmBuilder = new BerryWasmRequestBuilder(builder);
            
            var weatherData = await wasmBuilder
                .WithCorsMode("cors")
                .WithCredentials(true)
                .WithCacheMode("default")
                .WithTimeout(TimeSpan.FromSeconds(10))
                .ExecuteAsync<List<WeatherForecast>>();

            _logger.LogInformation("成功加载 {Count} 条天气数据", weatherData?.Count ?? 0);
            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载天气数据失败");
            return null;
        }
    }

    /// <summary>
    /// 提交表单数据
    /// </summary>
    public async Task<bool> SubmitContactFormAsync(ContactForm form)
    {
        try
        {
            _logger.LogInformation("提交联系表单");

            var builder = _httpClient.Post("/api/contact");
            var wasmBuilder = new BerryWasmRequestBuilder(builder);

            var result = await wasmBuilder
                .WithJsonBody(form)
                .WithCorsMode("cors")
                .WithCredentials(true)
                .WithHeader("X-Requested-With", "XMLHttpRequest")
                .ExecuteAsync<SubmitResult>();

            _logger.LogInformation("表单提交结果: {Success}", result?.Success ?? false);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "提交表单失败");
            return false;
        }
    }

    /// <summary>
    /// 文件上传（带进度显示）
    /// </summary>
    public async Task<string?> UploadFileWithProgressAsync(
        Stream fileStream,
        string fileName,
        Action<int> onProgressChanged)
    {
        try
        {
            _logger.LogInformation("开始上传文件: {FileName}", fileName);

            // 读取文件内容
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();

            // 创建进度回调
            void ProgressCallback(long uploaded, long total)
            {
                if (total > 0)
                {
                    var percentage = (int)((uploaded * 100) / total);
                    onProgressChanged?.Invoke(percentage);
                    _logger.LogDebug("上传进度: {Percentage}% ({Uploaded}/{Total})", 
                        percentage, uploaded, total);
                }
            }

            var builder = _httpClient.Post("/api/upload");
            var wasmBuilder = new BerryWasmRequestBuilder(builder);

            var result = await wasmBuilder
                .WithProgressCallback(ProgressCallback)
                .WithCorsMode("cors")
                .WithCredentials(true)
                .WithTimeout(TimeSpan.FromMinutes(10)) // 上传超时
                .ExecuteAsync<UploadResponse>();

            _logger.LogInformation("文件上传完成: {FileId}", result?.FileId);
            return result?.FileId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败");
            return null;
        }
    }

    /// <summary>
    /// 实时数据订阅
    /// </summary>
    public async Task<List<NotificationData>> GetLatestNotificationsAsync()
    {
        try
        {
            _logger.LogInformation("获取最新通知");

            var builder = _httpClient.Get("/api/notifications");
            var wasmBuilder = new BerryWasmRequestBuilder(builder);

            var notifications = await wasmBuilder
                .WithCorsMode("cors")
                .WithCredentials(true)
                .WithCacheMode("no-cache")
                .WithTimeout(TimeSpan.FromSeconds(30))
                .ExecuteAsync<List<NotificationData>>();

            _logger.LogInformation("获取到 {Count} 条通知", notifications?.Count ?? 0);
            return notifications ?? new List<NotificationData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取通知失败");
            return new List<NotificationData>();
        }
    }
}

/// <summary>
/// 天气预报数据
/// </summary>
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}

/// <summary>
/// 联系表单
/// </summary>
public class ContactForm
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
}

/// <summary>
/// 提交结果
/// </summary>
public class SubmitResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 上传响应
/// </summary>
public class UploadResponse
{
    public string FileId { get; set; } = "";
    public string FileName { get; set; } = "";
    public long Size { get; set; }
    public string Url { get; set; } = "";
}

/// <summary>
/// 通知数据
/// </summary>
public class NotificationData
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}
