using System.Security.Claims;
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

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } user ? user : null;

    public string? GetCurrentUserId()
    {
        // "uid" is written by JwtTokenService; "sub" arrives mapped to NameIdentifier by the JWT handler.
        return User?.FindFirst("uid")?.Value
            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst("sub")?.Value;
    }

    public string? GetCurrentUserEmail()
    {
        return User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;
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
