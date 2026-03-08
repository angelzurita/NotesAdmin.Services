using AdminServices.Application;
using AdminServices.Infrastructure;
using AdminServices.McpServer.Tools;

var builder = WebApplication.CreateBuilder(args);

// Register Application + Infrastructure layers
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register MCP Server with HTTP/SSE transport (network-accessible)
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(NoteTools).Assembly);

// Suppress noise logs
builder.Logging.AddFilter("Quartz", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

var app = builder.Build();

// API Key middleware — all MCP requests must include X-Api-Key header
var apiKey = app.Configuration["McpServer:ApiKey"];
if (!string.IsNullOrEmpty(apiKey))
{
    app.Use(async (context, next) =>
    {
        // Skip health check
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != apiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: invalid or missing X-Api-Key header.");
            return;
        }

        await next(context);
    });
}

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AdminServices.McpServer" }));

// MCP SSE endpoint
app.MapMcp("/mcp");

app.Run();
