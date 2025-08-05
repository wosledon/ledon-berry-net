using System.Net;
using System.Text.Json;
using Ledon.BerryNet.AspNetCore.Http;
using Ledon.BerryNet.AspNetCore.Options;
using Ledon.BerryNet.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ledon.BerryNet.AspNetCore.Extensions;

/// <summary>
/// BerryNet 服务注册扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加BerryNet HTTP客户端服务
    /// </summary>
    public static IServiceCollection AddBerryHttpClient(this IServiceCollection services)
    {
        return services.AddBerryHttpClient(_ => { });
    }

    /// <summary>
    /// 添加BerryNet HTTP客户端服务，支持配置
    /// </summary>
    public static IServiceCollection AddBerryHttpClient(
        this IServiceCollection services,
        Action<BerryHttpClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // 注册基础服务
        services.AddHttpClient();
        services.TryAddSingleton<IHttpContextAccessor, SimpleHttpContextAccessor>();
        
        // 配置选项
        services.Configure(configureOptions);
        
        // 注册BerryNet服务
        services.TryAddTransient<IBerryHttpClientFactory, BerryHttpClientFactoryEnhanced>();
        services.TryAddTransient<IBerryHttpClient>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryHttpClientFactory>();
            return factory.CreateClient();
        });
        
        services.TryAddTransient<IBerryHttpClientEnhanced>(provider =>
        {
            var innerClient = provider.GetRequiredService<IBerryHttpClient>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            return new BerryHttpClientEnhanced(innerClient, httpContextAccessor);
        });

        return services;
    }

    /// <summary>
    /// 添加命名的BerryNet HTTP客户端
    /// </summary>
    public static IServiceCollection AddBerryHttpClient(
        this IServiceCollection services,
        string name,
        Action<BerryHttpClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // 添加基础服务
        services.AddBerryHttpClient();
        
        // 配置命名选项
        services.Configure(name, configureOptions);
        
        // 配置命名HttpClient
        services.AddHttpClient(name, httpClient =>
        {
            var options = new BerryHttpClientOptions();
            configureOptions(options);
            
            if (!string.IsNullOrWhiteSpace(options.BaseAddress))
            {
                httpClient.BaseAddress = new Uri(options.BaseAddress);
            }
            
            if (options.Timeout.HasValue)
            {
                httpClient.Timeout = options.Timeout.Value;
            }
            
            foreach (var header in options.DefaultHeaders)
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        });

        return services;
    }

    /// <summary>
    /// 添加命名的BerryNet HTTP客户端，支持HttpClientBuilder配置
    /// </summary>
    public static IHttpClientBuilder AddBerryHttpClient(
        this IServiceCollection services,
        string name,
        Action<BerryHttpClientOptions> configureOptions,
        Action<IHttpClientBuilder> configureHttpClient)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));
        
        if (configureHttpClient == null)
            throw new ArgumentNullException(nameof(configureHttpClient));

        // 添加基础服务
        services.AddBerryHttpClient(name, configureOptions);
        
        // 获取HttpClientBuilder并应用配置
        var builder = services.AddHttpClient(name);
        configureHttpClient(builder);
        
        return builder;
    }

    /// <summary>
    /// 添加BerryNet HTTP客户端，支持泛型类型化客户端
    /// </summary>
    public static IServiceCollection AddBerryHttpClient<TClient>(
        this IServiceCollection services,
        Action<BerryHttpClientOptions>? configureOptions = null)
        where TClient : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var clientName = typeof(TClient).Name;
        
        if (configureOptions != null)
        {
            services.AddBerryHttpClient(clientName, configureOptions);
        }
        else
        {
            services.AddBerryHttpClient();
        }
        
        // 注册类型化客户端
        services.TryAddTransient<TClient>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryHttpClientFactory>();
            var httpClient = factory.CreateClient(clientName);
            
            // 尝试通过构造函数创建客户端实例
            var constructors = typeof(TClient).GetConstructors();
            var constructor = constructors.FirstOrDefault(c => 
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && 
                       (parameters[0].ParameterType == typeof(IBerryHttpClient) ||
                        parameters[0].ParameterType == typeof(IBerryHttpClientEnhanced));
            });
            
            if (constructor != null)
            {
                var parameterType = constructor.GetParameters()[0].ParameterType;
                
                if (parameterType == typeof(IBerryHttpClientEnhanced))
                {
                    var enhancedClient = provider.GetRequiredService<IBerryHttpClientEnhanced>();
                    return (TClient)Activator.CreateInstance(typeof(TClient), enhancedClient)!;
                }
                else
                {
                    return (TClient)Activator.CreateInstance(typeof(TClient), httpClient)!;
                }
            }
            
            throw new InvalidOperationException($"类型 {typeof(TClient).Name} 没有合适的构造函数");
        });

        return services;
    }
    
    /// <summary>
    /// 添加支持压缩的BerryNet HTTP客户端服务
    /// </summary>
    public static IServiceCollection AddBerryHttpClientWithCompression(
        this IServiceCollection services,
        Action<BerryHttpClientOptions>? configureOptions = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // 注册基础服务
        services.AddHttpClient();
        services.TryAddSingleton<IHttpContextAccessor, SimpleHttpContextAccessor>();
        
        // 配置选项
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        
        // 配置支持压缩的HttpClient
        services.AddHttpClient("BerryHttpClient", (serviceProvider, httpClient) =>
        {
            var options = new BerryHttpClientOptions();
            configureOptions?.Invoke(options);
            
            if (!string.IsNullOrWhiteSpace(options.BaseAddress))
            {
                httpClient.BaseAddress = new Uri(options.BaseAddress);
            }
            
            if (options.Timeout.HasValue)
            {
                httpClient.Timeout = options.Timeout.Value;
            }
            
            foreach (var header in options.DefaultHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        });
        
        // 注册BerryNet服务
        services.TryAddTransient<IBerryHttpClientFactory, BerryHttpClientFactoryEnhanced>();
        services.TryAddTransient<IBerryHttpClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("BerryHttpClient");
            return new BerryHttpClient(httpClient);
        });
        
        services.TryAddTransient<IBerryHttpClientEnhanced>(provider =>
        {
            var innerClient = provider.GetRequiredService<IBerryHttpClient>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            return new BerryHttpClientEnhanced(innerClient, httpContextAccessor);
        });

        return services;
    }
    
    /// <summary>
    /// 添加命名的支持压缩的BerryNet HTTP客户端
    /// </summary>
    public static IServiceCollection AddBerryHttpClientWithCompression<TClient>(
        this IServiceCollection services,
        string name,
        Action<BerryHttpClientOptions>? configureOptions = null)
        where TClient : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));

        // 添加基础服务
        services.AddBerryHttpClientWithCompression(configureOptions);
        
        // 配置命名HttpClient
        services.AddHttpClient(name, (serviceProvider, httpClient) =>
        {
            var options = new BerryHttpClientOptions();
            configureOptions?.Invoke(options);
            
            if (!string.IsNullOrWhiteSpace(options.BaseAddress))
            {
                httpClient.BaseAddress = new Uri(options.BaseAddress);
            }
            
            if (options.Timeout.HasValue)
            {
                httpClient.Timeout = options.Timeout.Value;
            }
            
            foreach (var header in options.DefaultHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
        });
        
        // 注册类型化客户端
        services.TryAddTransient<TClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(name);
            
            // 尝试不同的构造函数
            var constructors = typeof(TClient).GetConstructors();
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(HttpClient))
                {
                    return (TClient)Activator.CreateInstance(typeof(TClient), httpClient)!;
                }
                
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IBerryHttpClient))
                {
                    var berryClient = new BerryHttpClient(httpClient);
                    return (TClient)Activator.CreateInstance(typeof(TClient), berryClient)!;
                }
                
                if (parameters.Length == 0)
                {
                    return (TClient)Activator.CreateInstance(typeof(TClient))!;
                }
            }
            
            throw new InvalidOperationException($"类型 {typeof(TClient).Name} 没有合适的构造函数");
        });

        return services;
    }
}
