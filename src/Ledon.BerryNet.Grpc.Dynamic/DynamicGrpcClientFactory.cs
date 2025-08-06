using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Dynamic;

/// <summary>
/// 动态 gRPC 客户端工厂
/// </summary>
public class DynamicGrpcClientFactory
{
    private readonly ILogger<DynamicGrpcClient>? _logger;

    public DynamicGrpcClientFactory(ILogger<DynamicGrpcClient>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 创建动态 gRPC 客户端构建器
    /// </summary>
    public IDynamicGrpcClientBuilder CreateBuilder(string address)
    {
        return new DynamicGrpcClientBuilder(_logger).WithAddress(address);
    }

    /// <summary>
    /// 创建简单的动态 gRPC 客户端
    /// </summary>
    public IDynamicGrpcClient CreateClient(string address)
    {
        return CreateBuilder(address).Build();
    }

    /// <summary>
    /// 使用配置创建动态 gRPC 客户端
    /// </summary>
    public IDynamicGrpcClient CreateClient(string address, Action<IDynamicGrpcClientBuilder> configure)
    {
        var builder = CreateBuilder(address);
        configure(builder);
        return builder.Build();
    }
}
