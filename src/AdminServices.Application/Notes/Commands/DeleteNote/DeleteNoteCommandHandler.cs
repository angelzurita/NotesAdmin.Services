using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Commands.DeleteNote;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Note> _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNoteCommandHandler(
        IGenericRepository<Note> noteRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await _noteRepository.GetByIdAsync(request.Id, cancellationToken);
            if (note is null)
                return ApiResponse<Guid>.ErrorResponse("Note not found");

            _noteRepository.Remove(note);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(request.Id, "Note deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error deleting note: {ex.Message}");
        }
    }
}
