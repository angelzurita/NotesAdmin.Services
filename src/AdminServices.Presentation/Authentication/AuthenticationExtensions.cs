using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
// using Microsoft.Identity.Web; // Commented for local development

namespace AdminServices.Presentation.Authentication;

/// <summary>
/// Authentication configuration extensions
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Azure AD - commented for local development
        // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
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
        });

        return services;
    }
}
