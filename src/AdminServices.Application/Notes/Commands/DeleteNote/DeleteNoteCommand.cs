using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Notes.Commands.DeleteNote;

/// <summary>
/// Command to delete a note
/// </summary>
public record DeleteNoteCommand(Guid Id) : IRequest<ApiResponse<Guid>>;
