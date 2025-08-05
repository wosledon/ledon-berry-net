using Ledon.BerryNet.Examples;

Console.WriteLine("=== BerryNet HTTP客户端测试 ===");

Console.WriteLine("\n1. 基础GET请求示例:");
await HttpClientExamples.BasicGetExample();

Console.WriteLine("\n2. POST请求示例:");
await HttpClientExamples.PostExample();

Console.WriteLine("\n3. 工厂模式示例:");
await HttpClientExamples.FactoryExample();

Console.WriteLine("\n4. 复杂链式调用示例:");
await HttpClientExamples.ComplexChainExample();

Console.WriteLine("\n5. 压缩功能示例:");
await CompressionExample.RunAsync();

Console.WriteLine("\n=== 测试完成 ===");
