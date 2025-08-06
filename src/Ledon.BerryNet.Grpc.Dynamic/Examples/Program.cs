using Ledon.BerryNet.Grpc.Dynamic;
using Ledon.BerryNet.Grpc.Dynamic.Examples;

// 动态 gRPC 客户端示例程序
Console.WriteLine("🍓 BerryNet Dynamic gRPC Client Examples");
Console.WriteLine("========================================");

try
{
    // 运行基本使用示例
    Console.WriteLine("\n1. Basic Usage Example:");
    await DynamicGrpcExamples.BasicUsageExample();

    // 运行动态消息示例  
    Console.WriteLine("\n2. Dynamic Message Example:");
    await DynamicGrpcExamples.DynamicMessageExample();

    // 运行流式操作示例
    Console.WriteLine("\n3. Streaming Example:");
    await DynamicGrpcExamples.StreamingExample();

    // 运行服务发现示例
    Console.WriteLine("\n4. Service Discovery Example:");
    await DynamicGrpcExamples.ServiceDiscoveryExample();

    // 展示依赖注入配置
    Console.WriteLine("\n5. Dependency Injection Configuration:");
    DynamicGrpcExamples.DependencyInjectionExample();
    Console.WriteLine("Dependency injection configured successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine($"Note: These examples require a running gRPC server with reflection enabled.");
}

Console.WriteLine("\n✅ Examples completed!");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
