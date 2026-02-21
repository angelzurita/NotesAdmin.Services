using AdminServices.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AdminServices.Infrastructure.Common;

/// <summary>
/// HTTP Context Service implementation
/// </summary>
public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value;
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    }

    public string? GetClientCode()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["X-Client-Code"].FirstOrDefault();
    }

    public Dictionary<string, string> GetAllHeaders()
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;
        if (headers == null) return new Dictionary<string, string>();

        return headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    }
}
