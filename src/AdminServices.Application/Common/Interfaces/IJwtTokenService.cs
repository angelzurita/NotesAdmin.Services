using AdminServices.Domain.Entities;

namespace AdminServices.Application.Common.Interfaces;

/// <summary>Issued access token and when it expires</summary>
public sealed record IssuedToken(string Token, DateTime ExpiresAt);

/// <summary>
/// JWT Token Service interface
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Lifetime of an access token</summary>
    TimeSpan Lifetime { get; }

    /// <summary>Generates a token bound to a session (claim "sid"), which can be revoked</summary>
    IssuedToken GenerateToken(User user, Guid sessionId, DateTime expiresAt);
}
