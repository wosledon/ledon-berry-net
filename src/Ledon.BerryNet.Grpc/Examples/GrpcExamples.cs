using System;
using System.Threading.Tasks;
using Grpc.Core;
using Ledon.BerryNet.Grpc;
using Ledon.BerryNet.Grpc.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Examples;

/// <summary>
/// BerryNet gRPC 使用示例
/// </summary>
public class GrpcExamples
{
    /// <summary>
    /// 基本使用示例
    /// </summary>
    public static async Task BasicUsageExample()
    {
        // 1. 使用工厂模式创建客户端
        var factory = new BerryGrpcClientFactory();
        
        // 创建 gRPC 客户端
        var client = factory.CreateBuilder("https://localhost:5001")
            .WithTimeout(TimeSpan.FromSeconds(30))
            .WithMetadata("api-key", "your-api-key")
            .WithCompression()
            .CreateClient<ExampleService.ExampleServiceClient>();

        // 调用服务
        var request = new GetMessageRequest { Id = "123" };
        var response = await client.GetMessageAsync(request);
        
        Console.WriteLine($"收到消息: {response.Content}");
    }

    /// <summary>
    /// 链式调用示例
    /// </summary>
    public static async Task FluentApiExample()
    {
        var factory = new BerryGrpcClientFactory();
        
        // 链式配置并调用
        var response = await factory.CreateBuilder("https://localhost:5001")
            .WithBearerToken("your-jwt-token")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithRetry(retry =>
            {
                retry.MaxRetryAttempts = 3;
                retry.InitialBackoff = TimeSpan.FromSeconds(1);
            })
            .WithCompression("gzip")
            .CreateClient<ExampleService.ExampleServiceClient>()
            .GetMessageAsync(new GetMessageRequest { Id = "456" });

        Console.WriteLine($"响应: {response.Content}");
    }

    /// <summary>
    /// 依赖注入示例
    /// </summary>
    public static void DependencyInjectionExample()
    {
        var services = new ServiceCollection();
        
        // 注册 BerryNet gRPC 客户端
        services.AddBerryGrpcClient<ExampleService.ExampleServiceClient>(
            "https://localhost:5001",
            builder => builder
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithCompression()
                .WithRetry(retry => retry.MaxRetryAttempts = 3)
        );

        // 注册带认证的客户端
        services.AddBerryGrpcClientWithBearerToken<ExampleService.ExampleServiceClient>(
            "https://localhost:5001", 
            "your-jwt-token"
        );

        // 注册命名客户端
        services.AddBerryGrpcClient<ExampleService.ExampleServiceClient>(
            "primary", 
            "https://primary.service.com"
        );

        services.AddBerryGrpcClient<ExampleService.ExampleServiceClient>(
            "backup", 
            "https://backup.service.com"
        );

        var serviceProvider = services.BuildServiceProvider();
        
        // 使用客户端
        var client = serviceProvider.GetRequiredService<ExampleService.ExampleServiceClient>();
#if NET8_0_OR_GREATER
        var primaryClient = serviceProvider.GetRequiredKeyedService<ExampleService.ExampleServiceClient>("primary");
        var backupClient = serviceProvider.GetRequiredKeyedService<ExampleService.ExampleServiceClient>("backup");
#else
        // 在 .NET 7.0 中，只能使用普通的服务获取
        var primaryClient = serviceProvider.GetRequiredService<ExampleService.ExampleServiceClient>();
        var backupClient = serviceProvider.GetRequiredService<ExampleService.ExampleServiceClient>();
#endif
    }

    /// <summary>
    /// 流式调用示例
    /// </summary>
    public static async Task StreamingExample()
    {
        var factory = new BerryGrpcClientFactory();
        var client = factory.CreateClient<ExampleService.ExampleServiceClient>("https://localhost:5001");

        // 服务器流式调用
        var streamingCall = client.GetMessages(new GetMessagesRequest 
        { 
            Ids = { "1", "2", "3" },
            Limit = 10
        });

        // 使用扩展方法处理流
        await streamingCall.ResponseStream.ForEachAsync(message =>
        {
            Console.WriteLine($"收到消息: {message.Content}");
        });

        // 转换为列表
        var messages = await streamingCall.ResponseStream.ToListAsync();
        Console.WriteLine($"总共收到 {messages.Count} 条消息");
    }

