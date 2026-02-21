using Microsoft.Extensions.DependencyInjection;

namespace AdminServices.Presentation.Authorization;

/// <summary>
/// Authorization configuration extensions
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
            .AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "Admin"));

        return services;
    }
}
