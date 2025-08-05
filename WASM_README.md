# Ledon.BerryNet.Wasm - WebAssembly HTTP客户端扩展

## 概述

`Ledon.BerryNet.Wasm` 是对 `Ledon.BerryNet` 核心HTTP客户端库的WebAssembly特定扩展，专门为Blazor WebAssembly应用程序设计。它提供了浏览器特定的HTTP功能，如CORS控制、缓存管理、进度回调和凭据处理。

## 主要特性

### 🌐 浏览器特定功能
- **CORS模式控制**: 支持 `cors`、`no-cors`、`same-origin` 等模式
- **缓存策略**: 支持 `default`、`no-cache`、`reload`、`force-cache`、`only-if-cached` 等
- **凭据处理**: 控制是否在跨域请求中包含cookies和认证信息
- **引用策略**: 设置HTTP referrer政策
- **流式响应**: 支持大文件的流式下载

### ⚡ 性能增强
- **进度回调**: 实时监控上传/下载进度
- **异步流处理**: 支持大文件的内存友好处理
- **超时控制**: 精确的请求超时管理

### 🔧 开发者友好
- **链式API**: 继承核心库的流畅接口设计
- **类型安全**: 完整的泛型支持和强类型返回
- **日志集成**: 内置的日志记录和调试支持

## 核心接口

### IBerryWasmHttpClient
扩展了基础的 `IBerryHttpClient`，添加了WASM特定的HTTP方法：

```csharp
public interface IBerryWasmHttpClient : IBerryHttpClient
{
    new IBerryWasmRequestBuilder Get(string url);
    new IBerryWasmRequestBuilder Post(string url);
    new IBerryWasmRequestBuilder Put(string url);
    new IBerryWasmRequestBuilder Delete(string url);
    new IBerryWasmRequestBuilder Patch(string url);
    IBerryWasmRequestBuilder Head(string url);
    IBerryWasmRequestBuilder Options(string url);
}
```

### IBerryWasmRequestBuilder
扩展了基础的 `IBerryHttpRequestBuilder`，添加了浏览器特定功能：

```csharp
public interface IBerryWasmRequestBuilder : IBerryHttpRequestBuilder
{
    IBerryWasmRequestBuilder WithCorsMode(string corsMode);
    IBerryWasmRequestBuilder WithCredentials(bool includeCredentials);
    IBerryWasmRequestBuilder WithCacheMode(string cacheMode);
    IBerryWasmRequestBuilder WithProgressCallback(Action<long, long> progressCallback);
    IBerryWasmRequestBuilder WithReferrerPolicy(string policy);
    IBerryWasmRequestBuilder WithStreamingResponse();
    // ... 其他WASM特定方法
}
```

## 使用示例

### 基本GET请求（支持CORS）
```csharp
var users = await httpClient
    .Get("/api/users")
    .WithCorsMode("cors")
    .WithCredentials(true)
    .WithCacheMode("no-cache")
    .ExecuteAsync<List<User>>();
```

### 文件上传（带进度）
```csharp
var result = await httpClient
    .Post("/api/upload")
    .WithFormBody(formData)
    .WithCorsMode("cors")
    .WithCredentials(true)
    .WithProgressCallback((uploaded, total) => 
    {
        var percentage = (int)((uploaded * 100) / total);
        UpdateProgressBar(percentage);
    })
    .ExecuteAsync<UploadResult>();
```

### 流式下载
```csharp
var fileData = await httpClient
    .Get($"/api/files/{fileId}/download")
    .WithStreamingResponse()
    .WithCacheMode("no-cache")
    .WithProgressCallback(OnDownloadProgress)
    .ExecuteAsync();

var bytes = fileData.GetBytes();
```

### 长连接请求
```csharp
var realTimeData = await httpClient
    .Get("/api/realtime")
    .WithCredentials(true)
    .WithCacheMode("no-store")
    .WithReferrerPolicy("no-referrer")
    .WithTimeout(TimeSpan.FromMinutes(5))
    .ExecuteAsync<RealTimeData>();
```

## 架构设计

### 装饰器模式
`BerryWasmHttpClient` 使用装饰器模式包装核心的 `BerryHttpClient`，添加WASM特定功能而不修改原有代码。

### 构建器模式增强
`BerryWasmRequestBuilder` 扩展了核心的请求构建器，添加了浏览器特定的配置选项。

### 工厂模式
提供了 `BerryWasmHttpClientFactory` 来创建和管理WASM HTTP客户端实例。

## 依赖关系

- **Ledon.BerryNet**: 核心HTTP客户端功能
- **Microsoft.AspNetCore.Components.Web**: Blazor WebAssembly支持
- **Microsoft.Extensions.Logging**: 日志记录
- **System.Text.Json**: JSON序列化

## 目标框架

- .NET 7.0
- .NET 8.0  
- .NET 9.0

## 示例项目结构

```
src/Ledon.BerryNet.Wasm/
├── Http/
│   ├── IBerryWasmHttpClient.cs          # WASM HTTP客户端接口
│   ├── BerryWasmHttpClient.cs           # WASM HTTP客户端实现
│   ├── IBerryWasmRequestBuilder.cs      # WASM请求构建器接口
│   ├── BerryWasmRequestBuilder.cs       # WASM请求构建器实现
│   └── BerryWasmHttpClientFactory.cs    # WASM客户端工厂
├── Examples/
│   ├── WasmApiClient.cs                 # API客户端示例
│   └── BlazorUsageExamples.cs          # Blazor组件示例
└── Ledon.BerryNet.Wasm.csproj          # 项目文件
```

## 最佳实践

### 1. CORS配置
在Blazor WASM应用中，合理配置CORS模式：
- 使用 `cors` 模式进行跨域API调用
- 在需要凭据时设置 `WithCredentials(true)`

### 2. 缓存策略
根据数据特性选择合适的缓存模式：
- 静态资源使用 `default` 或 `force-cache`
- 动态数据使用 `no-cache` 或 `reload`

### 3. 进度监控
对于大文件操作，始终提供进度反馈：
```csharp
.WithProgressCallback((uploaded, total) => 
{
    var progress = (int)((uploaded * 100) / total);
    StateHasChanged(); // 通知Blazor组件更新UI
})
```

### 4. 错误处理
在Blazor组件中正确处理异步错误：
```csharp
try 
{
    var result = await wasmHttpClient.Get("/api/data").ExecuteAsync<Data>();
    // 处理成功响应
}
catch (HttpRequestException ex)
{
    // 处理网络错误
    logger.LogError(ex, "网络请求失败");
}
catch (TaskCanceledException ex)
{
    // 处理超时
    logger.LogWarning("请求超时");
}
```

## 性能考虑

1. **内存管理**: 使用流式处理避免大文件导致的内存问题
2. **连接复用**: 通过HttpClientFactory管理连接池
3. **并发控制**: 合理控制并发请求数量
4. **缓存利用**: 充分利用浏览器缓存机制

## 总结

`Ledon.BerryNet.Wasm` 为Blazor WebAssembly应用提供了一个功能完整、类型安全、易于使用的HTTP客户端解决方案。它在保持核心库简洁性的同时，为浏览器环境提供了必要的增强功能，是构建现代Web应用的理想选择。
