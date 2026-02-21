using AdminServices.Application.Notes.Commands.CreateNote;
using AdminServices.Application.Notes.Commands.DeleteNote;
using AdminServices.Application.Notes.Commands.UpdateNote;
using AdminServices.Application.Notes.Queries.GetAllNotes;
using AdminServices.Application.Notes.Queries.GetNoteById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

public static class NotesModule
{
    public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes")
            .WithTags("Notes")
            .WithOpenApi()
            .RequireAuthorization("RequireUserRole");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllNotesQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllNotes")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNoteByIdQuery(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("GetNoteById")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateNoteCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/notes/{result.Data}", result);
            return Results.BadRequest(result);
        })
        .WithName("CreateNote")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateNoteCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return Results.BadRequest("Route ID does not match command ID");

            var result = await mediator.Send(command);
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("UpdateNote")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteNoteCommand(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("DeleteNote")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
