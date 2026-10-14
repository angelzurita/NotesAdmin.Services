using AdminServices.Application.Common.Interfaces;
using AdminServices.Domain.Entities;
using AdminServices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AdminServices.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public SessionService(ApplicationDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IssuedToken> SignInAsync(User user, string? ssoSid = null, CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTime.UtcNow.Add(_jwtTokenService.Lifetime);
        var session = new UserSession(user.Id, ssoSid, expiresAt);
        _context.UserSessions.Add(session);

        // Housekeeping: forget this user's sessions that ended more than a week ago
        var limit = DateTime.UtcNow.AddDays(-7);
        await _context.UserSessions
            .Where(s => s.UserId == user.Id && s.ExpiresAt < limit)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return _jwtTokenService.GenerateToken(user, session.Id, expiresAt);
    }

    public async Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.UserSessions.AnyAsync(
            s => s.Id == sessionId && s.RevokedAt == null && s.ExpiresAt > now
                 && s.User!.IsActive, cancellationToken);
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        if (session is null) return;
        session.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RevokeSsoAsync(string ssoSub, string? ssoSid, string? exceptSsoSid, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = _context.UserSessions.Where(s => s.User!.SsoSub == ssoSub && s.RevokedAt == null && s.ExpiresAt > now);
        if (!string.IsNullOrEmpty(ssoSid))
            query = query.Where(s => s.SsoSid == ssoSid);
        else if (!string.IsNullOrEmpty(exceptSsoSid))
            query = query.Where(s => s.SsoSid == null || s.SsoSid != exceptSsoSid);

        var sessions = await query.ToListAsync(cancellationToken);
        foreach (var session in sessions)
            session.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
        return sessions.Count;
    }
}
