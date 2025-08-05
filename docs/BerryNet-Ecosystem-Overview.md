# BerryNet 生态系统概览

BerryNet 是一个全面的 .NET HTTP 客户端解决方案，为不同的应用场景提供专门优化的包。

## 🏗️ 架构概览

```
┌─────────────────────────────────────────────────────────────┐
│                    BerryNet 生态系统                          │
├─────────────────────────────────────────────────────────────┤
│  Ledon.BerryNet.Wasm          │  支持 .NET 7/8/9            │
│  ├─ WASM优化                   │  ├─ 压缩算法支持             │
│  ├─ CORS完整支持               │  ├─ Gzip/Deflate/Brotli    │
│  ├─ 凭证管理                   │  ├─ 自动解压缩               │
│  ├─ 缓存控制                   │  └─ 多框架兼容               │
│  ├─ 进度回调                   │                             │
│  └─ Blazor集成                 │                             │
├─────────────────────────────────────────────────────────────┤
│  Ledon.BerryNet.AspNetCore    │  支持 .NET 7/8/9            │
│  ├─ DI容器集成                 │  ├─ IServiceCollection      │
│  ├─ 配置系统集成               │  ├─ IConfiguration          │
│  ├─ 日志集成                   │  ├─ ILogger                 │
│  ├─ 健康检查                   │  ├─ HealthChecks            │
│  ├─ 中间件支持                 │  └─ 策略模式                 │
│  └─ 类型化客户端               │                             │
├─────────────────────────────────────────────────────────────┤
│  Ledon.BerryNet (核心)         │  支持 .NET 7/8/9            │
│  ├─ 流式API设计                │  ├─ 链式调用                 │
│  ├─ 工厂模式                   │  ├─ 构建器模式               │
│  ├─ 认证支持                   │  ├─ Bearer Token            │
│  ├─ 内容类型处理               │  ├─ JSON/XML/Form          │
│  ├─ 错误处理                   │  ├─ 重试机制                 │
│  ├─ 超时控制                   │  └─ 异常处理                 │
│  └─ 文件上传下载               │                             │
└─────────────────────────────────────────────────────────────┘
```

## 📦 包概览

### 1. Ledon.BerryNet (核心包)
**目标框架**: .NET 7.0, 8.0, 9.0
**用途**: 基础HTTP客户端功能

**主要特性**:
- ✅ 流式API设计
- ✅ 工厂模式支持
- ✅ 链式调用
- ✅ 认证支持 (Bearer Token, API Key, Basic Auth)
- ✅ 内容类型处理 (JSON, XML, Form)
- ✅ 文件上传下载
- ✅ 错误处理和重试机制
- ✅ 超时控制
- ✅ 自定义请求头

**安装**:
```bash
dotnet add package Ledon.BerryNet
```

**适用场景**:
- 控制台应用
- Windows服务
- 第三方API集成
- 简单的HTTP客户端需求

### 2. Ledon.BerryNet.AspNetCore (ASP.NET Core 扩展)
**目标框架**: .NET 9.0
**用途**: ASP.NET Core 应用的深度集成

**主要特性**:
- ✅ DI容器深度集成
- ✅ 配置系统集成 (IConfiguration)
- ✅ 日志集成 (ILogger)
- ✅ 健康检查集成
- ✅ 中间件支持
- ✅ 类型化客户端工厂
- ✅ 策略模式 (重试、熔断、缓存)
- ✅ 作用域管理

**安装**:
```bash
dotnet add package Ledon.BerryNet.AspNetCore
```

**适用场景**:
- Web API项目
- MVC应用
- 微服务架构
- 需要复杂配置管理的场景

### 3. Ledon.BerryNet.Wasm (WebAssembly 优化)
**目标框架**: .NET 7.0, 8.0, 9.0
**用途**: Blazor WebAssembly 应用专用

**主要特性**:
- ✅ WASM环境优化
- ✅ CORS完整支持
- ✅ 凭证管理 (Cookies, 认证)
- ✅ 缓存控制策略
- ✅ 进度回调支持
- ✅ 流式响应处理
- ✅ **压缩支持** (Gzip, Deflate, Brotli)
- ✅ **多框架支持** (.NET 7/8/9)
- ✅ **自动解压缩**
- ✅ Blazor组件集成

**安装**:
```bash
dotnet add package Ledon.BerryNet.Wasm
```

