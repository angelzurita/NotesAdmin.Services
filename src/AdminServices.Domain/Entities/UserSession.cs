using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// A sign-in of a user on one device. Every JWT carries the session id, so closing the session
/// (from the lab account or by signing out) invalidates its tokens before they expire.
/// </summary>
public class UserSession : Entity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    /// <summary>Session id of the lab account (one per device), used to close it from there</summary>
    public string? SsoSid { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private UserSession() { }

    public UserSession(Guid userId, string? ssoSid, DateTime expiresAt)
    {
        UserId = userId;
        SsoSid = ssoSid;
        ExpiresAt = expiresAt;
    }

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke()
    {
        RevokedAt ??= DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
