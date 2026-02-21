using AdminServices.Application.Categories.Commands.CreateCategory;
using AdminServices.Application.Categories.Commands.DeleteCategory;
using AdminServices.Application.Categories.Commands.UpdateCategory;
using AdminServices.Application.Categories.Queries.GetAllCategories;
using AdminServices.Application.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

/// <summary>
/// Categories module endpoints
/// </summary>
public static class CategoriesModule
{
    public static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllCategoriesQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllCategories")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCategoryByIdQuery(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("GetCategoryById")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateCategoryCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/categories/{result.Data}", result);
            return Results.BadRequest(result);
        })
        .WithName("CreateCategory")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return Results.BadRequest("Route ID does not match command ID");

            var result = await mediator.Send(command);
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteCategoryCommand(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("DeleteCategory")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
