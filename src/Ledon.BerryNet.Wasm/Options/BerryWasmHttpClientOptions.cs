using System.Text.Json;

namespace Ledon.BerryNet.Wasm.Options;

/// <summary>
/// BerryNet WASM HTTP客户端配置选项
/// </summary>
public class BerryWasmHttpClientOptions
{
    /// <summary>
    /// 基础地址
    /// </summary>
    public string? BaseAddress { get; set; }
    
    /// <summary>
    /// 默认超时时间
    /// </summary>
    public TimeSpan? Timeout { get; set; }
    
    /// <summary>
    /// 默认请求头
    /// </summary>
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    
    /// <summary>
    /// JSON序列化选项
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    
    /// <summary>
    /// 是否启用CORS凭证
    /// </summary>
    public bool EnableCredentials { get; set; }
    
    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool EnableCache { get; set; }
    
    /// <summary>
    /// 缓存模式
    /// </summary>
    public string CacheMode { get; set; } = "default";
    
    /// <summary>
    /// 重定向模式
    /// </summary>
    public string RedirectMode { get; set; } = "follow";

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
    /// 引用策略
    /// </summary>
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";
}
