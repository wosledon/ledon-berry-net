# BerryNet WebAssembly (WASM) HTTP客户端

BerryNet.Wasm 专为 Blazor WebAssembly 应用设计，提供了针对WASM环境优化的HTTP客户端功能。

## � 支持的框架

- ✅ **.NET 7.0** - 长期支持版本
- ✅ **.NET 8.0** - 长期支持版本  
- ✅ **.NET 9.0** - 最新版本

## �🌟 WASM特性

- ✅ **CORS支持** - 完整的跨域资源共享支持
- ✅ **凭证管理** - 自动处理cookies和认证信息
- ✅ **缓存控制** - 灵活的缓存策略配置
- ✅ **进度回调** - 文件上传/下载进度监控
- ✅ **流式响应** - 支持大文件处理
- ✅ **压缩支持** - Gzip、Deflate、Brotli压缩算法
- ✅ **Blazor集成** - 深度集成Blazor WebAssembly
- ✅ **异步流** - 支持实时数据流
- ✅ **日志集成** - 完整的日志记录
- ✅ **多框架支持** - 同时支持.NET 7、8、9

## 📦 安装

```xml
<PackageReference Include="Ledon.BerryNet.Wasm" Version="1.0.0" />
```

## 🚀 快速开始

### 1. 在Program.cs中注册服务

```csharp
using Ledon.BerryNet.Wasm.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 基础配置
builder.Services.AddBerryWasmHttpClientForBlazor(builder.HostEnvironment.BaseAddress);

// 自定义配置（支持压缩）
builder.Services.AddBerryWasmHttpClient(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.EnableCredentials = true;
    options.CacheMode = "no-cache";
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, deflate, br";
    options.AutoDecompression = true;
    options.DefaultHeaders.Add("X-Client-Type", "Blazor-WASM");
});

// 压缩优化配置
builder.Services.AddBerryWasmHttpClientWithCompression(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, deflate, br";
    options.AutoDecompression = true;
    options.EnableCredentials = true;
});

// 或使用预配置的压缩设置
builder.Services.ConfigureWasmCompression(compressionOptions =>
{
    compressionOptions.EnableGzip = true;
    compressionOptions.EnableDeflate = true;
    compressionOptions.EnableBrotli = true;
});

await builder.Build().RunAsync();
```

### 2. 在Blazor组件中使用

```razor
@page "/weather"
@inject IBerryWasmHttpClient HttpClient
@inject ILogger<Weather> Logger

<h3>天气预报</h3>

@if (forecasts == null)
{
    <p><em>加载中...</em></p>
}
else
{
    <table class="table">
        @foreach (var forecast in forecasts)
        {
            <tr>
                <td>@forecast.Date.ToShortDateString()</td>
                <td>@forecast.TemperatureC</td>
                <td>@forecast.Summary</td>
            </tr>
        }
    </table>
}

@code {
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            forecasts = await HttpClient
                .Get("/api/weather")
                .WithCorsMode("cors")
                .WithCredentials(true)
                .WithCacheMode("default")
                .WithGzipCompression()  // 启用Gzip压缩
                .ExecuteAsync<WeatherForecast[]>();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "加载天气数据失败");
        }
    }
}
```

### 3. 类型化客户端

```csharp
// 注册类型化客户端
builder.Services.AddBerryWasmHttpClient<WeatherApiClient>(options =>
{
    options.BaseAddress = "https://api.weather.com";
    options.EnableCredentials = true;
});

// 客户端实现
public class WeatherApiClient
{
    private readonly IBerryWasmHttpClient _httpClient;

    public WeatherApiClient(IBerryWasmHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherForecast[]?> GetForecastAsync(string city)
    {
        return await _httpClient
            .Get($"/forecast/{city}")
            .WithCorsMode("cors")
            .WithCredentials(true)
            .WithCacheMode("default")
            .WithCompression("gzip, deflate, br")  // 启用多种压缩算法
            .ExecuteAsync<WeatherForecast[]>();
    }
            .ExecuteAsync<WeatherForecast[]>();
    }
}
```

## 🔧 WASM特定功能

### 1. CORS配置

