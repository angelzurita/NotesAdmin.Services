using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetFolderTree;

/// <summary>
/// Returns the full recursive folder tree.
/// Pass a RootFolderId to get subtree, or null for the entire tree.
/// </summary>
public record GetFolderTreeQuery(Guid? RootFolderId = null) : IRequest<ApiResponse<List<FolderTreeDto>>>;

/// <summary>
/// Recursive DTO representing a folder node in the tree
/// </summary>
public class FolderTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentFolderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DocumentCount { get; set; }
    public List<FolderTreeDto> Children { get; set; } = new();
}
