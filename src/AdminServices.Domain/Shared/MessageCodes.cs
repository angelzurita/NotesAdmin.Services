namespace AdminServices.Domain.Shared;

/// <summary>
/// Message codes for API responses
/// </summary>
public enum MessageCodes
{
    Success = 0,
    Error = 1,
    NotFound = 2,
    ValidationError = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Conflict = 6,
    BadRequest = 7
}