```csharp
// 设置CORS模式
var data = await httpClient
    .Get("/api/data")
    .WithCorsMode("cors")           // cors, no-cors, same-origin
    .WithCredentials(true)          // 包含cookies
    .WithGzipCompression()          // 启用Gzip压缩
    .ExecuteAsync<DataModel>();
```

### 2. 缓存控制

```csharp
// 配置缓存策略
var result = await httpClient
    .Get("/api/cached-data")
    .WithCacheMode("no-cache")      // default, no-cache, reload, force-cache, only-if-cached
    .WithBrotliCompression()        // 启用Brotli压缩
    .ExecuteAsync<CachedData>();
```

### 3. 压缩功能

```csharp
// 单一压缩算法
var result1 = await httpClient
    .Get("/api/data")
    .WithGzipCompression()
    .ExecuteAsync<DataModel>();

// 多种压缩算法
var result2 = await httpClient
    .Post("/api/upload", data)
    .WithCompression("gzip, deflate, br")
    .ExecuteAsync<UploadResult>();

// Brotli压缩（最优压缩率）
var result3 = await httpClient
    .Get("/api/large-data")
    .WithBrotliCompression()
    .ExecuteAsync<LargeDataModel>();

// Deflate压缩
var result4 = await httpClient
    .Get("/api/text-data")
    .WithDeflateCompression()
    .ExecuteAsync<string>();
```

### 4. 文件上传（带进度）

```csharp
// 文件上传进度回调
private async Task UploadFileAsync(IBrowserFile file)
{
    var progressPercentage = 0;

    void OnProgress(long uploaded, long total)
    {
        progressPercentage = (int)((uploaded * 100) / total);
        StateHasChanged(); // 更新UI
    }

    using var stream = file.OpenReadStream();
    using var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);

    var result = await HttpClient
        .Post("/api/upload")
        .WithProgressCallback(OnProgress)
        .WithCorsMode("cors")
        .WithCredentials(true)
        .ExecuteAsync<UploadResult>();
}
```

### 4. 实时数据流

```csharp
// 异步数据流
private async Task SubscribeToUpdatesAsync()
{
    await foreach (var update in GetRealTimeUpdatesAsync(cancellationToken))
    {
        // 处理实时更新
        notifications.Add(update);
        StateHasChanged();
    }
}

private async IAsyncEnumerable<NotificationData> GetRealTimeUpdatesAsync(
    CancellationToken cancellationToken = default)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        var notification = await HttpClient
            .Get("/api/notifications/poll")
            .WithCorsMode("cors")
            .WithCredentials(true)
            .WithCacheMode("no-cache")
            .WithGzipCompression()           // 压缩实时数据
            .WithTimeout(TimeSpan.FromSeconds(30))
            .ExecuteAsync<NotificationData>(cancellationToken);

        if (notification != null)
            yield return notification;

        await Task.Delay(1000, cancellationToken);
    }
}
```

### 5. 引用策略配置

```csharp
// 设置引用策略
var secureData = await httpClient
    .Get("/api/secure-data")
    .WithReferrerPolicy("no-referrer")  // 不发送引用信息
    .WithCredentials(true)
    .WithBrotliCompression()            // 使用高压缩率算法
    .ExecuteAsync<SecureData>();
```

## ⚙️ 配置选项

### BerryWasmHttpClientOptions

```csharp
public class BerryWasmHttpClientOptions
{
    // 基础配置
    public string? BaseAddress { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    
    // WASM特定配置
    public bool EnableCredentials { get; set; }           // 启用凭证
    public bool EnableCache { get; set; }                 // 启用缓存
    public string CacheMode { get; set; }                 // 缓存模式
    public string RedirectMode { get; set; }              // 重定向模式
    public string ReferrerPolicy { get; set; }            // 引用策略
    
    // 压缩配置
    public bool EnableCompression { get; set; }           // 启用压缩
    public string CompressionEncodings { get; set; }      // 压缩算法
    public bool AutoDecompression { get; set; }           // 自动解压缩
}
```

### 常用配置模式

