using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Dynamic.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加动态 gRPC 客户端工厂
    /// </summary>
    public static IServiceCollection AddDynamicGrpcClientFactory(this IServiceCollection services)
    {
        services.AddSingleton<DynamicGrpcClientFactory>(serviceProvider =>
        {
            var logger = serviceProvider.GetService<ILogger<DynamicGrpcClient>>();
            return new DynamicGrpcClientFactory(logger);
        });

        return services;
    }

    /// <summary>
    /// 添加命名的动态 gRPC 客户端
    /// </summary>
    public static IServiceCollection AddDynamicGrpcClient(
        this IServiceCollection services,
        string name,
        string address,
        Action<IDynamicGrpcClientBuilder>? configure = null)
    {
        services.AddDynamicGrpcClientFactory();

#if NET7_0
        services.AddSingleton<Func<string, IDynamicGrpcClient>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DynamicGrpcClientFactory>();
            return clientName =>
            {
                if (clientName == name)
                {
                    return configure != null 
                        ? factory.CreateClient(address, configure)
                        : factory.CreateClient(address);
                }
                throw new InvalidOperationException($"Dynamic gRPC client '{clientName}' not found");
            };
        });
#else
        services.AddKeyedSingleton<IDynamicGrpcClient>(name, (serviceProvider, key) =>
        {
            var factory = serviceProvider.GetRequiredService<DynamicGrpcClientFactory>();
            return configure != null 
                ? factory.CreateClient(address, configure)
                : factory.CreateClient(address);
        });
#endif

        return services;
    }

    /// <summary>
    /// 添加默认动态 gRPC 客户端
    /// </summary>
    public static IServiceCollection AddDynamicGrpcClient(
        this IServiceCollection services,
        string address,
        Action<IDynamicGrpcClientBuilder>? configure = null)
    {
        services.AddDynamicGrpcClientFactory();
        
        services.AddSingleton<IDynamicGrpcClient>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<DynamicGrpcClientFactory>();
            return configure != null 
                ? factory.CreateClient(address, configure)
                : factory.CreateClient(address);
        });

        return services;
    }

#if !NET7_0
    /// <summary>
    /// 获取命名的动态 gRPC 客户端 (.NET 8+)
    /// </summary>
    public static IDynamicGrpcClient GetDynamicGrpcClient(this IServiceProvider services, string name)
    {
        return services.GetRequiredKeyedService<IDynamicGrpcClient>(name);
    }
#else
    /// <summary>
    /// 获取命名的动态 gRPC 客户端 (.NET 7)
    /// </summary>
    public static IDynamicGrpcClient GetDynamicGrpcClient(this IServiceProvider services, string name)
    {
        var clientFactory = services.GetRequiredService<Func<string, IDynamicGrpcClient>>();
        return clientFactory(name);
    }
#endif
}
