using Ledon.BerryNet.Grpc.Dynamic;
using Ledon.BerryNet.Grpc.Dynamic.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Dynamic.Examples;

/// <summary>
/// 动态 gRPC 客户端使用示例
/// </summary>
public static class DynamicGrpcExamples
{
    /// <summary>
    /// 基本使用示例
    /// </summary>
    public static async Task BasicUsageExample()
    {
        // 创建工厂
        var factory = new DynamicGrpcClientFactory();
        
        // 创建客户端
        using var client = factory.CreateClient("https://localhost:5001", builder =>
        {
            builder
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithBearerToken("your-jwt-token")
                .WithCompression("gzip")
                .WithRetry(retry =>
                {
                    retry.MaxRetryAttempts = 3;
                    retry.InitialBackoff = TimeSpan.FromSeconds(1);
                });
        });

        // 获取可用服务
        var services = await client.GetAvailableServicesAsync();
        Console.WriteLine($"Available services: {string.Join(", ", services)}");

        // 调用服务方法 (JSON)
        var requestJson = """
        {
            "name": "World",
            "message": "Hello from dynamic client!"
        }
        """;

        var responseJson = await client.CallUnaryJsonAsync(
            "MyService", 
            "SayHello", 
            requestJson);
        
        Console.WriteLine($"Response: {responseJson}");
    }

    /// <summary>
    /// 使用动态消息的示例
    /// </summary>
    public static async Task DynamicMessageExample()
    {
        var factory = new DynamicGrpcClientFactory();
        using var client = factory.CreateClient("https://localhost:5001");

        // 创建动态消息
        var request = await client.CreateMessageAsync("MyRequest");
        request.SetField("name", "Alice")
               .SetField("age", 30)
               .SetField("email", "alice@example.com");

        // 调用服务
        var response = await client.CallUnaryAsync("UserService", "CreateUser", request);
        
        // 获取响应字段
        var userId = response.GetField<string>("userId");
        var success = response.GetField<bool>("success");
        
        Console.WriteLine($"User created: {userId}, Success: {success}");
    }

    /// <summary>
    /// 流式操作示例
    /// </summary>
    public static async Task StreamingExample()
    {
        var factory = new DynamicGrpcClientFactory();
        using var client = factory.CreateClient("https://localhost:5001");

        // 服务端流式
        var request = await client.CreateMessageAsync("StreamRequest");
        request.SetField("count", 10);

        await foreach (var response in client.CallServerStreamingAsync("StreamService", "GetNumbers", request))
        {
            var number = response.GetField<int>("value");
            Console.WriteLine($"Received: {number}");
        }

        // 客户端流式
        var responses = GenerateRequests(client);
        var result = await client.CallClientStreamingAsync("StreamService", "SumNumbers", responses);
        var sum = result.GetField<long>("sum");
        Console.WriteLine($"Sum: {sum}");
    }

    /// <summary>
    /// 依赖注入示例
    /// </summary>
    public static void DependencyInjectionExample()
    {
        var services = new ServiceCollection();
        
        // 添加日志（简化版本，实际使用中需要添加 Microsoft.Extensions.Logging 包）
        // services.AddLogging(builder => builder.AddConsole());
        
        // 添加动态 gRPC 客户端
        services.AddDynamicGrpcClient("https://localhost:5001", builder =>
        {
            builder
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithCompression("gzip")
                .WithRetry(retry => retry.MaxRetryAttempts = 3);
        });

        // 添加命名客户端
        services.AddDynamicGrpcClient("user-service", "https://user-service:5001");
        services.AddDynamicGrpcClient("order-service", "https://order-service:5001");

        // 简化版本：在实际项目中需要添加 Microsoft.Extensions.DependencyInjection 包
        // var serviceProvider = services.BuildServiceProvider();
        
        Console.WriteLine("Dependency injection configuration completed (simplified version)");
        
        // 实际使用示例：
        // var defaultClient = serviceProvider.GetRequiredService<IDynamicGrpcClient>();
        // var userClient = serviceProvider.GetDynamicGrpcClient("user-service");
        // var orderClient = serviceProvider.GetDynamicGrpcClient("order-service");
    }

    /// <summary>
    /// 服务发现示例
    /// </summary>
    public static async Task ServiceDiscoveryExample()
    {
        var factory = new DynamicGrpcClientFactory();
        using var client = factory.CreateClient("https://localhost:5001");

        // 列出所有服务
        var services = await client.GetAvailableServicesAsync();
        
        foreach (var serviceName in services)
        {
            Console.WriteLine($"\nService: {serviceName}");
            
            // 获取服务方法
            var methods = await client.GetServiceMethodsAsync(serviceName);
            
            foreach (var method in methods)
            {
                var type = GetMethodType(method);
                Console.WriteLine($"  {type} {method.Name}({method.InputType.Name}) -> {method.OutputType.Name}");
            }
        }
    }

    private static async IAsyncEnumerable<Messages.DynamicMessage> GenerateRequests(IDynamicGrpcClient client)
    {
        for (int i = 1; i <= 5; i++)
        {
            var request = await client.CreateMessageAsync("NumberRequest");
            request.SetField("value", i);
            yield return request;
        }
    }

    private static string GetMethodType(Google.Protobuf.Reflection.MethodDescriptor method)
    {
        if (method.IsClientStreaming && method.IsServerStreaming)
            return "Bidirectional";
        if (method.IsClientStreaming)
            return "ClientStreaming";
        if (method.IsServerStreaming)
            return "ServerStreaming";
        return "Unary";
    }
}
