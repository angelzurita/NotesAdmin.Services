using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.DeleteDocument;

/// <summary>
/// Command to delete a stored document
/// </summary>
public record DeleteDocumentCommand(Guid Id) : IRequest<ApiResponse<Guid>>;
