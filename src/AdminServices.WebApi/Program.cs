using AdminServices.Application;
using AdminServices.Infrastructure;
using AdminServices.Infrastructure.Persistence;
using AdminServices.Presentation.Authentication;
using AdminServices.Presentation.Authorization;
using AdminServices.Presentation.Filters;
using AdminServices.Presentation.Middlewares;
using AdminServices.Presentation.Modules;
using AdminServices.WebApi;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Secrets live outside the repo and the package (JWT key, lab account).
// In the lab: /opt/storage/tenants/alicelabs/sites/notes/secrets/notes.json (override with NOTES_SECRETS).
builder.Configuration.AddJsonFile(Environment.GetEnvironmentVariable("NOTES_SECRETS")
    ?? "/opt/storage/tenants/alicelabs/sites/notes/secrets/notes.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// Uploads are stored in the database: cap them (10 MB per file, a bit more for the base64 JSON variant)
const long MaxUploadBytes = 10 * 1024 * 1024;
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = MaxUploadBytes);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = MaxUploadBytes * 14 / 10);

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// The web is served by this same app; CORS is only needed for the Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevFrontend", policy => policy
        .WithOrigins("http://localhost:4200", "https://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Behind the Cloudflare tunnel / reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

// Security
builder.Services.AddAuthorization();
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomAuthentication(builder.Configuration);

// Application + Infrastructure
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHttpClient("cuenta", c => c.Timeout = TimeSpan.FromSeconds(8));

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

// Swagger (Development only)
builder.Services.AddSwaggerGen(options =>
{
    var swaggerOptions = builder.Configuration.GetSection("Swagger").Get<OpenApiInfo>() ?? new OpenApiInfo { Title = "Notes API", Version = "v1" };
    options.SwaggerDoc("v1", swaggerOptions);
    options.DocumentFilter<ExcludeUnhandledEndpointsFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JSON Options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Apply pending migrations on start (the lab deploys without a separate migration step)
if (!app.Configuration.GetValue<bool>("Database:SkipMigrations"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}

// =========================
// PIPELINE CONFIGURATION
// =========================

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference();
    app.UseCors("DevFrontend");
}

// Security headers for the web and the API
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["Referrer-Policy"] = "same-origin";
    h["X-Frame-Options"] = "DENY";
    h["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob: https:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

var sso = SsoConfig.From(app.Configuration);
app.UseLabGuards(sso);

// Web (Angular build in wwwroot): hashed files are immutable, index.html is never cached
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = ctx.File.Name == "index.html"
            ? "no-cache"
            : "public, max-age=31536000, immutable";
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// =========================
// ENDPOINTS
// =========================

app.MapCategoriesEndpoints();
app.MapNotesEndpoints();
app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapLocalStorageEndpoints();
app.MapLab(sso);

app.MapHealthChecks("/health");

// Unknown API routes are 404; any other route is the SPA
app.MapFallback("/api/{**rest}", () => Results.NotFound(new { error = "No existe" }));
app.MapFallback("/auth/{**rest}", () => Results.NotFound(new { error = "No existe" }));
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});

app.Run();

/// <summary>Entry point (public for WebApplicationFactory in the tests)</summary>
public partial class Program;
