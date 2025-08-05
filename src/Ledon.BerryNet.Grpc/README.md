# Ledon.BerryNet.Grpc

BerryNet gRPC 扩展库，提供链式调用和工厂模式的 gRPC 客户端封装。

## 特性

- 🔗 **链式调用** - 支持流畅的 API 调用方式
- 🏭 **工厂模式** - 统一的客户端创建和管理
- 🔄 **自动重试** - 可配置的重试策略
- 🗜️ **压缩支持** - 内置 gzip、deflate、brotli 压缩
- 🔐 **认证支持** - Bearer Token 和自定义认证头
- 📊 **流式操作** - 丰富的流式调用扩展方法
- 🏗️ **依赖注入** - 完整的 ASP.NET Core DI 集成
- 📝 **详细日志** - 内置日志记录支持
- 🎯 **多框架支持** - 支持 .NET 7.0/8.0/9.0

## 安装

```bash
dotnet add package Ledon.BerryNet.Grpc
```

## 快速开始

### 1. 基本使用

```csharp
using Ledon.BerryNet.Grpc;

// 创建工厂
var factory = new BerryGrpcClientFactory();

// 创建客户端
var client = factory.CreateBuilder("https://localhost:5001")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithMetadata("api-key", "your-api-key")
    .WithCompression()
    .CreateClient<ExampleService.ExampleServiceClient>();

// 调用服务
var request = new GetMessageRequest { Id = "123" };
var response = await client.GetMessageAsync(request);
```

### 2. 链式调用

```csharp
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
```

### 3. 依赖注入

```csharp
using Ledon.BerryNet.Grpc.Extensions;

// 在 Program.cs 或 Startup.cs 中注册服务
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

// 在控制器或服务中使用
public class MyController : ControllerBase
{
    private readonly ExampleService.ExampleServiceClient _client;

    public MyController(ExampleService.ExampleServiceClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessage(string id)
    {
        var response = await _client.GetMessageAsync(new GetMessageRequest { Id = id });
        return Ok(response);
    }
}
```

### 4. 流式操作

```csharp
using Ledon.BerryNet.Grpc.Extensions;

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

// 客户端流式调用
using var call = client.SendMessages();
var messages = new[]
{
    new SendMessageRequest { Content = "Hello", Sender = "User1" },
    new SendMessageRequest { Content = "World", Sender = "User2" }
};

await call.RequestStream.WriteAllAsync(messages);
await call.RequestStream.CompleteAsync();
var response = await call;
```

## 高级功能

### 重试策略

```csharp
var client = factory.CreateBuilder("https://localhost:5001")
    .WithRetry(retry =>
    {
        retry.MaxRetryAttempts = 5;
        retry.InitialBackoff = TimeSpan.FromSeconds(1);
        retry.MaxBackoff = TimeSpan.FromSeconds(10);
        retry.BackoffMultiplier = 2.0;
        retry.RetryableStatusCodes.Add(StatusCode.Unavailable);
    })
    .CreateClient<ExampleService.ExampleServiceClient>();
```

### 认证配置

```csharp
// Bearer Token
var client = factory.CreateBuilder("https://localhost:5001")
    .WithBearerToken("your-jwt-token")
    .CreateClient<ExampleService.ExampleServiceClient>();

// 自定义认证头
var client = factory.CreateBuilder("https://localhost:5001")
    .WithAuthToken("custom-auth-token")
    .WithMetadata("x-api-key", "your-api-key")
    .CreateClient<ExampleService.ExampleServiceClient>();
```

### 压缩配置

```csharp
// 使用 gzip 压缩
var client = factory.CreateBuilder("https://localhost:5001")
    .WithCompression()  // 默认使用 gzip
    .CreateClient<ExampleService.ExampleServiceClient>();

// 指定压缩算法
var client = factory.CreateBuilder("https://localhost:5001")
    .WithCompression("deflate")
    .CreateClient<ExampleService.ExampleServiceClient>();
```

