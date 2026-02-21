using System.Net;
// Azure packages commented for local development
// using Azure.Extensions.AspNetCore.Configuration.Secrets;
// using Azure.Identity;
// using Azure.Messaging.ServiceBus;
// using Azure.Security.KeyVault.Secrets;
// using Azure.Storage.Blobs;
using AdminServices.Application.Common.Interfaces;
using AdminServices.Domain.Repositories;
using AdminServices.Infrastructure.Common;
using AdminServices.Infrastructure.Options;
using AdminServices.Infrastructure.Persistence;
using AdminServices.Infrastructure.Persistence.Interceptors;
using AdminServices.Infrastructure.Persistence.Repositories;
using AdminServices.Infrastructure.Services;
// using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
// using Microsoft.Extensions.Azure;
// using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
// using StackExchange.Redis;
using Quartz;

namespace AdminServices.Infrastructure;

/// <summary>
/// Dependency injection for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var jitterer = new Random();
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt) / 2) +
                    TimeSpan.FromMilliseconds(jitterer.Next(100, 500))
            );
    }

    private static Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }

    private static Polly.Timeout.AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IHttpContextService, HttpContextService>();

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (connectionString != null)
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                });
                options.EnableDetailedErrors(true);
                options.EnableSensitiveDataLogging(false);
            }
        });

        // Azure Blob Storage - commented for local development
        // services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));
        // services.AddAzureClients(azureBuilder =>
        // {
        //     var blobOptions = configuration.GetSection(BlobStorageOptions.SectionName).Get<BlobStorageOptions>();
        //     if (blobOptions != null && !string.IsNullOrEmpty(blobOptions.BaseUrl))
        //     {
        //         azureBuilder.AddBlobServiceClient(new Uri(blobOptions.BaseUrl))
        //                    .WithName("BlobStorage")
        //                    .ConfigureOptions(options => options.Retry.MaxRetries = 3);
        //     }
        // });
        // services.AddScoped<IBlobStorageService>(serviceProvider =>
        // {
        //     var blobOptions = serviceProvider.GetRequiredService<IOptions<BlobStorageOptions>>();
        //     var credential = new DefaultAzureCredential();
        //     var serviceClient = new BlobServiceClient(new Uri(blobOptions.Value.BaseUrl), credential);
        //     return new BlobStorageService(serviceClient, blobOptions);
        // });

        // JWT and Auth services
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Add repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Quartz for background jobs
        services.AddQuartz();

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }

    // Redis - commented for local development
    // public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    // {
    //     var redisConnection = configuration.GetConnectionString("Redis");
    //     if (!string.IsNullOrEmpty(redisConnection))
    //     {
    //         services.AddStackExchangeRedisCache(options =>
    //         {
    //             options.Configuration = redisConnection;
    //             options.InstanceName = "AdminServices_";
    //         });
    //         services.AddScoped<ICacheService, RedisCacheService>();
    //     }
    //     return services;
    // }

    // Service Bus - commented for local development
    // public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    // {
    //     var serviceBusConnection = configuration.GetConnectionString("ServiceBus");
    //     if (!string.IsNullOrEmpty(serviceBusConnection))
    //     {
    //         services.AddSingleton(serviceProvider => new ServiceBusClient(serviceBusConnection));
    //     }
    //     return services;
    // }

    // Application Insights - commented for local development
    // public static IServiceCollection AddApplicationInsights(this IServiceCollection services, ILoggingBuilder logging, IConfiguration configuration)
    // {
    //     var instrumentationKey = configuration["ApplicationInsights:InstrumentationKey"];
    //     if (!string.IsNullOrEmpty(instrumentationKey))
    //     {
    //         services.AddApplicationInsightsTelemetry(options =>
    //         {
    //             options.ConnectionString = $"InstrumentationKey={instrumentationKey}";
    //         });
    //         logging.AddApplicationInsights(
    //             configureTelemetryConfiguration: (config) =>
    //                 config.ConnectionString = $"InstrumentationKey={instrumentationKey}",
    //             configureApplicationInsightsLoggerOptions: (options) => { }
    //         );
    //         logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
    //     }
    //     return services;
    // }
}
