# BerryNet HTTP客户端

BerryNet 提供了一个强大且易用的HTTP客户端封装，支持链式调用、工厂模式和AspNetCore集成。

## 特性

- ✅ 支持链式调用的流畅API
- ✅ 工厂模式支持，便于依赖注入
- ✅ AspNetCore深度集成
- ✅ 自动JSON序列化/反序列化
- ✅ 请求头传播（Authentication、Correlation ID等）
- ✅ 类型化客户端支持
- ✅ 重试和超时机制
- ✅ 完整的异步支持

## 基础用法

### 1. 基础HTTP客户端使用

```csharp
using Ledon.BerryNet.Http;

// 创建客户端
var client = new BerryHttpClient();

// GET请求
var response = await client
    .Get("https://api.example.com/users")
    .WithBearerToken("your-token")
    .WithQueryParameter("page", "1")
    .ExecuteAsync();

// POST请求
var user = new { Name = "John", Email = "john@example.com" };
var result = await client
    .Post("https://api.example.com/users")
    .WithJsonBody(user)
    .WithHeader("Content-Type", "application/json")
    .ExecuteAsync<User>();
```

### 2. 工厂模式使用

```csharp
using Ledon.BerryNet.Http;

// 创建工厂
var factory = new BerryHttpClientFactory();

// 创建客户端
var client = factory.CreateClient();

// 使用命名客户端
var apiClient = factory.CreateClient("api");
```

## AspNetCore集成

### 1. 服务注册

```csharp
using Ledon.BerryNet.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 添加基础HTTP客户端
builder.Services.AddBerryHttpClient(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.Timeout = TimeSpan.FromSeconds(30);
    options.DefaultHeaders.Add("User-Agent", "MyApp/1.0");
});

// 添加命名客户端
builder.Services.AddBerryHttpClient("weather", options =>
{
    options.BaseAddress = "https://api.openweathermap.org/data/2.5";
    options.Timeout = TimeSpan.FromSeconds(10);
});

// 添加类型化客户端
builder.Services.AddBerryHttpClient<WeatherApiClient>(options =>
{
    options.BaseAddress = "https://api.openweathermap.org/data/2.5";
});

var app = builder.Build();
```

### 2. 在控制器中使用

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
        // 自动传播当前请求的认证信息和相关ID
        var users = await _httpClient
            .Get("/users")
            .PropagateAuthentication(HttpContext)
            .PropagateCorrelationId(HttpContext)
            .ExecuteAsync<List<User>>();

        return Ok(users);
    }
}
```

### 3. 类型化客户端

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

// 在控制器中使用
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly WeatherApiClient _weatherClient;

    public WeatherController(WeatherApiClient weatherClient)
    {
        _weatherClient = weatherClient;
    }

    [HttpGet("weather/{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var weather = await _weatherClient.GetWeatherAsync(city);
        return Ok(weather);
    }
}
```

## 高级特性

### 1. 请求头传播

```csharp
// 传播认证信息
await client
    .Get("/api/data")
    .PropagateAuthentication(HttpContext)
    .ExecuteAsync();

// 传播特定请求头
await client
    .Get("/api/data")
    .PropagateHeaders(HttpContext, "X-Custom-Header", "X-Client-Version")
    .ExecuteAsync();

// 传播相关ID
await client
    .Get("/api/data")
    .PropagateCorrelationId(HttpContext)
    .ExecuteAsync();
```

### 2. 错误处理

```csharp
try
{
    var result = await client
        .Get("/api/data")
        .ExecuteAsync<ApiResponse>();
}
catch (HttpRequestException ex)
{
    // HTTP请求异常
    Console.WriteLine($"请求失败: {ex.Message}");
}
catch (TimeoutException ex)
{
    // 超时异常
    Console.WriteLine($"请求超时: {ex.Message}");
}
```

### 3. 自定义JSON选项

```csharp
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

builder.Services.AddBerryHttpClient(options =>
{
    options.JsonSerializerOptions = jsonOptions;
});
```

## API参考

### IBerryHttpClient 接口

- `CreateRequest()` - 创建新的请求构建器
- `Get(string url)` - 创建GET请求
- `Post(string url)` - 创建POST请求
- `Put(string url)` - 创建PUT请求
- `Delete(string url)` - 创建DELETE请求
- `Patch(string url)` - 创建PATCH请求

### IBerryHttpRequestBuilder 接口

- `WithUrl(string url)` - 设置请求URL
- `WithMethod(HttpMethod method)` - 设置请求方法
- `WithHeader(string name, string value)` - 添加请求头
- `WithBearerToken(string token)` - 添加Bearer Token
- `WithJsonBody<T>(T data)` - 设置JSON请求体
- `WithQueryParameter(string name, string value)` - 添加查询参数
- `WithTimeout(TimeSpan timeout)` - 设置超时时间
- `ExecuteAsync()` - 执行请求
- `ExecuteAsync<T>()` - 执行请求并返回指定类型

### IBerryHttpClientEnhanced 接口 (AspNetCore)

继承自 `IBerryHttpClient`，额外提供：

- `PropagateHeaders(HttpContext, params string[])` - 传播指定请求头
- `PropagateAuthentication(HttpContext)` - 传播认证信息
- `PropagateCorrelationId(HttpContext)` - 传播相关ID

## 配置选项

### BerryHttpClientOptions

```csharp
public class BerryHttpClientOptions
{
    public string? BaseAddress { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; }
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }
    public bool IgnoreSslErrors { get; set; }
    public int MaxRetryCount { get; set; }
    public TimeSpan RetryDelay { get; set; }
}
```

## 最佳实践

1. **使用工厂模式**: 在AspNetCore应用中，优先使用依赖注入的方式获取客户端实例
2. **合理设置超时**: 根据API的响应时间合理设置超时时间
3. **传播上下文**: 在微服务环境中，确保传播认证信息和相关ID
4. **错误处理**: 总是处理可能的HTTP异常和超时异常
5. **类型化客户端**: 对于复杂的API调用，使用类型化客户端提高代码的可维护性

## 示例项目

查看 `Examples` 文件夹中的示例代码，了解更多使用方法和最佳实践。
