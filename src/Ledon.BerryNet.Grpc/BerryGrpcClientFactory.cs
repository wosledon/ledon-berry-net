using System;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc;

/// <summary>
/// BerryNet gRPC 客户端工厂接口
/// </summary>
public interface IBerryGrpcClientFactory
{
    /// <summary>
    /// 创建 gRPC 客户端构建器
    /// </summary>
    /// <returns>gRPC 客户端构建器</returns>
    IBerryGrpcClientBuilder CreateBuilder();

    /// <summary>
    /// 创建 gRPC 客户端构建器并设置服务地址
    /// </summary>
    /// <param name="address">服务地址</param>
    /// <returns>gRPC 客户端构建器</returns>
    IBerryGrpcClientBuilder CreateBuilder(string address);

    /// <summary>
    /// 创建 gRPC 客户端构建器并设置服务地址
    /// </summary>
    /// <param name="address">服务地址</param>
    /// <returns>gRPC 客户端构建器</returns>
    IBerryGrpcClientBuilder CreateBuilder(Uri address);

    /// <summary>
    /// 直接创建 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="address">服务地址</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClient<T>(string address) where T : ClientBase<T>;

    /// <summary>
    /// 直接创建 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="address">服务地址</param>
    /// <param name="configure">配置委托</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClient<T>(string address, Action<IBerryGrpcClientBuilder> configure) where T : ClientBase<T>;

    /// <summary>
    /// 创建带认证的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="address">服务地址</param>
    /// <param name="token">认证 Token</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClientWithAuth<T>(string address, string token) where T : ClientBase<T>;

    /// <summary>
    /// 创建带 Bearer Token 的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="address">服务地址</param>
    /// <param name="bearerToken">Bearer Token</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClientWithBearerToken<T>(string address, string bearerToken) where T : ClientBase<T>;

    /// <summary>
    /// 创建带压缩的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="address">服务地址</param>
    /// <param name="compressionProvider">压缩提供程序</param>
    /// <returns>gRPC 客户端实例</returns>
    T CreateClientWithCompression<T>(string address, string compressionProvider = "gzip") where T : ClientBase<T>;
}

/// <summary>
/// BerryNet gRPC 客户端工厂实现
/// </summary>
public class BerryGrpcClientFactory : IBerryGrpcClientFactory
{
    private readonly ILogger<BerryGrpcClientBuilder>? _logger;

    /// <summary>
    /// 初始化新实例
    /// </summary>
    public BerryGrpcClientFactory()
    {
    }

    /// <summary>
    /// 初始化新实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public BerryGrpcClientFactory(ILogger<BerryGrpcClientBuilder>? logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder CreateBuilder()
    {
        return new BerryGrpcClientBuilder(_logger);
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder CreateBuilder(string address)
    {
        return new BerryGrpcClientBuilder(_logger).WithAddress(address);
    }

    /// <inheritdoc />
    public IBerryGrpcClientBuilder CreateBuilder(Uri address)
    {
        return new BerryGrpcClientBuilder(_logger).WithAddress(address);
    }

    /// <inheritdoc />
    public T CreateClient<T>(string address) where T : ClientBase<T>
    {
        return CreateBuilder(address).CreateClient<T>();
    }

    /// <inheritdoc />
    public T CreateClient<T>(string address, Action<IBerryGrpcClientBuilder> configure) where T : ClientBase<T>
    {
        var builder = CreateBuilder(address);
        configure?.Invoke(builder);
        return builder.CreateClient<T>();
    }

    /// <inheritdoc />
    public T CreateClientWithAuth<T>(string address, string token) where T : ClientBase<T>
    {
        return CreateBuilder(address)
            .WithAuthToken(token)
            .CreateClient<T>();
    }

    /// <inheritdoc />
    public T CreateClientWithBearerToken<T>(string address, string bearerToken) where T : ClientBase<T>
    {
        return CreateBuilder(address)
            .WithBearerToken(bearerToken)
            .CreateClient<T>();
    }

    /// <inheritdoc />
    public T CreateClientWithCompression<T>(string address, string compressionProvider = "gzip") where T : ClientBase<T>
    {
        return CreateBuilder(address)
            .WithCompression(compressionProvider)
            .CreateClient<T>();
    }
}
