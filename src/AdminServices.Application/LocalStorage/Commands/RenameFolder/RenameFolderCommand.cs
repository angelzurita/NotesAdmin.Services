using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.RenameFolder;

/// <summary>
/// Command to rename an existing folder
/// </summary>
public record RenameFolderCommand(Guid Id, string Name) : IRequest<ApiResponse<Guid>>;