```csharp
// API客户端配置（高压缩）
builder.Services.AddBerryWasmHttpClient("api", options =>
{
    options.BaseAddress = "https://api.example.com";
    options.EnableCredentials = true;
    options.CacheMode = "no-cache";
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, deflate, br";
    options.AutoDecompression = true;
    options.DefaultHeaders.Add("Accept", "application/json");
    options.DefaultHeaders.Add("X-Client-Version", "1.0.0");
});

// 文件服务配置（支持大文件压缩）
builder.Services.AddBerryWasmHttpClient("files", options =>
{
    options.BaseAddress = "https://files.example.com";
    options.EnableCredentials = false;
    options.CacheMode = "default";
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip, br";  // 文件传输优先使用高压缩率
    options.Timeout = TimeSpan.FromMinutes(10);
});

// 实时服务配置（轻量压缩）
builder.Services.AddBerryWasmHttpClient("realtime", options =>
{
    options.BaseAddress = "https://realtime.example.com";
    options.EnableCredentials = true;
    options.CacheMode = "no-store";
    options.EnableCompression = true;
    options.CompressionEncodings = "gzip";      // 实时数据优先速度
    options.Timeout = TimeSpan.FromMinutes(5);
});
```

## 🎯 最佳实践

### 1. 错误处理

```csharp
try
{
    var data = await httpClient
        .Get("/api/data")
        .WithGzipCompression()          // 启用压缩以减少传输时间
        .WithTimeout(TimeSpan.FromSeconds(30))
        .ExecuteAsync<DataModel>();
        .WithCorsMode("cors")
        .ExecuteAsync<DataModel>();
}
catch (HttpRequestException ex) when (ex.Message.Contains("CORS"))
{
    // 处理CORS错误
    Logger.LogError("CORS错误: {Error}", ex.Message);
}
catch (TaskCanceledException)
{
    // 处理超时
    Logger.LogWarning("请求超时");
}
catch (Exception ex)
{
    // 处理其他错误
    Logger.LogError(ex, "请求失败");
}
```

### 2. 性能优化

```csharp
// 使用适当的缓存策略和压缩
var cachedData = await httpClient
    .Get("/api/static-data")
    .WithCacheMode("force-cache")  // 强制使用缓存
    .WithBrotliCompression()       // 静态数据使用高压缩率
    .ExecuteAsync<StaticData>();

// 减少不必要的请求头，使用轻量压缩
var lightData = await httpClient
    .Get("/api/light-data")
    .WithCorsMode("same-origin")   // 同源请求，减少预检
    .WithGzipCompression()         // 轻量数据使用快速压缩
    .ExecuteAsync<LightData>();

// 大文件传输优化
var largeFile = await httpClient
    .Get("/api/downloads/large-file")
    .WithBrotliCompression()       // 大文件使用最高压缩率
    .WithCacheMode("default")      // 允许缓存大文件
    .ExecuteAsync<byte[]>();

// 频繁请求的小数据优化
var frequentData = await httpClient
    .Get("/api/frequent-updates")
    .WithGzipCompression()         // 快速压缩，减少延迟
    .WithCacheMode("no-cache")     // 确保数据最新
    .ExecuteAsync<FrequentData>();
```

### 3. 安全考虑

```csharp
// 敏感数据请求
var sensitiveData = await httpClient
    .Post("/api/sensitive")
    .WithJsonBody(request)
    .WithCorsMode("cors")
    .WithCredentials(true)
    .WithReferrerPolicy("no-referrer")  // 不泄露引用信息
    .WithHeader("X-Requested-With", "XMLHttpRequest")
    .ExecuteAsync<SensitiveData>();
```

## 🔍 调试和监控

### 日志记录

```csharp
// 启用详细日志
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// 日志会自动记录请求详情
// Debug: 创建WASM HTTP客户端: Default
// Debug: 设置CORS模式: cors
// Information: 执行WASM HTTP请求
// Information: WASM HTTP请求完成，状态码: 200
```

### 性能监控

