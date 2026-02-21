using AdminServices.Application.Users.Queries.GetAllUsers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

public static class UsersModule
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization("RequireAdminRole");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllUsersQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllUsers")
        .Produces(StatusCodes.Status200OK);

        return app;
    }
}
