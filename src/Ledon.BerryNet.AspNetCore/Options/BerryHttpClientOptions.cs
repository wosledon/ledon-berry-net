using System.Text.Json;

namespace Ledon.BerryNet.AspNetCore.Options;

/// <summary>
/// BerryNet HTTP客户端配置选项
/// </summary>
public class BerryHttpClientOptions
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
    /// 是否忽略SSL证书错误
    /// </summary>
    public bool IgnoreSslErrors { get; set; }
    
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; }
    
    /// <summary>
    /// 重试间隔
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
