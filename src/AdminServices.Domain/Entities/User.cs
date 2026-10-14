using AdminServices.Domain.Primitives;

namespace AdminServices.Domain.Entities;

/// <summary>
/// User entity for authentication and authorization
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User";
    public bool IsActive { get; private set; }

    /// <summary>Id of the user in the lab account (Cuenta Lab); null for local users</summary>
    public string? SsoSub { get; private set; }

    private User() { }

    public User(string email, string passwordHash, string fullName, string role = "User")
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        IsActive = true;
    }

    /// <summary>Creates a user that signs in with the lab account (no local password)</summary>
    public static User FromSso(string ssoSub, string email, string fullName) =>
        new(email, string.Empty, fullName) { SsoSub = ssoSub };

    public void UpdateFullName(string fullName)
    {
        if (FullName == fullName) return;
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string fullName, string role)
    {
        FullName = fullName;
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
