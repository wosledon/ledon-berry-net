using System;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Extensions;

/// <summary>
/// BerryNet gRPC 服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 BerryNet gRPC 客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient(this IServiceCollection services)
    {
        services.TryAddSingleton<IBerryGrpcClientFactory, BerryGrpcClientFactory>();
        return services;
    }

    /// <summary>
    /// 添加 BerryNet gRPC 客户端服务并配置
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient(this IServiceCollection services, Action<BerryGrpcClientOptions> configure)
    {
        var options = new BerryGrpcClientOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<IBerryGrpcClientFactory>(provider =>
        {
            var logger = provider.GetService<ILogger<BerryGrpcClientBuilder>>();
            return new BerryGrpcClientFactory(logger);
        });

        return services;
    }

    /// <summary>
    /// 添加指定类型的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="address">服务地址</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient<T>(this IServiceCollection services, string address)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address);
        });

        return services;
    }

    /// <summary>
    /// 添加指定类型的 gRPC 客户端并配置
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="address">服务地址</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient<T>(this IServiceCollection services, string address, Action<IBerryGrpcClientBuilder> configure)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address, configure);
        });

        return services;
    }

    /// <summary>
    /// 添加带认证的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="address">服务地址</param>
    /// <param name="token">认证 Token</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClientWithAuth<T>(this IServiceCollection services, string address, string token)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClientWithAuth<T>(address, token);
        });

        return services;
    }

    /// <summary>
    /// 添加带 Bearer Token 的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="address">服务地址</param>
    /// <param name="bearerToken">Bearer Token</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClientWithBearerToken<T>(this IServiceCollection services, string address, string bearerToken)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClientWithBearerToken<T>(address, bearerToken);
        });

        return services;
    }

    /// <summary>
    /// 添加带压缩的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="address">服务地址</param>
    /// <param name="compressionProvider">压缩提供程序</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClientWithCompression<T>(this IServiceCollection services, string address, string compressionProvider = "gzip")
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClientWithCompression<T>(address, compressionProvider);
        });

        return services;
    }

    /// <summary>
    /// 添加命名的 gRPC 客户端
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="address">服务地址</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient<T>(this IServiceCollection services, string name, string address)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
#if NET8_0_OR_GREATER
        services.AddKeyedTransient<T>(name, (provider, key) =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address);
        });
#else
        // 对于 .NET 7.0，使用普通的服务注册
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address);
        });
#endif

        return services;
    }

    /// <summary>
    /// 添加命名的 gRPC 客户端并配置
    /// </summary>
    /// <typeparam name="T">客户端类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="name">客户端名称</param>
    /// <param name="address">服务地址</param>
    /// <param name="configure">配置委托</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBerryGrpcClient<T>(this IServiceCollection services, string name, string address, Action<IBerryGrpcClientBuilder> configure)
        where T : ClientBase<T>
    {
        services.AddBerryGrpcClient();
        
#if NET8_0_OR_GREATER
        services.AddKeyedTransient<T>(name, (provider, key) =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address, configure);
        });
#else
        // 对于 .NET 7.0，使用普通的服务注册
        services.AddTransient<T>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryGrpcClientFactory>();
            return factory.CreateClient<T>(address, configure);
        });
#endif

        return services;
    }
}

/// <summary>
/// BerryNet gRPC 客户端选项
/// </summary>
public class BerryGrpcClientOptions
{
    /// <summary>
    /// 默认超时时间
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 默认重试次数
    /// </summary>
    public int DefaultRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 启用日志记录
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// 默认压缩提供程序
    /// </summary>
    public string DefaultCompressionProvider { get; set; } = "gzip";
}
