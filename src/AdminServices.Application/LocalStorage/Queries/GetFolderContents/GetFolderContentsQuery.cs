using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetFolderContents;

/// <summary>
/// Returns the immediate contents of a folder: direct subfolders + documents
/// </summary>
public record GetFolderContentsQuery(Guid? FolderId) : IRequest<ApiResponse<FolderContentsDto>>;

public class FolderContentsDto
{
    public Guid? FolderId { get; set; }
    public string? FolderName { get; set; }
    public List<FolderSummaryDto> SubFolders { get; set; } = new();
    public List<FolderDocumentDto> Documents { get; set; } = new();
}

public class FolderSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ChildFolderCount { get; set; }
    public int DocumentCount { get; set; }
}

public class FolderDocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? FolderId { get; init; }

}
