using System.Text.Json;
using System.Net;
using Ledon.BerryNet.Http;
using Ledon.BerryNet.Wasm.Http;
using Ledon.BerryNet.Wasm.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Wasm.Extensions;

/// <summary>
/// BerryNet WASM 服务注册扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加BerryNet WASM HTTP客户端服务
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClient(this IServiceCollection services)
    {
        return services.AddBerryWasmHttpClient(_ => { });
    }

    /// <summary>
    /// 添加BerryNet WASM HTTP客户端服务，支持配置
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClient(
        this IServiceCollection services,
        Action<BerryWasmHttpClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // 注册基础服务
        services.AddHttpClient();
        services.AddLogging();
        
        // 配置选项
        services.Configure(configureOptions);
        
        // 注册BerryNet WASM服务
        services.TryAddTransient<IBerryWasmHttpClientFactory, BerryWasmHttpClientFactory>();
        services.TryAddTransient<IBerryWasmHttpClient>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryWasmHttpClientFactory>();
            return factory.CreateClient();
        });
        
        // 也注册基础接口实现
        services.TryAddTransient<IBerryHttpClientFactory>(provider =>
            provider.GetRequiredService<IBerryWasmHttpClientFactory>());
        services.TryAddTransient<IBerryHttpClient>(provider =>
            provider.GetRequiredService<IBerryWasmHttpClient>());

        return services;
    }

    /// <summary>
    /// 添加命名的BerryNet WASM HTTP客户端
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClient(
        this IServiceCollection services,
        string name,
        Action<BerryWasmHttpClientOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("客户端名称不能为空", nameof(name));
        
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // 添加基础服务
        services.AddBerryWasmHttpClient();
        
        // 配置命名选项
        services.Configure(name, configureOptions);
        
        // 配置命名HttpClient
        services.AddHttpClient(name, httpClient =>
        {
            var options = new BerryWasmHttpClientOptions();
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
    /// 添加命名的BerryNet WASM HTTP客户端，支持HttpClientBuilder配置
    /// </summary>
    public static IHttpClientBuilder AddBerryWasmHttpClient(
        this IServiceCollection services,
        string name,
        Action<BerryWasmHttpClientOptions> configureOptions,
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
        services.AddBerryWasmHttpClient(name, configureOptions);
        
        // 获取HttpClientBuilder并应用配置
        var builder = services.AddHttpClient(name);
        configureHttpClient(builder);
        
        return builder;
    }

    /// <summary>
    /// 添加BerryNet WASM HTTP客户端，支持泛型类型化客户端
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClient<TClient>(
        this IServiceCollection services,
        Action<BerryWasmHttpClientOptions>? configureOptions = null)
        where TClient : class
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var clientName = typeof(TClient).Name;
        
        if (configureOptions != null)
        {
            services.AddBerryWasmHttpClient(clientName, configureOptions);
        }
        else
        {
            services.AddBerryWasmHttpClient();
        }
        
        // 注册类型化客户端
        services.TryAddTransient<TClient>(provider =>
        {
            var factory = provider.GetRequiredService<IBerryWasmHttpClientFactory>();
            var httpClient = factory.CreateClient(clientName);
            
            // 尝试通过构造函数创建客户端实例
            var constructors = typeof(TClient).GetConstructors();
            var constructor = constructors.FirstOrDefault(c => 
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && 
                       (parameters[0].ParameterType == typeof(IBerryHttpClient) ||
                        parameters[0].ParameterType == typeof(IBerryWasmHttpClient));
            });
            
            if (constructor != null)
            {
                var parameterType = constructor.GetParameters()[0].ParameterType;
                
                if (parameterType == typeof(IBerryWasmHttpClient))
                {
                    return (TClient)Activator.CreateInstance(typeof(TClient), httpClient)!;
                }
                else
                {
                    // 向后兼容性
                    return (TClient)Activator.CreateInstance(typeof(TClient), (IBerryHttpClient)httpClient)!;
                }
            }
            
            throw new InvalidOperationException($"类型 {typeof(TClient).Name} 没有合适的构造函数");
        });

        return services;
    }

    /// <summary>
    /// 添加用于Blazor WebAssembly的默认配置
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClientForBlazor(
        this IServiceCollection services,
        string? baseAddress = null)
    {
        return services.AddBerryWasmHttpClient(options =>
        {
            if (!string.IsNullOrEmpty(baseAddress))
            {
                options.BaseAddress = baseAddress;
            }
            
            options.EnableCredentials = true;
            options.CacheMode = "no-cache";
            options.EnableCompression = true;
            options.CompressionEncodings = new[] { "gzip", "deflate", "br" };
            options.AutoDecompression = true;
            options.DefaultHeaders.Add("Accept", "application/json");
            options.DefaultHeaders.Add("Content-Type", "application/json");
            
            // WASM特定的JSON配置
            options.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        });
    }

    /// <summary>
    /// 配置HttpClient以支持压缩
    /// </summary>
    public static IServiceCollection ConfigureWasmCompression(
        this IServiceCollection services,
        Action<HttpClientHandler>? configureHandler = null)
    {
        services.AddHttpClient("BerryWasmDefault").ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();

            // 启用自动解压缩
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

#if NET8_0_OR_GREATER
                handler.AutomaticDecompression |= DecompressionMethods.Brotli;
#endif
            }

            configureHandler?.Invoke(handler);

            return handler;
        });

        return services;
    }

    /// <summary>
    /// 添加完整的Blazor WASM支持（包括压缩）
    /// </summary>
    public static IServiceCollection AddBerryWasmHttpClientWithCompression(
        this IServiceCollection services,
        string? baseAddress = null,
        Action<BerryWasmHttpClientOptions>? configureOptions = null)
    {
        // 添加基础WASM客户端
        services.AddBerryWasmHttpClientForBlazor(baseAddress);
        
        // 配置压缩
        services.ConfigureWasmCompression();
        
        // 应用额外配置
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        return services;
    }
}
