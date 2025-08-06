# BerryNet.Grpc.Dynamic

动态 gRPC 客户端库，支持在运行时调用 gRPC 服务，无需预先生成的 proto 文件代码。

## ✨ 特性

- 🚀 **无需 Proto 文件** - 运行时动态调用 gRPC 服务
- 🔍 **服务发现** - 自动发现可用的服务和方法
- 📝 **动态消息** - 运行时创建和操作 Protobuf 消息
- 🌊 **流式支持** - 支持所有 gRPC 流式模式
- 🔄 **JSON 支持** - 直接使用 JSON 调用服务
- 🏭 **工厂模式** - 便于管理和依赖注入
- ⚡ **高性能** - 基于 Grpc.Net.Client 构建
- 🛡️ **错误处理** - 内置重试和超时机制

## 📦 安装

```bash
dotnet add package Ledon.BerryNet.Grpc.Dynamic
```

## 🚀 快速开始

### 基本使用

```csharp
using Ledon.BerryNet.Grpc.Dynamic;

// 创建动态客户端
var factory = new DynamicGrpcClientFactory();
using var client = factory.CreateClient("https://localhost:5001");

// 使用 JSON 调用服务
var requestJson = """
{
    "name": "World",
    "message": "Hello!"
}
""";

var responseJson = await client.CallUnaryJsonAsync(
    "GreeterService", 
    "SayHello", 
    requestJson);

Console.WriteLine(responseJson);
```

### 使用动态消息

```csharp
// 创建动态消息
var request = await client.CreateMessageAsync("HelloRequest");
request.SetField("name", "Alice")
       .SetField("count", 42);

// 调用服务
var response = await client.CallUnaryAsync("MyService", "Process", request);

// 获取响应字段
var result = response.GetField<string>("result");
var success = response.GetField<bool>("success");
```

### 流式操作

```csharp
// 服务端流式
var request = await client.CreateMessageAsync("StreamRequest");
request.SetField("count", 10);

await foreach (var response in client.CallServerStreamingAsync("StreamService", "GetData", request))
{
    var data = response.GetField<string>("data");
    Console.WriteLine($"Received: {data}");
}

// 客户端流式
async IAsyncEnumerable<DynamicMessage> GenerateRequests()
{
    for (int i = 0; i < 5; i++)
    {
        var req = await client.CreateMessageAsync("DataRequest");
        req.SetField("value", i);
        yield return req;
    }
}

var result = await client.CallClientStreamingAsync("StreamService", "UploadData", GenerateRequests());
```

## 🏗️ 高级配置

### 完整配置

```csharp
using var client = factory.CreateClient("https://localhost:5001", builder =>
{
    builder
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithBearerToken("your-jwt-token")
        .WithHeader("X-Custom-Header", "value")
        .WithCompression("gzip")
        .WithRetry(retry =>
        {
            retry.MaxRetryAttempts = 3;
            retry.InitialBackoff = TimeSpan.FromSeconds(1);
            retry.BackoffMultiplier = 2.0;
        })
        .WithReflection(true)
        .WithChannelOptions(options =>
        {
            options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
        });
});
```

### 依赖注入

```csharp
using Ledon.BerryNet.Grpc.Dynamic.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// 添加默认客户端
services.AddDynamicGrpcClient("https://localhost:5001", builder =>
{
    builder.WithTimeout(TimeSpan.FromSeconds(30))
           .WithCompression("gzip");
});

// 添加命名客户端
services.AddDynamicGrpcClient("user-service", "https://user-service:5001");
services.AddDynamicGrpcClient("order-service", "https://order-service:5001");

var serviceProvider = services.BuildServiceProvider();

// 使用客户端
var defaultClient = serviceProvider.GetRequiredService<IDynamicGrpcClient>();
var userClient = serviceProvider.GetDynamicGrpcClient("user-service");
```

## 🔍 服务发现

```csharp
// 列出所有可用服务
var services = await client.GetAvailableServicesAsync();
foreach (var serviceName in services)
{
    Console.WriteLine($"Service: {serviceName}");
    
    // 获取服务方法
    var methods = await client.GetServiceMethodsAsync(serviceName);
    foreach (var method in methods)
    {
        Console.WriteLine($"  {method.Name}: {method.InputType.Name} -> {method.OutputType.Name}");
    }
}
```

## 📋 API 参考

### IDynamicGrpcClient

- `CallUnaryAsync(service, method, request)` - 调用一元 RPC
- `CallUnaryJsonAsync(service, method, json)` - 使用 JSON 调用一元 RPC
- `CallClientStreamingAsync(service, method, requests)` - 客户端流式 RPC
- `CallServerStreamingAsync(service, method, request)` - 服务端流式 RPC
- `CallBidirectionalStreamingAsync(service, method, requests)` - 双向流式 RPC
- `GetAvailableServicesAsync()` - 获取可用服务列表
- `GetServiceMethodsAsync(service)` - 获取服务方法列表
- `CreateMessageAsync(messageType)` - 创建动态消息

### DynamicMessage

- `SetField(name, value)` - 设置字段值
- `GetField<T>(name)` - 获取字段值
- `ToJson()` - 转换为 JSON
- `FromJson(descriptor, json)` - 从 JSON 创建

## ⚠️ 注意事项

1. **服务器反射**: 目标 gRPC 服务器需要启用反射功能
2. **性能考虑**: 动态调用比静态生成的客户端略慢
3. **类型安全**: 运行时类型检查，建议充分测试
4. **依赖关系**: 需要访问服务器的 proto 定义或反射服务

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

MIT License - 查看 [LICENSE](../../../LICENSE) 文件了解详情。
