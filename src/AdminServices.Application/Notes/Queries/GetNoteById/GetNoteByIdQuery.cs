using AdminServices.Application.Common.Models;
using AdminServices.Application.Notes.Queries.GetAllNotes;
using MediatR;

namespace AdminServices.Application.Notes.Queries.GetNoteById;

/// <summary>
/// Query to get a note by ID
/// </summary>
public record GetNoteByIdQuery(Guid Id) : IRequest<ApiResponse<NoteDto>>;
