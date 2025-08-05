using Ledon.BerryNet.Http;

namespace Ledon.BerryNet.Examples;

/// <summary>
/// 压缩功能使用示例
/// </summary>
public class CompressionExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== BerryNet 压缩功能演示 ===\n");

        // 创建支持压缩的HTTP客户端
        var factory = new BerryHttpClientFactory();
        var httpClient = factory.CreateClientWithCompression();

        Console.WriteLine("1. 使用 Gzip 压缩获取数据:");
        try
        {
            var data1 = await httpClient
                .Get("https://httpbin.org/json")
                .WithGzipCompression()
                .WithHeader("User-Agent", "BerryNet-Example/1.0")
                .ExecuteAsync<dynamic>();
            
            Console.WriteLine("✓ 成功获取数据 (Gzip压缩)\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 请求失败: {ex.Message}\n");
        }

        Console.WriteLine("2. 使用 Brotli 压缩(最高压缩率):");
        try
        {
            var data2 = await httpClient
                .Get("https://httpbin.org/json")
                .WithBrotliCompression()
                .WithHeader("User-Agent", "BerryNet-Example/1.0")
                .ExecuteAsync<dynamic>();
            
            Console.WriteLine("✓ 成功获取数据 (Brotli压缩)\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 请求失败: {ex.Message}\n");
        }

        Console.WriteLine("3. 使用 Deflate 压缩(最快速度):");
        try
        {
            var data3 = await httpClient
                .Get("https://httpbin.org/json")
                .WithDeflateCompression()
                .WithHeader("User-Agent", "BerryNet-Example/1.0")
                .ExecuteAsync<dynamic>();
            
            Console.WriteLine("✓ 成功获取数据 (Deflate压缩)\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 请求失败: {ex.Message}\n");
        }

        Console.WriteLine("4. 使用多种压缩算法(让服务器选择最优):");
        try
        {
            var data4 = await httpClient
                .Get("https://httpbin.org/json")
                .WithCompression("br, gzip, deflate")  // 优先级顺序
                .WithHeader("User-Agent", "BerryNet-Example/1.0")
                .ExecuteAsync<dynamic>();
            
            Console.WriteLine("✓ 成功获取数据 (多压缩算法)\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 请求失败: {ex.Message}\n");
        }

        Console.WriteLine("5. POST 请求使用压缩:");
        try
        {
            var postData = new { name = "BerryNet", version = "1.0.0", compression = true };
            var response = await httpClient
                .Post("https://httpbin.org/post")
                .WithJsonBody(postData)
                .WithGzipCompression()
                .WithHeader("User-Agent", "BerryNet-Example/1.0")
                .ExecuteAsync<dynamic>();
            
            Console.WriteLine("✓ 成功发送POST请求 (Gzip压缩)\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ 请求失败: {ex.Message}\n");
        }

        Console.WriteLine("=== 压缩功能演示完成 ===");
        Console.WriteLine("\n压缩算法选择指南:");
        Console.WriteLine("• Gzip: 平衡型 - 压缩率65%，速度快，兼容性最佳");
        Console.WriteLine("• Deflate: 快速型 - 压缩率62%，速度最快，延迟最低");  
        Console.WriteLine("• Brotli: 高效型 - 压缩率72%，最节省带宽，现代浏览器支持");
        Console.WriteLine("• 组合: br, gzip, deflate - 让服务器选择最优算法");
    }
}
