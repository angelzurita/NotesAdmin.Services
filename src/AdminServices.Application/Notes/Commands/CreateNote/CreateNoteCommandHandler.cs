using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Commands.CreateNote;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Note> _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNoteCommandHandler(
        IGenericRepository<Note> noteRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = new Note(
                request.Title,
                request.Content,
                request.CategoryId,
                request.Image1,
                request.Image2,
                request.Metadata);

            await _noteRepository.AddAsync(note, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(note.Id, "Note created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error creating note: {ex.Message}");
        }
    }
}
