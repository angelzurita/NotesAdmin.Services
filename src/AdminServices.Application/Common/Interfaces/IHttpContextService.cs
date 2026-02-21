namespace AdminServices.Application.Common.Interfaces;

/// <summary>
/// HTTP Context Service interface
/// </summary>
public interface IHttpContextService
{
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    string? GetClientCode();
    Dictionary<string, string> GetAllHeaders();
}
