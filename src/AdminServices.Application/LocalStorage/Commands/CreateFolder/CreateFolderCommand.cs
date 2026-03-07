using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.CreateFolder;

/// <summary>
/// Command to create a folder or subfolder in the local storage tree
/// </summary>
public record CreateFolderCommand : IRequest<ApiResponse<Guid>>
{
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// null = root folder; set to parent Guid for subfolders (recursive)
    /// </summary>
    public Guid? ParentFolderId { get; init; }
}
