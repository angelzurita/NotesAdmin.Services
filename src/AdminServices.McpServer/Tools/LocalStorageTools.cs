using System.ComponentModel;
using System.Text.Json;
using AdminServices.Application.LocalStorage.Queries.GetAllDocuments;
using AdminServices.Application.LocalStorage.Queries.GetDocumentById;
using AdminServices.Application.LocalStorage.Queries.GetFolderContents;
using AdminServices.Application.LocalStorage.Queries.GetFolderTree;
using MediatR;
using ModelContextProtocol.Server;

namespace AdminServices.McpServer.Tools;

/// <summary>
/// MCP Tools for Local Storage — allows AI assistants to browse folders and documents stored in the DB.
/// </summary>
[McpServerToolType]
public class LocalStorageTools
{
    private readonly IMediator _mediator;

    public LocalStorageTools(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get all documents stored in the database (metadata only, no binary data). Returns id, fileName, contentType, fileSizeBytes, description, tags, folderId, createdAt.")]
    public async Task<string> GetAllDocuments()
    {
        var result = await _mediator.Send(new GetAllDocumentsQuery());

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Get a single document's metadata by its GUID id.")]
    public async Task<string> GetDocumentById(
        [Description("The GUID of the document to retrieve.")] Guid id)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery(id));

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Get the full folder tree structure (all folders in hierarchy). Useful to understand the folder organisation before querying specific folder contents.")]
    public async Task<string> GetFolderTree()
    {
        var result = await _mediator.Send(new GetFolderTreeQuery());

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    [McpServerTool]
    [Description("Get the contents (subfolders and documents) of a specific folder by its GUID id. Use GetFolderTree first to discover folder ids.")]
    public async Task<string> GetFolderContents(
        [Description("The GUID of the folder to browse. Pass null or empty to get root-level items.")] Guid? folderId = null)
    {
        var result = await _mediator.Send(new GetFolderContentsQuery(folderId));

        if (!result.Success)
            return $"Error: {result.Message}";

        return JsonSerializer.Serialize(result.Data, JsonOptions);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };
}