```csharp
// 自定义进度监控
private readonly Dictionary<string, (DateTime Start, long Total)> _requestMetrics = new();

void OnProgress(long uploaded, long total)
{
    var requestId = Guid.NewGuid().ToString();
    if (!_requestMetrics.ContainsKey(requestId))
    {
        _requestMetrics[requestId] = (DateTime.Now, total);
    }
    
    var (start, totalSize) = _requestMetrics[requestId];
    var elapsed = DateTime.Now - start;
    var speed = uploaded / elapsed.TotalSeconds;
    
    Logger.LogDebug("传输速度: {Speed:F2} bytes/sec", speed);
}
```

## 📋 兼容性

- **Blazor WebAssembly**: ✅ 完全支持
- **Blazor Server**: ⚠️ 部分功能受限
- **浏览器支持**: 现代浏览器 (Chrome 60+, Firefox 60+, Safari 12+)
- **.NET版本**: .NET 9.0+

## 🔄 迁移指南

### 从标准BerryNet迁移

```csharp
// 之前
services.AddBerryHttpClient();

// 现在
services.AddBerryWasmHttpClient();

// 代码几乎无需修改
var data = await httpClient
    .Get("/api/data")
    .WithBearerToken(token)
    .ExecuteAsync<DataModel>();
```

### 新增WASM特性

```csharp
// 添加WASM特性到现有代码
var data = await httpClient
    .Get("/api/data")
    .WithBearerToken(token)
    .WithCorsMode("cors")          // 新增: CORS控制
    .WithCredentials(true)         // 新增: 凭证管理
    .WithGzipCompression()         // 新增: 压缩支持
    .WithCacheMode("no-cache")     // 新增: 缓存控制
    .ExecuteAsync<DataModel>();

// 高级压缩配置
var compressedData = await httpClient
    .Post("/api/upload", largeData)
    .WithBrotliCompression()       // 新增: 高压缩率算法
    .WithProgressCallback(OnProgress) // 新增: 进度回调
    .ExecuteAsync<UploadResult>();
```

## 🔧 压缩算法选择指南

### Gzip压缩 - 平衡型
```csharp
// 适用场景：通用API调用、中等大小数据
.WithGzipCompression()
```
- **压缩率**: 中等 (60-70%)
- **速度**: 快
- **兼容性**: 最广泛
- **最佳用途**: 日常API调用、JSON数据、小到中等文件

### Deflate压缩 - 快速型
```csharp
// 适用场景：频繁请求、对延迟敏感的场景
.WithDeflateCompression()
```
- **压缩率**: 中等 (55-65%)
- **速度**: 最快
- **兼容性**: 广泛
- **最佳用途**: 实时数据、频繁更新、对速度要求高的场景

### Brotli压缩 - 高效型
```csharp
// 适用场景：大文件传输、静态资源、对带宽敏感的场景
.WithBrotliCompression()
```
- **压缩率**: 最高 (70-80%)
- **速度**: 较慢
- **兼容性**: 现代浏览器
- **最佳用途**: 大文件下载、静态资源、带宽受限环境

### 组合压缩策略
```csharp
// 让服务器选择最佳压缩算法
.WithCompression("br, gzip, deflate")  // 优先级排序
```

## 📋 兼容性

- **Blazor WebAssembly**: ✅ 完全支持
- **Blazor Server**: ⚠️ 部分功能受限
- **浏览器支持**: 现代浏览器 (Chrome 60+, Firefox 60+, Safari 12+)
- **.NET版本**: 
  - ✅ **.NET 7.0** - 完全支持
  - ✅ **.NET 8.0** - 完全支持  
  - ✅ **.NET 9.0** - 完全支持

## 🔄 迁移指南

### 从标准BerryNet迁移

```csharp
// 之前
services.AddBerryHttpClient();

// 现在
services.AddBerryWasmHttpClient();

// 代码几乎无需修改
var data = await httpClient
    .Get("/api/data")
    .WithBearerToken(token)
    .ExecuteAsync<DataModel>();
```
    .WithBearerToken(token)
    .WithCorsMode("cors")        // 新增
    .WithCredentials(true)       // 新增
    .WithCacheMode("no-cache")   // 新增
    .ExecuteAsync<DataModel>();
```

BerryNet.Wasm 为Blazor WebAssembly应用提供了完整的HTTP客户端解决方案，特别针对浏览器环境的限制和特性进行了优化。
