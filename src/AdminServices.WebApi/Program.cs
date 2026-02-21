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

// Application Insights - commented for local development
// builder.Services.AddApplicationInsights(builder.Logging, builder.Configuration);

//Adding global error handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

// Add Logging Middleware Services
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestProperties
       | HttpLoggingFields.RequestBody
       | HttpLoggingFields.ResponsePropertiesAndHeaders;
    logging.RequestBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

// Register application services
builder.Services.AddAuthorization();
builder.Services.AddCustomAuthorization();
builder.Services.AddCustomAuthentication(builder.Configuration);

// Add Application Services
builder.Services.AddApplication(builder.Configuration);

// Add Healthchecks
builder.Services.AddHealthChecks();

// Add Application Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);
// .AddRedis(builder.Configuration) // Commented for local development
// .AddServiceBus(builder.Configuration); // Commented for local development

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
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

// Configure JSON options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapCategoriesEndpoints();
app.MapNotesEndpoints();
app.MapAuthEndpoints();
app.MapUsersEndpoints();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();