### TLS 配置

```csharp
// 启用 TLS
var client = factory.CreateBuilder("https://localhost:5001")
    .WithTls()
    .CreateClient<ExampleService.ExampleServiceClient>();

// 禁用 TLS（用于开发环境）
var client = factory.CreateBuilder("http://localhost:5000")
    .WithoutTls()
    .CreateClient<ExampleService.ExampleServiceClient>();
```

### 命名客户端

```csharp
// 注册多个命名客户端
services.AddBerryGrpcClient<ExampleService.ExampleServiceClient>(
    "primary", 
    "https://primary.service.com"
);

services.AddBerryGrpcClient<ExampleService.ExampleServiceClient>(
    "backup", 
    "https://backup.service.com"
);

// 使用命名客户端
public class MyService
{
    private readonly ExampleService.ExampleServiceClient _primaryClient;
    private readonly ExampleService.ExampleServiceClient _backupClient;

    public MyService(IServiceProvider serviceProvider)
    {
        _primaryClient = serviceProvider.GetRequiredKeyedService<ExampleService.ExampleServiceClient>("primary");
        _backupClient = serviceProvider.GetRequiredKeyedService<ExampleService.ExampleServiceClient>("backup");
    }
}
```

### 错误处理

```csharp
try
{
    var response = await client.GetMessageAsync(request);
}
catch (RpcException ex)
{
    switch (ex.StatusCode)
    {
        case StatusCode.NotFound:
            // 处理资源未找到
            break;
        case StatusCode.Unauthenticated:
            // 处理身份验证失败
            break;
        case StatusCode.PermissionDenied:
            // 处理权限不足
            break;
        case StatusCode.Unavailable:
            // 处理服务不可用
            break;
    }
}
```

### 带进度报告的流处理

```csharp
var progress = new Progress<int>(processedCount =>
{
    Console.WriteLine($"已处理 {processedCount} 条消息");
});

await streamingCall.ResponseStream.ProcessWithProgressAsync(
    message => Console.WriteLine($"处理消息: {message.Content}"),
    progress
);
```

## API 参考

### IBerryGrpcClientBuilder

主要方法：

- `WithAddress(string|Uri)` - 设置服务地址
- `WithChannelOptions(Action<GrpcChannelOptions>)` - 配置通道选项
- `WithTimeout(TimeSpan)` - 设置超时时间
- `WithDeadline(DateTime)` - 设置截止时间
- `WithMetadata(string, string)` - 添加元数据
- `WithAuthToken(string)` - 设置认证 Token
- `WithBearerToken(string)` - 设置 Bearer Token
- `WithCompression(string)` - 启用压缩
- `WithRetry(Action<RetryPolicy>)` - 配置重试策略
- `WithTls()` - 启用 TLS
- `WithoutTls()` - 禁用 TLS
- `CreateChannel()` - 创建 gRPC 通道
- `CreateClient<T>()` - 创建客户端

### IBerryGrpcClientFactory

主要方法：

- `CreateBuilder()` - 创建构建器
- `CreateClient<T>(string)` - 直接创建客户端
- `CreateClientWithAuth<T>(string, string)` - 创建带认证的客户端
- `CreateClientWithBearerToken<T>(string, string)` - 创建带 Bearer Token 的客户端
- `CreateClientWithCompression<T>(string, string)` - 创建带压缩的客户端

### 流式扩展方法

- `ToAsyncEnumerable<T>()` - 转换为异步枚举
- `ToListAsync<T>()` - 转换为列表
- `ForEachAsync<T>()` - 对每个元素执行操作
- `FirstAsync<T>()` - 获取第一个元素
- `CountAsync<T>()` - 计算元素数量
- `WriteAllAsync<T>()` - 批量写入
- `ProcessWithProgressAsync<T>()` - 带进度报告的处理
- `ProcessWithErrorHandlingAsync<T>()` - 带错误处理的处理

## 许可证

MIT License
