using AdminServices.Application.Common.Models;
using AdminServices.Application.Notes.Queries.GetAllNotes;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Queries.GetNoteById;

public class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, ApiResponse<NoteDto>>
{
    private readonly IGenericRepository<Note> _noteRepository;

    public GetNoteByIdQueryHandler(IGenericRepository<Note> noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<ApiResponse<NoteDto>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note is null)
                return ApiResponse<NoteDto>.ErrorResponse("Note not found");

            var noteDto = new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CategoryId = note.CategoryId,
                Image1 = note.Image1,
                Image2 = note.Image2,
                Metadata = note.Metadata,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt,
                CreatedBy = note.CreatedBy
            };

            return ApiResponse<NoteDto>.SuccessResponse(noteDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<NoteDto>.ErrorResponse($"Error retrieving note: {ex.Message}");
        }
    }
}
