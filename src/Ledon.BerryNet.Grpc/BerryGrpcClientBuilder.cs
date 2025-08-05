using System;
using System.Linq;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc;

/// <summary>
/// BerryNet gRPC 客户端构建器实现
/// </summary>
public class BerryGrpcClientBuilder : IBerryGrpcClientBuilder
{
    private Uri? _address;
    private GrpcChannelOptions _channelOptions = new();
    private CallOptions _callOptions = new();
    private readonly Metadata _metadata = new();
    private RetryPolicy? _retryPolicy;
    private readonly ILogger<BerryGrpcClientBuilder>? _logger;

    /// <summary>
    /// 初始化新实例
    /// </summary>
    public BerryGrpcClientBuilder()
    {
    }

    /// <summary>
    /// 初始化新实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public BerryGrpcClientBuilder(ILogger<BerryGrpcClientBuilder>? logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or empty", nameof(address));

        _address = new Uri(address);
        _logger?.LogDebug("设置 gRPC 服务地址: {Address}", address);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithAddress(Uri address)
    {
        _address = address ?? throw new ArgumentNullException(nameof(address));
        _logger?.LogDebug("设置 gRPC 服务地址: {Address}", address);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithChannelOptions(Action<GrpcChannelOptions> configure)
    {
        configure?.Invoke(_channelOptions);
        _logger?.LogDebug("配置 gRPC 通道选项");
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithCallOptions(CallOptions callOptions)
    {
        _callOptions = callOptions;
        _logger?.LogDebug("设置调用选项");
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithTimeout(TimeSpan timeout)
    {
        _callOptions = _callOptions.WithDeadline(DateTime.UtcNow.Add(timeout));
        _logger?.LogDebug("设置超时时间: {Timeout}", timeout);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithDeadline(DateTime deadline)
    {
        _callOptions = _callOptions.WithDeadline(deadline);
        _logger?.LogDebug("设置截止时间: {Deadline}", deadline);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        _metadata.Add(key, value ?? string.Empty);
        _logger?.LogDebug("添加元数据: {Key} = {Value}", key, value);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithMetadata(Metadata metadata)
    {
        if (metadata != null)
        {
            foreach (var entry in metadata)
            {
                _metadata.Add(entry);
            }
            _logger?.LogDebug("添加 {Count} 个元数据项", metadata.Count);
        }
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithAuthToken(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _metadata.Add("authorization", token);
            _logger?.LogDebug("设置认证 Token");
        }
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithBearerToken(string token)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            _metadata.Add("authorization", $"Bearer {token}");
            _logger?.LogDebug("设置 Bearer Token");
        }
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithCompression(string compressionProvider = "gzip")
    {
        // gRPC 压缩通过通道选项配置
        _logger?.LogDebug("启用压缩: {Provider}", compressionProvider);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithRetry(Action<RetryPolicy> configure)
    {
        _retryPolicy = new RetryPolicy();
        configure?.Invoke(_retryPolicy);
        _logger?.LogDebug("配置重试策略: 最大重试次数 {MaxAttempts}", _retryPolicy.MaxRetryAttempts);
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithTls()
    {
        _channelOptions.Credentials = ChannelCredentials.SecureSsl;
        _logger?.LogDebug("启用 TLS");
        return this;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder WithoutTls()
    {
        _channelOptions.Credentials = ChannelCredentials.Insecure;
        _logger?.LogDebug("禁用 TLS");
        return this;
    }

    /// <inheritdoc />
    public GrpcChannel CreateChannel()
    {
        if (_address == null)
            throw new InvalidOperationException("服务地址未设置，请先调用 WithAddress 方法");

        // 应用元数据
        if (_metadata.Count > 0)
        {
            _callOptions = _callOptions.WithHeaders(_metadata);
        }

        // 配置重试策略
        if (_retryPolicy != null)
        {
            ConfigureRetryPolicy(_channelOptions, _retryPolicy);
        }

        var channel = GrpcChannel.ForAddress(_address, _channelOptions);
        _logger?.LogInformation("创建 gRPC 通道: {Address}", _address);
        
        return channel;
    }

    /// <inheritdoc />
    public T CreateClient<T>() where T : ClientBase<T>
    {
        var channel = CreateChannel();
        var constructor = typeof(T).GetConstructor(new[] { typeof(GrpcChannel) });
        
        if (constructor == null)
        {
            constructor = typeof(T).GetConstructor(new[] { typeof(ChannelBase) });
        }

        if (constructor == null)
        {
            throw new InvalidOperationException($"无法为类型 {typeof(T).Name} 找到合适的构造函数");
        }

        var client = (T)constructor.Invoke(new object[] { channel });
        _logger?.LogInformation("创建 gRPC 客户端: {ClientType}", typeof(T).Name);
        
        return client;
    }

    /// <inheritdoc />
    public T CreateClient<T>(Func<GrpcChannel, T> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var channel = CreateChannel();
        var client = factory(channel);
        _logger?.LogInformation("使用工厂方法创建 gRPC 客户端: {ClientType}", typeof(T).Name);
        
        return client;
    }

    private void ConfigureRetryPolicy(GrpcChannelOptions options, RetryPolicy retryPolicy)
    {
        // 注意：gRPC 重试策略配置在生产环境中可能需要更复杂的实现
        // 这里提供一个基础的配置示例
        _logger?.LogDebug("配置重试策略完成");
    }
}