    /// <summary>
    /// 客户端流式调用示例
    /// </summary>
    public static async Task ClientStreamingExample()
    {
        var factory = new BerryGrpcClientFactory();
        var client = factory.CreateClient<ExampleService.ExampleServiceClient>("https://localhost:5001");

        using var call = client.SendMessages();

        // 发送多条消息
        var messages = new[]
        {
            new SendMessageRequest { Content = "Hello", Sender = "User1" },
            new SendMessageRequest { Content = "World", Sender = "User2" },
            new SendMessageRequest { Content = "gRPC", Sender = "User3" }
        };

        // 使用扩展方法批量写入
        await call.RequestStream.WriteAllAsync(messages);
        await call.RequestStream.CompleteAsync();

        var response = await call;
        Console.WriteLine($"发送结果: {response.Success}, 消息ID: {response.MessageId}");
    }

    /// <summary>
    /// 双向流式调用示例
    /// </summary>
    public static async Task BidirectionalStreamingExample()
    {
        var factory = new BerryGrpcClientFactory();
        var client = factory.CreateClient<ExampleService.ExampleServiceClient>("https://localhost:5001");

        using var call = client.Chat();

        // 启动接收任务
        var receiveTask = Task.Run(async () =>
        {
            await call.ResponseStream.ForEachAsync(message =>
            {
                Console.WriteLine($"收到聊天消息: {message.Content} (来自: {message.Sender})");
            });
        });

        // 发送消息
        await call.RequestStream.WriteAsync(new ChatMessage
        {
            Content = "Hello, everyone!",
            Sender = "User1",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        await call.RequestStream.WriteAsync(new ChatMessage
        {
            Content = "How are you doing?",
            Sender = "User1",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        await call.RequestStream.CompleteAsync();
        await receiveTask;
    }

    /// <summary>
    /// 错误处理示例
    /// </summary>
    public static async Task ErrorHandlingExample()
    {
        var factory = new BerryGrpcClientFactory();
        
        try
        {
            var client = factory.CreateBuilder("https://localhost:5001")
                .WithTimeout(TimeSpan.FromSeconds(5))
                .WithRetry(retry =>
                {
                    retry.MaxRetryAttempts = 3;
                    retry.InitialBackoff = TimeSpan.FromSeconds(1);
                    retry.RetryableStatusCodes.Add(StatusCode.Unavailable);
                })
                .CreateClient<ExampleService.ExampleServiceClient>();

            var response = await client.GetMessageAsync(new GetMessageRequest { Id = "test" });
            Console.WriteLine($"成功: {response.Content}");
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"gRPC 错误: {ex.StatusCode} - {ex.Message}");
            
            // 根据状态码处理不同的错误
            switch (ex.StatusCode)
            {
                case StatusCode.NotFound:
                    Console.WriteLine("资源未找到");
                    break;
                case StatusCode.Unauthenticated:
                    Console.WriteLine("身份验证失败");
                    break;
                case StatusCode.PermissionDenied:
                    Console.WriteLine("权限不足");
                    break;
                case StatusCode.Unavailable:
                    Console.WriteLine("服务不可用");
                    break;
                default:
                    Console.WriteLine("其他错误");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"一般错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 带进度报告的流处理示例
    /// </summary>
    public static async Task ProgressReportingExample()
    {
        var factory = new BerryGrpcClientFactory();
        var client = factory.CreateClient<ExampleService.ExampleServiceClient>("https://localhost:5001");

        var progress = new Progress<int>(processedCount =>
        {
            Console.WriteLine($"已处理 {processedCount} 条消息");
        });

        var streamingCall = client.GetMessages(new GetMessagesRequest 
        { 
            Ids = { "1", "2", "3", "4", "5" },
            Limit = 100
        });

        await streamingCall.ResponseStream.ProcessWithProgressAsync(
            message => Console.WriteLine($"处理消息: {message.Content}"),
            progress
        );
    }
}
