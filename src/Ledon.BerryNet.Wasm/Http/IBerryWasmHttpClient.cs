using Ledon.BerryNet.Http;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Wasm.Http;

/// <summary>
/// WASM增强的HTTP客户端接口
/// </summary>
public interface IBerryWasmHttpClient : IBerryHttpClient
{
    /// <summary>
    /// 创建GET请求（WASM特定）
    /// </summary>
    new IBerryWasmRequestBuilder Get(string url);

    /// <summary>
    /// 创建POST请求（WASM特定）
    /// </summary>
    new IBerryWasmRequestBuilder Post(string url);

    /// <summary>
    /// 创建PUT请求（WASM特定）
    /// </summary>
    new IBerryWasmRequestBuilder Put(string url);

    /// <summary>
    /// 创建DELETE请求（WASM特定）
    /// </summary>
    new IBerryWasmRequestBuilder Delete(string url);

    /// <summary>
    /// 创建PATCH请求（WASM特定）
    /// </summary>
    new IBerryWasmRequestBuilder Patch(string url);

    /// <summary>
    /// 创建HEAD请求（WASM特定）
    /// </summary>
    IBerryWasmRequestBuilder Head(string url);

    /// <summary>
    /// 创建OPTIONS请求（WASM特定）
    /// </summary>
    IBerryWasmRequestBuilder Options(string url);
}
