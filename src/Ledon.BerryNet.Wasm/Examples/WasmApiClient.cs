using Ledon.BerryNet.Wasm.Http;

namespace Ledon.BerryNet.Wasm.Examples;

/// <summary>
/// 示例：WASM API客户端
/// </summary>
public class WasmApiClient
{
    private readonly IBerryWasmHttpClient _httpClient;

    public WasmApiClient(IBerryWasmHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 获取用户列表（支持CORS和压缩）
    /// </summary>
    public async Task<List<User>?> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient
            .Get("/api/users")
            .WithCorsMode("cors")
            .WithCredentials(true)
            .WithCacheMode("no-cache")
            .WithGzipCompression()
            .ExecuteAsync<List<User>>(cancellationToken);
    }

    /// <summary>
    /// 创建用户（支持压缩）
    /// </summary>
    public async Task<User?> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await _httpClient
            .Post("/api/users")
            .WithJsonBody(request)
            .WithCorsMode("cors")
            .WithCredentials(true)
            .WithBrotliCompression()
            .ExecuteAsync<User>(cancellationToken);
    }

    /// <summary>
    /// 下载文件（支持进度回调）
    /// </summary>
    public async Task<byte[]> DownloadFileAsync(
        string fileId, 
        Action<long, long>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var builder = _httpClient
            .Get($"/api/files/{fileId}/download")
            .WithStreamingResponse()
            .WithCacheMode("no-cache");

        if (progressCallback != null)
        {
            builder = builder.WithProgressCallback(progressCallback);
        }

        var response = await builder.ExecuteAsync(cancellationToken);
        return response.GetBytes();
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    public async Task<UploadResult?> UploadFileAsync(
        string fileName,
        byte[] fileData,
        Action<long, long>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        var formData = new Dictionary<string, string>
        {
            ["fileName"] = fileName,
            ["fileSize"] = fileData.Length.ToString()
        };

        var builder = _httpClient
            .Post("/api/files/upload")
            .WithFormBody(formData)
            .WithCorsMode("cors")
            .WithCredentials(true);

        if (progressCallback != null)
        {
            builder = builder.WithProgressCallback(progressCallback);
        }

        return await builder.ExecuteAsync<UploadResult>(cancellationToken);
    }

    /// <summary>
    /// 获取实时数据（支持长连接）
    /// </summary>
    public async Task<RealTimeData?> GetRealTimeDataAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient
            .Get("/api/realtime")
            .WithCredentials(true)
            .WithCacheMode("no-store")
            .WithReferrerPolicy("no-referrer")
            .WithTimeout(TimeSpan.FromMinutes(5)) // 长连接超时
            .ExecuteAsync<RealTimeData>(cancellationToken);
    }
}

/// <summary>
/// 用户模型
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// 创建用户请求
/// </summary>
public class CreateUserRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

/// <summary>
/// 上传结果
/// </summary>
public class UploadResult
{
    public string FileId { get; set; } = "";
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public string UploadUrl { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}

/// <summary>
/// 实时数据
/// </summary>
public class RealTimeData
{
    public string Id { get; set; } = "";
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = "";
}
