using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdminServices.Application;

/// <summary>
/// Dependency Injection for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add AutoMapper
        services.AddAutoMapper(assembly);

        return services;
    }
}
