using System;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Net.Client;

namespace Ledon.BerryNet.Grpc;

/// <summary>
/// BerryNet gRPC 客户端构建器接口
/// </summary>
public interface IBerryGrpcClientBuilder
{
    /// <summary>
    /// 设置 gRPC 服务地址
    /// </summary>
    /// <param name="address">服务地址</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithAddress(string address);

    /// <summary>
    /// 设置 gRPC 服务地址
    /// </summary>
    /// <param name="address">服务地址</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithAddress(Uri address);

    /// <summary>
    /// 配置 gRPC 通道选项
    /// </summary>
    /// <param name="configure">配置委托</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithChannelOptions(Action<GrpcChannelOptions> configure);

    /// <summary>
    /// 设置调用选项
    /// </summary>
    /// <param name="callOptions">调用选项</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithCallOptions(CallOptions callOptions);

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithTimeout(TimeSpan timeout);

    /// <summary>
    /// 设置截止时间
    /// </summary>
    /// <param name="deadline">截止时间</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithDeadline(DateTime deadline);

    /// <summary>
    /// 添加元数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithMetadata(string key, string value);

    /// <summary>
    /// 添加元数据
    /// </summary>
    /// <param name="metadata">元数据</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithMetadata(Metadata metadata);

    /// <summary>
    /// 设置认证 Token
    /// </summary>
    /// <param name="token">认证 Token</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithAuthToken(string token);

    /// <summary>
    /// 设置 Bearer Token
    /// </summary>
    /// <param name="token">Bearer Token</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithBearerToken(string token);

    /// <summary>
    /// 启用压缩
    /// </summary>
    /// <param name="compressionProvider">压缩提供程序</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithCompression(string compressionProvider = "gzip");

    /// <summary>
    /// 设置重试策略
    /// </summary>
    /// <param name="configure">重试配置</param>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithRetry(Action<RetryPolicy> configure);

    /// <summary>
    /// 启用 TLS
    /// </summary>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithTls();

    /// <summary>
    /// 禁用 TLS（使用 HTTP）
    /// </summary>
    /// <returns>构建器实例</returns>
    IBerryGrpcClientBuilder WithoutTls();

    /// <summary>
    /// 创建 gRPC 通道
    /// </summary>
    /// <returns>gRPC 通道</returns>
    GrpcChannel CreateChannel();

    /// <summary>
    /// 创建指定类型的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClient<T>() where T : ClientBase<T>;

    /// <summary>
    /// 创建指定类型的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="factory">客户端工厂方法</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClient<T>(Func<GrpcChannel, T> factory);
}

/// <summary>
/// 重试策略配置
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 初始重试延迟
    /// </summary>
    public TimeSpan InitialBackoff { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 最大重试延迟
    /// </summary>
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 退避乘数
    /// </summary>
    public double BackoffMultiplier { get; set; } = 1.6;

    /// <summary>
    /// 可重试的状态码
    /// </summary>
    public ISet<StatusCode> RetryableStatusCodes { get; set; } = new HashSet<StatusCode>
    {
        StatusCode.Unavailable,
        StatusCode.DeadlineExceeded,
        StatusCode.ResourceExhausted
    };
}
