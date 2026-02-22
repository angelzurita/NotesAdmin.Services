using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.DeleteFolder;

/// <summary>
/// Command to delete a folder and all its subfolders/documents recursively
/// </summary>
public record DeleteFolderCommand(Guid Id) : IRequest<ApiResponse<Guid>>;
