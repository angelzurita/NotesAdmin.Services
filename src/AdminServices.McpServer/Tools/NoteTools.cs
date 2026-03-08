using System.ComponentModel;
using System.Text.Json;
using AdminServices.Application.Notes.Commands.CreateNote;
using AdminServices.Application.Notes.Commands.DeleteNote;
using AdminServices.Application.Notes.Commands.UpdateNote;
using AdminServices.Application.Notes.Queries.GetAllNotes;
using AdminServices.Application.Notes.Queries.GetNoteById;
using MediatR;
using ModelContextProtocol.Server;

namespace AdminServices.McpServer.Tools;

/// <summary>
/// MCP Tools for Notes — allows AI assistants to query and manage notes directly from the DB.
/// </summary>
[McpServerToolType]
public class NoteTools
{
    private readonly IMediator _mediator;

    public NoteTools(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get all notes stored in the database. Returns id, title, content, categoryId, categoryName, metadata, createdAt and updatedAt.")]
    public async Task<string> GetAllNotes()
    {
        var result = await _mediator.Send(new GetAllNotesQuery());

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Get a single note by its GUID id. Returns full note details including content.")]
    public async Task<string> GetNoteById(
        [Description("The GUID of the note to retrieve.")] Guid id)
    {
        var result = await _mediator.Send(new GetNoteByIdQuery(id));

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Create a new note in the database. Returns the GUID of the created note.")]
    public async Task<string> CreateNote(
        [Description("Title of the note.")] string title,
        [Description("Full content/body of the note.")] string content,
        [Description("Optional GUID of the category to assign this note to.")] Guid? categoryId = null,
        [Description("Optional metadata as a JSON string or free text.")] string? metadata = null)
    {
        var command = new CreateNoteCommand
        {
            Title = title,
            Content = content,
            CategoryId = categoryId,
            Metadata = metadata
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return $"Error: {result.Message}";

        return $"Note created successfully. Id: {result.Data}";
    }

    [McpServerTool]
    [Description("Update an existing note by its GUID id.")]
    public async Task<string> UpdateNote(
        [Description("The GUID of the note to update.")] Guid id,
        [Description("New title for the note.")] string title,
        [Description("New content/body for the note.")] string content,
        [Description("Optional GUID of the category.")] Guid? categoryId = null,
        [Description("Optional metadata as JSON or free text.")] string? metadata = null)
    {
        var command = new UpdateNoteCommand
        {
            Id = id,
            Title = title,
            Content = content,
            CategoryId = categoryId,
            Metadata = metadata
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return $"Error: {result.Message}";

        return "Note updated successfully.";
    }

    [McpServerTool]
    [Description("Delete a note permanently from the database by its GUID id.")]
    public async Task<string> DeleteNote(
        [Description("The GUID of the note to delete.")] Guid id)
    {
        var result = await _mediator.Send(new DeleteNoteCommand(id));

        if (!result.Success)
            return $"Error: {result.Message}";

        return "Note deleted successfully.";
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}
