using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.MoveDocument;

/// <summary>
/// Command to move a document to a different folder (or root if FolderId is null)
/// </summary>
public record MoveDocumentCommand(Guid DocumentId, Guid? TargetFolderId) : IRequest<ApiResponse<Guid>>;
