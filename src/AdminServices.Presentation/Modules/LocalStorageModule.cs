using AdminServices.Application.LocalStorage.Commands.DeleteDocument;
using AdminServices.Application.LocalStorage.Commands.UploadDocument;
using AdminServices.Application.LocalStorage.Commands.CreateFolder;
using AdminServices.Application.LocalStorage.Commands.DeleteFolder;
using AdminServices.Application.LocalStorage.Commands.RenameFolder;
using AdminServices.Application.LocalStorage.Commands.MoveDocument;
using AdminServices.Application.LocalStorage.Queries.GetAllDocuments;
using AdminServices.Application.LocalStorage.Queries.GetDocumentById;
using AdminServices.Application.LocalStorage.Queries.GetFolderTree;
using AdminServices.Application.LocalStorage.Queries.GetFolderContents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AdminServices.Presentation.Modules;

public static class LocalStorageModule
{
    public static IEndpointRouteBuilder MapLocalStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/local-storage")
            .WithTags("LocalStorage")
            .WithOpenApi()
            .RequireAuthorization("RequireUserRole");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAllDocumentsQuery());
            return Results.Ok(result);
        })
        .WithName("GetAllDocuments")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDocumentByIdQuery(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("GetDocumentById")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/upload", async (IFormFile file, IMediator mediator, string? description = null, string? tags = null, Guid? folderId = null) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest("No file provided");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileDataBase64 = Convert.ToBase64String(memoryStream.ToArray());

            var command = new UploadDocumentCommand
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileDataBase64 = fileDataBase64,
                FileSizeBytes = file.Length,
                Description = description,
                Tags = tags,
                FolderId = folderId
            };

            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/local-storage/{result.Data!.Id}", result);
            return Results.BadRequest(result);
        })
        .WithName("UploadDocument")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        group.MapPost("/upload-base64", async (UploadDocumentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/local-storage/{result.Data!.Id}", result);
            return Results.BadRequest(result);
        })
        .WithName("UploadDocumentBase64")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteDocumentCommand(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("DeleteDocument")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/download", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDocumentByIdQuery(id));
            if (!result.Success)
                return Results.NotFound(result);

            var document = result.Data!;
            var fileBytes = Convert.FromBase64String(document.FileDataBase64);
            return Results.File(fileBytes, document.ContentType, document.FileName);
        })
        .WithName("DownloadDocument")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // =====================
        // FOLDER ENDPOINTS
        // =====================
        var folders = app.MapGroup("/api/local-storage/folders")
            .WithTags("LocalStorage - Folders")
            .WithOpenApi()
            .RequireAuthorization("RequireUserRole");

        // GET full recursive tree (optionally rooted at a specific folder)
        folders.MapGet("/tree", async (IMediator mediator, Guid? rootFolderId) =>
        {
            var result = await mediator.Send(new GetFolderTreeQuery(rootFolderId));
            return Results.Ok(result);
        })
        .WithName("GetFolderTree")
        .Produces(StatusCodes.Status200OK);

        // GET immediate contents of a folder (subfolders + documents)
        // Use /root to list everything at the root level
        folders.MapGet("/contents", async (IMediator mediator, Guid? folderId) =>
        {
            var result = await mediator.Send(new GetFolderContentsQuery(folderId));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("GetFolderContents")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // POST create a folder (root or subfolder via ParentFolderId)
        folders.MapPost("/", async (CreateFolderCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            if (result.Success)
                return Results.Created($"/api/local-storage/folders/{result.Data}", result);
            return Results.BadRequest(result);
        })
        .WithName("CreateFolder")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // PUT rename a folder
        folders.MapPut("/{id:guid}/rename", async (Guid id, RenameFolderRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new RenameFolderCommand(id, req.Name));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("RenameFolder")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE a folder and all its subfolders/documents recursively
        folders.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteFolderCommand(id));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithName("DeleteFolder")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // PATCH move a document to a different folder
        app.MapPatch("/api/local-storage/{documentId:guid}/move", async (Guid documentId, MoveDocumentRequest req, IMediator mediator) =>
        {
            var result = await mediator.Send(new MoveDocumentCommand(documentId, req.folderId));
            if (!result.Success)
                return Results.NotFound(result);
            return Results.Ok(result);
        })
        .WithTags("LocalStorage")
        .WithOpenApi()
        .RequireAuthorization("RequireUserRole")
        .WithName("MoveDocument")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}

// Small request records used by minimal API endpoints
public record RenameFolderRequest(string Name);
public record MoveDocumentRequest(Guid? folderId);
