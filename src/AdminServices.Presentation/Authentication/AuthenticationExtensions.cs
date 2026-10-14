using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AdminServices.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AdminServices.Presentation.Authentication;

/// <summary>
/// Authentication configuration extensions
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
            throw new InvalidOperationException("Jwt:SecretKey is not configured (at least 32 characters).");
        var issuer = jwtSection["Issuer"] ?? "AdminServices";
        var audience = jwtSection["Audience"] ?? "AdminServicesClient";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // Every token belongs to a session: once it is revoked (sign out here or in the lab
            // account) the token stops working even if it has not expired yet.
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var sid = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sid)?.Value
                        ?? context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
                    var sessions = context.HttpContext.RequestServices.GetRequiredService<ISessionService>();
                    if (!Guid.TryParse(sid, out var sessionId)
                        || !await sessions.IsActiveAsync(sessionId, context.HttpContext.RequestAborted))
                    {
                        context.Fail("Session closed");
                    }
                }
            };
        });

        return services;
    }

    /// <summary>Session id of the authenticated request (claim "sid")</summary>
    public static Guid? GetSessionId(this System.Security.Claims.ClaimsPrincipal user)
    {
        var sid = user.FindFirst(JwtRegisteredClaimNames.Sid)?.Value
            ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
        return Guid.TryParse(sid, out var id) ? id : null;
    }
}
