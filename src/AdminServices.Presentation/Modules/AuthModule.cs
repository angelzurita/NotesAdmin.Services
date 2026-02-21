using AdminServices.Application.Auth.Commands.Login;
using AdminServices.Application.Auth.Commands.Register;
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

        return app;
    }
}
