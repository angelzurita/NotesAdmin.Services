using AdminServices.Application;
using AdminServices.Infrastructure;
using AdminServices.Presentation.Authentication;
using AdminServices.Presentation.Authorization;
using AdminServices.Presentation.Filters;
using AdminServices.Presentation.Middlewares;
using AdminServices.Presentation.Modules;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add environment variables
builder.Configuration.AddEnvironmentVariables();

// Global Exception Handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// =========================
// ✅ CORS CONFIGURATION
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",     // Angular default
                "https://localhost:4200",
                "https://localhost:6001"     // Si estás usando este puerto
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestProperties
       | HttpLoggingFields.RequestBody
       | HttpLoggingFields.ResponsePropertiesAndHeaders;

    logging.RequestBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Security
builder.Services.AddAuthorization();
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomAuthentication(builder.Configuration);

// Application + Infrastructure
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    var swaggerOptions = builder.Configuration.GetSection("Swagger").Get<OpenApiInfo>();
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

// =========================
// PIPELINE CONFIGURATION
// =========================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpLogging();

app.UseHttpsRedirection();

// 🔥 MUY IMPORTANTE EN MINIMAL APIs
app.UseRouting();

// ✅ CORS DEBE IR ANTES DE AUTH
app.UseCors("AllowFrontend");

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

app.MapHealthChecks("/health");

app.Run();