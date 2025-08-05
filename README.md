# BerryNet

# BerryNet

BerryNet 是一个现代化的 .NET HTTP 客户端库生态系统，提供流畅的链式调用API、工厂模式支持、深度的 AspNetCore 集成以及专门的 WebAssembly 优化。

## 🌟 特性

- ✅ **流畅的链式调用API** - 直观且易于使用的方法链
- ✅ **工厂模式支持** - 便于依赖注入和管理
- ✅ **多框架支持** - 同时支持 .NET 7.0、8.0、9.0
- ✅ **AspNetCore深度集成** - 无缝集成到AspNetCore应用中
- ✅ **WASM专项优化** - 专为 Blazor WebAssembly 设计
- ✅ **压缩支持** - Gzip、Deflate、Brotli 多种压缩算法
- ✅ **自动JSON序列化** - 内置JSON支持，可自定义序列化选项
- ✅ **请求头传播** - 自动传播认证信息、相关ID等
- ✅ **类型化客户端** - 强类型API客户端支持
- ✅ **异步支持** - 完整的async/await支持
- ✅ **错误处理** - 内置超时和异常处理
- ✅ **进度回调** - 文件上传下载进度监控

## 📦 项目结构

```
Ledon.BerryNet/
├── src/
│   ├── Ledon.BerryNet/              # 核心库 (.NET 7/8/9)
│   │   ├── Http/                    # HTTP客户端核心实现
│   │   └── Examples/                # 使用示例
│   ├── Ledon.BerryNet.AspNetCore/   # AspNetCore扩展 (.NET 9)
│   │   ├── Http/                    # AspNetCore增强功能
│   │   ├── Options/                 # 配置选项
│   │   ├── Extensions/              # 服务扩展
│   │   └── Examples/                # AspNetCore示例
│   └── Ledon.BerryNet.Wasm/         # WebAssembly优化 (.NET 7/8/9)
│       ├── Http/                    # WASM专用HTTP实现
│       ├── Options/                 # WASM配置选项
│       ├── Extensions/              # Blazor集成扩展
│       └── Examples/                # WASM使用示例
└── docs/                            # 详细文档
```
```

## 🚀 快速开始

### 1. 安装包

```bash
# 核心功能 (适用于所有 .NET 应用)
dotnet add package Ledon.BerryNet

# AspNetCore 集成 (Web 应用)
dotnet add package Ledon.BerryNet.AspNetCore

# WebAssembly 优化 (Blazor WASM)
dotnet add package Ledon.BerryNet.Wasm
```

### 2. 基础使用 (核心包)

```csharp
using Ledon.BerryNet.Http;

// 创建客户端
var client = new BerryHttpClient();

// GET请求 (支持压缩)
var users = await client
    .Get("https://api.example.com/users")
    .WithBearerToken("your-token")
    .WithQueryParameter("page", "1")
    .ExecuteAsync<List<User>>();

// POST请求
var newUser = new { Name = "John", Email = "john@example.com" };
var result = await client
    .Post("https://api.example.com/users")
    .WithJsonBody(newUser)
    .ExecuteAsync<User>();
```

### 3. AspNetCore集成

在 `Program.cs` 中注册服务：

```csharp
using Ledon.BerryNet.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加BerryNet HTTP客户端
builder.Services.AddBerryHttpClient(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DefaultHeaders.Add("User-Agent", "MyApp/1.0");
});

// 添加类型化客户端
builder.Services.AddBerryHttpClient<WeatherApiClient>(options =>
{
    options.BaseAddress = "https://api.openweathermap.org/data/2.5";
});

var app = builder.Build();
```

### 4. Blazor WebAssembly 使用

在 `Program.cs` 中注册服务：

```csharp
using Ledon.BerryNet.Wasm.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 添加WASM优化的HTTP客户端 (支持压缩)
builder.Services.AddBerryWasmHttpClientWithCompression(options =>
{
    options.BaseAddress = builder.HostEnvironment.BaseAddress;
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, deflate, br";
    options.EnableCredentials = true;
});

await builder.Build().RunAsync();
```

在 Blazor 组件中使用：

```razor
@inject IBerryWasmHttpClient HttpClient

@code {
    private async Task LoadDataAsync()
    {
        var data = await HttpClient
            .Get("/api/weather")
            .WithCorsMode("cors")
            .WithCredentials(true)
            .WithBrotliCompression()  // 高压缩率
            .ExecuteAsync<WeatherData[]>();
    }
}
```

在控制器中使用：

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IBerryHttpClientEnhanced _httpClient;

    public UsersController(IBerryHttpClientEnhanced httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // 自动传播认证信息和相关ID
        var users = await _httpClient
            .Get("/users")
            .PropagateAuthentication(HttpContext)
            .PropagateCorrelationId(HttpContext)
            .ExecuteAsync<List<User>>();

        return Ok(users);
    }
}
```

