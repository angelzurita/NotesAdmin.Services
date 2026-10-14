using AdminServices.Application.Auth.Commands.Login;
using AdminServices.Application.Auth.Commands.Register;
using AdminServices.Application.Common.Interfaces;
using AdminServices.Presentation.Authentication;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

public static class AuthModule
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/register", async (RegisterCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/users/{result.Data}", result);
            return Results.BadRequest(result);
        })
        .WithName("Register")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Ok(result);
            return Results.Unauthorized();
        })
        .WithName("Login")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        // Who am I (used by the web to show the user and to check that the session is still open)
        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            id = user.FindFirst("uid")?.Value,
            email = user.FindFirst(ClaimTypes.Email)?.Value,
            name = user.FindFirst(ClaimTypes.Name)?.Value,
            role = user.FindFirst(ClaimTypes.Role)?.Value
        }))
        .WithName("Me")
        .RequireAuthorization();

        // Closes the current session: its token stops working right away
        group.MapPost("/logout", async (ClaimsPrincipal user, ISessionService sessions, CancellationToken ct) =>
        {
            if (user.GetSessionId() is { } sessionId)
                await sessions.RevokeAsync(sessionId, ct);
            return Results.NoContent();
        })
        .WithName("Logout")
        .RequireAuthorization();

        return app;
    }
}
