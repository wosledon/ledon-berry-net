using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Compression;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace Ledon.BerryNet.Grpc.Dynamic;

/// <summary>
/// 动态 gRPC 客户端构建器实现
/// </summary>
public class DynamicGrpcClientBuilder : IDynamicGrpcClientBuilder
{
    private string? _address;
    private TimeSpan? _timeout;
    private readonly Dictionary<string, string> _headers = new();
    private string? _compressionAlgorithm;
    private RetryPolicy? _retryPolicy;
    private bool _enableReflection = true;
    private Action<GrpcChannelOptions>? _channelOptionsAction;
    private readonly ILogger<DynamicGrpcClient>? _logger;
    private readonly List<FileDescriptor> _fileDescriptors = new();

    public DynamicGrpcClientBuilder(ILogger<DynamicGrpcClient>? logger = null)
    {
        _logger = logger;
    }

    public IDynamicGrpcClientBuilder WithAddress(string address)
    {
        _address = address ?? throw new ArgumentNullException(nameof(address));
        return this;
    }

    public IDynamicGrpcClientBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public IDynamicGrpcClientBuilder WithBearerToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _headers["Authorization"] = $"Bearer {token}";
        }
        return this;
    }

    public IDynamicGrpcClientBuilder WithHeader(string key, string value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _headers[key] = value ?? string.Empty;
        }
        return this;
    }

    public IDynamicGrpcClientBuilder WithCompression(string? algorithm = "gzip")
    {
        _compressionAlgorithm = algorithm;
        return this;
    }

    public IDynamicGrpcClientBuilder WithRetry(Action<RetryPolicy> configure)
    {
        _retryPolicy = new RetryPolicy();
        configure?.Invoke(_retryPolicy);
        return this;
    }

    public IDynamicGrpcClientBuilder WithReflection(bool enabled = true)
    {
        _enableReflection = enabled;
        return this;
    }

    public IDynamicGrpcClientBuilder WithChannelOptions(Action<GrpcChannelOptions> configure)
    {
        _channelOptionsAction = configure;
        return this;
    }

    public IDynamicGrpcClient Build()
    {
        if (string.IsNullOrEmpty(_address))
            throw new InvalidOperationException("Address must be specified");

        var options = new GrpcChannelOptions();
        
        // 应用自定义配置
        _channelOptionsAction?.Invoke(options);

        var channel = GrpcChannel.ForAddress(_address, options);
        var client = new DynamicGrpcClient(
            channel,
            _headers,
            _retryPolicy,
            _enableReflection,
            _logger);

        // 注册预定义的文件描述符
        foreach (var fileDescriptor in _fileDescriptors)
        {
            client.RegisterFileDescriptor(fileDescriptor);
        }

        return client;
    }

    /// <summary>
    /// 添加文件描述符
    /// </summary>
    public void AddFileDescriptor(FileDescriptor fileDescriptor)
    {
        _fileDescriptors.Add(fileDescriptor);
    }
}