### 5. 类型化客户端

```csharp
public class WeatherApiClient
{
    private readonly IBerryHttpClient _httpClient;

    public WeatherApiClient(IBerryHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherInfo?> GetWeatherAsync(string city)
    {
        return await _httpClient
            .Get("/weather")
            .WithQueryParameter("q", city)
            .WithQueryParameter("appid", "your-api-key")
            .ExecuteAsync<WeatherInfo>();
    }
}
```

## 🎯 选择合适的包

| 应用类型 | 推荐包 | 主要特性 |
|---------|--------|----------|
| **控制台应用** | `Ledon.BerryNet` | 核心HTTP功能、认证、文件处理 |
| **Web API/MVC** | `Ledon.BerryNet.AspNetCore` | DI集成、配置管理、日志集成 |
| **Blazor WASM** | `Ledon.BerryNet.Wasm` | CORS支持、压缩优化、进度回调 |

## 🚀 压缩性能对比

| 压缩算法 | 压缩率 | 速度 | 最佳场景 |
|---------|--------|------|----------|
| **Gzip** | 65% | 快 | 通用API调用 |
| **Deflate** | 62% | 最快 | 实时数据 |
| **Brotli** | 72% | 较慢 | 大文件传输 |

## 📚 文档

详细文档请查看：
- **[生态系统概览](docs/BerryNet-Ecosystem-Overview.md)** - 完整架构说明
- **[核心功能指南](docs/BerryNet-Core-Guide.md)** - 基础使用教程
- **[AspNetCore集成](docs/BerryNet-AspNetCore-Guide.md)** - Web应用集成
- **[WASM优化指南](docs/BerryNet-Wasm-Guide.md)** - Blazor专用功能
- **[快速开始](docs/BerryNet-QuickStart.md)** - 5分钟上手指南

## 🔧 运行示例

```bash
# 克隆仓库
git clone https://github.com/wosledon/ledon-berry-net.git
cd ledon-berry-net

# 编译所有项目 (多框架支持)
dotnet build

# 运行基础示例
dotnet run --project src/Ledon.BerryNet

# 运行WASM示例
dotnet run --project src/Ledon.BerryNet.Wasm
```

## ✅ 完成的功能总结

### 🎯 多框架全面支持
- **✅ .NET 7.0** - 完全支持，适用于长期项目
- **✅ .NET 8.0** - 完全支持，LTS版本  
- **✅ .NET 9.0** - 完全支持，最新特性

所有三个包现在都支持多目标框架：
- `Ledon.BerryNet` (核心) - .NET 7/8/9
- `Ledon.BerryNet.AspNetCore` - .NET 7/8/9  
- `Ledon.BerryNet.Wasm` - .NET 7/8/9

### 🗜️ 压缩功能全面支持
- **✅ Gzip压缩** - 65%压缩率，快速处理，最佳兼容性
- **✅ Deflate压缩** - 62%压缩率，最快速度，实时数据首选
- **✅ Brotli压缩** - 72%压缩率，最高效率，现代浏览器支持
- **✅ 组合压缩** - 多算法优先级选择，让服务器决定最优方案
- **✅ 自动解压缩** - HttpClientHandler自动处理响应解压

### 🚀 使用示例

```csharp
// 1. 核心包 - 快速压缩
var httpClient = factory.CreateClientWithCompression();
var data = await httpClient
    .Get("https://api.example.com/data")
    .WithGzipCompression()
    .ExecuteAsync<DataModel>();

// 2. AspNetCore - 高级压缩配置
services.AddBerryHttpClientWithCompression(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.EnableCompression = true;
});

// 3. WASM - 优化带宽
builder.Services.AddBerryWasmHttpClientWithCompression(options =>
{
    options.CompressionEncodings = "br, gzip, deflate";
    options.AutoDecompression = true;
});
```

欢迎提交问题和拉取请求！

## 📄 许可证

本项目采用 MIT 许可证。详情请查看 [LICENSE](LICENSE) 文件。

## 🏷️ 版本

当前版本：1.0.0-preview

支持的目标框架：
- .NET 7.0
- .NET 8.0  
- .NET 9.0

## 📞 联系

如有问题或建议，请提交 [Issue](https://github.com/wosledon/ledon-berry-net/issues)。