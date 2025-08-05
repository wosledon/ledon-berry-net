using System.Text.Json;

namespace Ledon.BerryNet.Wasm.Extensions;

/// <summary>
/// WASM HTTP客户端配置选项
/// </summary>
public class BerryWasmHttpClientOptions
{
    /// <summary>
    /// 基础地址
    /// </summary>
    public string? BaseAddress { get; set; }

    /// <summary>
    /// 请求超时时间
    /// </summary>
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();

    /// <summary>
    /// JSON序列化选项
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// 是否启用凭据（cookies等）
    /// </summary>
    public bool EnableCredentials { get; set; } = false;

    /// <summary>
    /// 默认CORS模式
    /// </summary>
    public string CorsMode { get; set; } = "cors";

    /// <summary>
    /// 默认缓存模式
    /// </summary>
    public string CacheMode { get; set; } = "default";

    /// <summary>
    /// 默认引用策略
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// 是否启用压缩支持
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// 支持的压缩算法
    /// </summary>
    public string[] CompressionEncodings { get; set; } = { "gzip", "deflate", "br" };

    /// <summary>
    /// 是否自动解压响应
    /// </summary>
    public bool AutoDecompression { get; set; } = true;

    /// <summary>
    /// 重定向模式
    /// </summary>
    public string RedirectMode { get; set; } = "follow";

    /// <summary>
    /// 最大重定向次数
    /// </summary>
    public int MaxRedirects { get; set; } = 10;

    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// 连接池大小
    /// </summary>
    public int ConnectionPoolSize { get; set; } = 10;

    /// <summary>
    /// 连接保持时间
    /// </summary>
    public TimeSpan ConnectionKeepAlive { get; set; } = TimeSpan.FromMinutes(2);
}
