using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Ledon.BerryNet.Http;

/// <summary>
/// HTTP请求构建器接口
/// </summary>
public interface IBerryHttpRequestBuilder
{
    /// <summary>
    /// 设置请求URL
    /// </summary>
    IBerryHttpRequestBuilder WithUrl(string url);
    
    /// <summary>
    /// 设置请求方法
    /// </summary>
    IBerryHttpRequestBuilder WithMethod(HttpMethod method);
    
    /// <summary>
    /// 添加请求头
    /// </summary>
    IBerryHttpRequestBuilder WithHeader(string name, string value);
    
    /// <summary>
    /// 添加认证头
    /// </summary>
    IBerryHttpRequestBuilder WithAuthentication(string scheme, string token);
    
    /// <summary>
    /// 添加Bearer Token
    /// </summary>
    IBerryHttpRequestBuilder WithBearerToken(string token);
    
    /// <summary>
    /// 设置请求体（JSON）
    /// </summary>
    IBerryHttpRequestBuilder WithJsonBody<T>(T data);
    
    /// <summary>
    /// 设置请求体（字符串）
    /// </summary>
    IBerryHttpRequestBuilder WithStringBody(string content, string mediaType = "text/plain");
    
    /// <summary>
    /// 设置请求体（表单数据）
    /// </summary>
    IBerryHttpRequestBuilder WithFormBody(Dictionary<string, string> formData);
    
    /// <summary>
    /// 添加查询参数
    /// </summary>
    IBerryHttpRequestBuilder WithQueryParameter(string name, string value);
    
    /// <summary>
    /// 设置超时时间
    /// </summary>
    IBerryHttpRequestBuilder WithTimeout(TimeSpan timeout);
    
    /// <summary>
    /// 启用Gzip压缩
    /// </summary>
    IBerryHttpRequestBuilder WithGzipCompression();
    
    /// <summary>
    /// 启用Deflate压缩
    /// </summary>
    IBerryHttpRequestBuilder WithDeflateCompression();
    
    /// <summary>
    /// 启用Brotli压缩
    /// </summary>
    IBerryHttpRequestBuilder WithBrotliCompression();
    
    /// <summary>
    /// 设置自定义压缩编码
    /// </summary>
    IBerryHttpRequestBuilder WithCompression(string encodings);
    
    /// <summary>
    /// 执行请求并返回响应
    /// </summary>
    Task<IBerryHttpResponse> ExecuteAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 执行请求并返回指定类型的结果
    /// </summary>
    Task<T> ExecuteAsync<T>(CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP响应接口
/// </summary>
public interface IBerryHttpResponse
{
    /// <summary>
    /// 状态码
    /// </summary>
    int StatusCode { get; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    bool IsSuccessful { get; }
    
    /// <summary>
    /// 响应头
    /// </summary>
    HttpResponseHeaders Headers { get; }
    
    /// <summary>
    /// 响应内容
    /// </summary>
    string Content { get; }
    
    /// <summary>
    /// 获取响应内容为指定类型
    /// </summary>
    T GetContent<T>();
    
    /// <summary>
    /// 获取响应内容为字节数组
    /// </summary>
    byte[] GetBytes();
}

/// <summary>
/// HTTP客户端接口
/// </summary>
public interface IBerryHttpClient
{
    /// <summary>
    /// 创建新的请求构建器
    /// </summary>
    IBerryHttpRequestBuilder CreateRequest();
    
    /// <summary>
    /// GET请求
    /// </summary>
    IBerryHttpRequestBuilder Get(string url);
    
    /// <summary>
    /// POST请求
    /// </summary>
    IBerryHttpRequestBuilder Post(string url);
    
    /// <summary>
    /// PUT请求
    /// </summary>
    IBerryHttpRequestBuilder Put(string url);
    
    /// <summary>
    /// DELETE请求
    /// </summary>
    IBerryHttpRequestBuilder Delete(string url);
    
    /// <summary>
    /// PATCH请求
    /// </summary>
    IBerryHttpRequestBuilder Patch(string url);
}
