using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Commands.UpdateNote;

public class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Note> _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNoteCommandHandler(
        IGenericRepository<Note> noteRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note is null)
                return ApiResponse<Guid>.ErrorResponse("Note not found");

            note.Update(request.Title, request.Content, request.CategoryId, request.Image1, request.Image2, request.Metadata);
            _noteRepository.Update(note);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(note.Id, "Note updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error updating note: {ex.Message}");
        }
    }
}
