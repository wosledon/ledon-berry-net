using Ledon.BerryNet.Http;

namespace Ledon.BerryNet.Wasm.Http;

/// <summary>
/// WASM HTTP请求构建器接口
/// </summary>
public interface IBerryWasmRequestBuilder : IBerryHttpRequestBuilder
{
    /// <summary>
    /// 设置CORS模式
    /// </summary>
    IBerryWasmRequestBuilder WithCorsMode(string corsMode);

    /// <summary>
    /// 设置是否包含凭据
    /// </summary>
    IBerryWasmRequestBuilder WithCredentials(bool includeCredentials);

    /// <summary>
    /// 设置缓存模式
    /// </summary>
    IBerryWasmRequestBuilder WithCacheMode(string cacheMode);

    /// <summary>
    /// 设置进度回调
    /// </summary>
    IBerryWasmRequestBuilder WithProgressCallback(Action<long, long> progressCallback);

    /// <summary>
    /// 设置引用策略
    /// </summary>
    IBerryWasmRequestBuilder WithReferrerPolicy(string policy);

    /// <summary>
    /// 启用流式响应
    /// </summary>
    IBerryWasmRequestBuilder WithStreamingResponse();

    /// <summary>
    /// 设置压缩方式
    /// </summary>
    IBerryWasmRequestBuilder WithCompression(params string[] encodings);

    /// <summary>
    /// 启用Gzip压缩
    /// </summary>
    new IBerryWasmRequestBuilder WithGzipCompression();

    /// <summary>
    /// 启用Brotli压缩
    /// </summary>
    new IBerryWasmRequestBuilder WithBrotliCompression();

    /// <summary>
    /// 启用Deflate压缩
    /// </summary>
    new IBerryWasmRequestBuilder WithDeflateCompression();

    /// <summary>
    /// 设置JSON请求体（WASM特定的返回类型）
    /// </summary>
    new IBerryWasmRequestBuilder WithJsonBody<T>(T data);

    /// <summary>
    /// 设置表单数据（WASM特定的返回类型）
    /// </summary>
    new IBerryWasmRequestBuilder WithFormBody(Dictionary<string, string> formData);

    /// <summary>
    /// 设置超时时间（WASM特定的返回类型）  
    /// </summary>
    new IBerryWasmRequestBuilder WithTimeout(TimeSpan timeout);
}
