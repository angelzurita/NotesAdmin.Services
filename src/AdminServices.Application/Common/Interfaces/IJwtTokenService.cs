using AdminServices.Domain.Entities;

namespace AdminServices.Application.Common.Interfaces;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(User user);
}