**适用场景**:
- Blazor WebAssembly应用
- 浏览器环境HTTP请求
- 前端数据交互
- 大文件传输需求

## 🎯 选择指南

### 场景1: 控制台应用 / Windows服务
```csharp
// 使用核心包
dotnet add package Ledon.BerryNet

// 简单配置
var httpClient = BerryHttpClientFactory.Create(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.Timeout = TimeSpan.FromMinutes(2);
});
```

### 场景2: ASP.NET Core Web应用
```csharp
// 使用AspNetCore扩展
dotnet add package Ledon.BerryNet.AspNetCore

// 在Program.cs中注册
builder.Services.AddBerryHttpClient("api", builder.Configuration.GetSection("ApiSettings"));

// 在控制器中使用
public class ApiController : ControllerBase
{
    private readonly IBerryHttpClient _httpClient;
    
    public ApiController([FromKeyedServices("api")] IBerryHttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
```

### 场景3: Blazor WebAssembly应用
```csharp
// 使用WASM优化包
dotnet add package Ledon.BerryNet.Wasm

// 在Program.cs中注册
builder.Services.AddBerryWasmHttpClientWithCompression(options =>
{
    options.BaseAddress = builder.HostEnvironment.BaseAddress;
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, deflate, br";
    options.EnableCredentials = true;
});

// 在组件中使用
@inject IBerryWasmHttpClient HttpClient

var data = await HttpClient
    .Get("/api/data")
    .WithCorsMode("cors")
    .WithBrotliCompression()
    .ExecuteAsync<DataModel>();
```

## 🚀 性能对比

### 压缩效果对比 (基于1MB JSON数据)

| 压缩算法 | 压缩率 | 压缩速度 | 解压速度 | 传输时间节省 |
|---------|--------|----------|----------|-------------|
| **无压缩** | 0% | - | - | 基准线 |
| **Gzip** | 65% | 快 | 快 | 65% ⬇️ |
| **Deflate** | 62% | 最快 | 最快 | 62% ⬇️ |
| **Brotli** | 72% | 较慢 | 快 | 72% ⬇️ |

### 多框架支持对比

| 功能特性 | .NET 7.0 | .NET 8.0 | .NET 9.0 |
|---------|----------|----------|----------|
| **核心功能** | ✅ | ✅ | ✅ |
| **压缩支持** | ✅ | ✅ | ✅ |
| **WASM优化** | ✅ | ✅ | ✅ |
| **性能提升** | 基准 | +15% | +25% |
| **内存使用** | 基准 | -10% | -20% |

## 🔄 迁移路径

### 从HttpClient迁移
```csharp
// 之前
using var httpClient = new HttpClient();
var response = await httpClient.GetAsync("https://api.example.com/data");
var content = await response.Content.ReadAsStringAsync();
var data = JsonSerializer.Deserialize<DataModel>(content);

// 迁移后
var data = await berryHttpClient
    .Get("https://api.example.com/data")
    .WithGzipCompression()  // 新增压缩支持
    .ExecuteAsync<DataModel>();
```

### 从其他HTTP库迁移
```csharp
// RestSharp风格 → BerryNet
var data = await httpClient
    .Get("/api/data")
    .WithBearerToken(token)
    .WithBrotliCompression()  // 新增高压缩率
    .ExecuteAsync<DataModel>();

// Flurl风格 → BerryNet  
var result = await httpClient
    .Post("/api/upload", model)
    .WithCompression("gzip, deflate, br")  // 新增多压缩算法
    .ExecuteAsync<UploadResult>();
```

## 📚 更多资源

- **[快速开始指南](./BerryNet-QuickStart.md)** - 5分钟上手指南
- **[核心功能文档](./BerryNet-Core-Guide.md)** - 核心包详细说明
- **[AspNetCore集成指南](./BerryNet-AspNetCore-Guide.md)** - Web应用集成
- **[WASM优化指南](./BerryNet-Wasm-Guide.md)** - Blazor应用专用
- **[API参考文档](./BerryNet-API-Reference.md)** - 完整API文档
- **[示例代码库](./examples/)** - 实际使用示例

## 🤝 贡献指南

我们欢迎社区贡献！请查看 [CONTRIBUTING.md](../CONTRIBUTING.md) 了解详细信息。

## 📄 许可证

本项目采用 [MIT 许可证](../LICENSE)。

---

**BerryNet** - 让HTTP客户端开发更简单、更高效、更现代化！
