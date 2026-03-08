using AdminServices.Application.Ai.Queries.ChatQuery;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

/// <summary>
/// AI Chat module — natural language interface to application data
/// </summary>
public static class AiModule
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/chat", async (ChatRequest request, IMediator mediator, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return Results.BadRequest(new { error = "Message cannot be empty" });

            var result = await mediator.Send(new AiChatQuery(request.Message, request.SessionId), ct);

            if (!result.Success)
                return Results.Problem(result.Error ?? "Unknown error", statusCode: StatusCodes.Status500InternalServerError);

            return Results.Ok(new ChatApiResponse(result.Message, result.SessionId));
        })
        .WithName("AiChat")
        .WithSummary("Chat with AI assistant")
        .WithDescription("Send a natural language message and receive an AI response with real-time data from the database")
        .Produces<ChatApiResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}

/// <summary>Request body for AI chat</summary>
public record ChatRequest(
    string Message,
    string? SessionId = null
);

/// <summary>Response from AI chat</summary>
public record ChatApiResponse(
    string Response,
    string? SessionId
);
