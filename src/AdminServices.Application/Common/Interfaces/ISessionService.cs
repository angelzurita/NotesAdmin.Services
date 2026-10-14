using AdminServices.Domain.Entities;

namespace AdminServices.Application.Common.Interfaces;

/// <summary>
/// Sign-in sessions: every access token belongs to one, and a revoked session invalidates its tokens.
/// </summary>
public interface ISessionService
{
    /// <summary>Opens a session for the user and returns its access token</summary>
    Task<IssuedToken> SignInAsync(User user, string? ssoSid = null, CancellationToken cancellationToken = default);

    /// <summary>True while the session exists, is not revoked and has not expired</summary>
    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>Revokes one session</summary>
    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the sessions of a lab-account user: only the one of <paramref name="ssoSid"/>, all but
    /// <paramref name="exceptSsoSid"/>, or all of them when both are null. Returns how many were closed.
    /// </summary>
    Task<int> RevokeSsoAsync(string ssoSub, string? ssoSid, string? exceptSsoSid, CancellationToken cancellationToken = default);
}
