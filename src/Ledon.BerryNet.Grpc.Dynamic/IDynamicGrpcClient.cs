using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client;
using Ledon.BerryNet.Grpc.Dynamic.Messages;

namespace Ledon.BerryNet.Grpc.Dynamic;

/// <summary>
/// 动态 gRPC 客户端接口
/// </summary>
public interface IDynamicGrpcClient : IDisposable
{
    /// <summary>
    /// 获取服务描述符
    /// </summary>
    Task<ServiceDescriptor?> GetServiceDescriptorAsync(string serviceName);

    /// <summary>
    /// 调用一元 RPC 方法
    /// </summary>
    Task<DynamicMessage> CallUnaryAsync(string serviceName, string methodName, DynamicMessage request, CallOptions? options = null);

    /// <summary>
    /// 调用一元 RPC 方法 (JSON)
    /// </summary>
    Task<string> CallUnaryJsonAsync(string serviceName, string methodName, string requestJson, CallOptions? options = null);

    /// <summary>
    /// 调用客户端流式 RPC 方法
    /// </summary>
    Task<DynamicMessage> CallClientStreamingAsync(string serviceName, string methodName, IAsyncEnumerable<DynamicMessage> requests, CallOptions? options = null);

    /// <summary>
    /// 调用服务端流式 RPC 方法
    /// </summary>
    IAsyncEnumerable<DynamicMessage> CallServerStreamingAsync(string serviceName, string methodName, DynamicMessage request, CallOptions? options = null);

    /// <summary>
    /// 调用双向流式 RPC 方法
    /// </summary>
    IAsyncEnumerable<DynamicMessage> CallBidirectionalStreamingAsync(string serviceName, string methodName, IAsyncEnumerable<DynamicMessage> requests, CallOptions? options = null);

    /// <summary>
    /// 获取所有可用的服务名称
    /// </summary>
    Task<IEnumerable<string>> GetAvailableServicesAsync();

    /// <summary>
    /// 获取指定服务的所有方法
    /// </summary>
    Task<IEnumerable<MethodDescriptor>> GetServiceMethodsAsync(string serviceName);

    /// <summary>
    /// 创建指定消息类型的空消息
    /// </summary>
    Task<DynamicMessage> CreateMessageAsync(string messageTypeName);
}

/// <summary>
/// 动态 gRPC 客户端构建器接口
/// </summary>
public interface IDynamicGrpcClientBuilder
{
    /// <summary>
    /// 设置连接地址
    /// </summary>
    IDynamicGrpcClientBuilder WithAddress(string address);

    /// <summary>
    /// 设置超时时间
    /// </summary>
    IDynamicGrpcClientBuilder WithTimeout(TimeSpan timeout);

    /// <summary>
    /// 添加认证头
    /// </summary>
    IDynamicGrpcClientBuilder WithBearerToken(string token);

    /// <summary>
    /// 添加自定义头信息
    /// </summary>
    IDynamicGrpcClientBuilder WithHeader(string key, string value);

    /// <summary>
    /// 启用压缩
    /// </summary>
    IDynamicGrpcClientBuilder WithCompression(string? algorithm = "gzip");

    /// <summary>
    /// 配置重试策略
    /// </summary>
    IDynamicGrpcClientBuilder WithRetry(Action<RetryPolicy> configure);

    /// <summary>
    /// 设置是否启用反射
    /// </summary>
    IDynamicGrpcClientBuilder WithReflection(bool enabled = true);

    /// <summary>
    /// 配置 gRPC 通道选项
    /// </summary>
    IDynamicGrpcClientBuilder WithChannelOptions(Action<GrpcChannelOptions> configure);

    /// <summary>
    /// 创建动态客户端
    /// </summary>
    IDynamicGrpcClient Build();
}

/// <summary>
/// 重试策略配置
/// </summary>
public class RetryPolicy
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan InitialBackoff { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromSeconds(30);
    public double BackoffMultiplier { get; set; } = 2.0;
    public IEnumerable<StatusCode> RetryableStatusCodes { get; set; } = new[]
    {
        StatusCode.Unavailable,
        StatusCode.DeadlineExceeded,
        StatusCode.ResourceExhausted
    };
}
