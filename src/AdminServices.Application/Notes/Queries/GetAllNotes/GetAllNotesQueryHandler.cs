using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Queries.GetAllNotes;

public class GetAllNotesQueryHandler : IRequestHandler<GetAllNotesQuery, ApiResponse<List<NoteDto>>>
{
    private readonly IGenericRepository<Note> _noteRepository;

    public GetAllNotesQueryHandler(IGenericRepository<Note> noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<ApiResponse<List<NoteDto>>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notes = await _noteRepository.GetAllAsync(cancellationToken);

            var noteDtos = notes.Select(n => new NoteDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                CategoryId = n.CategoryId,
                Image1 = n.Image1,
                Image2 = n.Image2,
                Metadata = n.Metadata,
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                CreatedBy = n.CreatedBy
            }).ToList();

            return ApiResponse<List<NoteDto>>.SuccessResponse(noteDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NoteDto>>.ErrorResponse($"Error retrieving notes: {ex.Message}");
        }
    }
}
