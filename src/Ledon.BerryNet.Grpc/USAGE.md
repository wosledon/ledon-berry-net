# BerryNet gRPC 使用示例

## 基本用法

```csharp
using Ledon.BerryNet.Grpc;
using Ledon.BerryNet.Grpc.Extensions;

// 创建 gRPC 客户端工厂
var factory = new BerryGrpcClientFactory();

// 使用链式调用创建客户端
var client = factory.CreateBuilder("https://localhost:5001")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithBearerToken("your-jwt-token")
    .WithCompression()
    .WithRetry(retry =>
    {
        retry.MaxRetryAttempts = 3;
        retry.InitialBackoff = TimeSpan.FromSeconds(1);
    })
    .CreateClient<YourService.YourServiceClient>();

// 调用 gRPC 服务
var request = new YourRequest { Message = "Hello, gRPC!" };
var response = await client.YourMethodAsync(request);
```

## 依赖注入

```csharp
using Microsoft.Extensions.DependencyInjection;
using Ledon.BerryNet.Grpc.Extensions;

var services = new ServiceCollection();

// 注册 gRPC 客户端
services.AddBerryGrpcClient<YourService.YourServiceClient>(
    "https://localhost:5001",
    builder => builder
        .WithTimeout(TimeSpan.FromSeconds(30))
        .WithCompression()
);

// 在服务中使用
public class MyService
{
    private readonly YourService.YourServiceClient _grpcClient;

    public MyService(YourService.YourServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task<string> CallGrpcServiceAsync()
    {
        var request = new YourRequest { Message = "Hello from DI!" };
        var response = await _grpcClient.YourMethodAsync(request);
        return response.Message;
    }
}
```

## 流式操作

```csharp
using Ledon.BerryNet.Grpc.Extensions;

// 服务器流式调用
var streamingCall = client.GetStreamAsync(new StreamRequest());

// 使用扩展方法处理流
await streamingCall.ResponseStream.ForEachAsync(response =>
{
    Console.WriteLine($"收到: {response.Message}");
});

// 或转换为列表
var allResponses = await streamingCall.ResponseStream.ToListAsync();
```

## 特性总结

✅ **多框架支持**: .NET 7.0/8.0/9.0  
✅ **链式调用**: 流畅的 API 设计  
✅ **工厂模式**: 统一的客户端创建  
✅ **依赖注入**: 完整的 DI 集成  
✅ **认证支持**: Bearer Token 和自定义头  
✅ **压缩支持**: gzip 等压缩算法  
✅ **重试机制**: 可配置的重试策略  
✅ **流式扩展**: 丰富的流处理方法  
✅ **错误处理**: 详细的异常处理  

构建成功！所有框架版本都正常工作。
