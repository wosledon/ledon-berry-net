using Microsoft.AspNetCore.Http;

namespace Ledon.BerryNet.AspNetCore.Http;

/// <summary>
/// 简单的HttpContextAccessor实现
/// </summary>
public class SimpleHttpContextAccessor : IHttpContextAccessor
{
    private static readonly AsyncLocal<HttpContext?> _httpContextCurrent = new AsyncLocal<HttpContext?>();

    public HttpContext? HttpContext
    {
        get => _httpContextCurrent.Value;
        set => _httpContextCurrent.Value = value;
    }
}
