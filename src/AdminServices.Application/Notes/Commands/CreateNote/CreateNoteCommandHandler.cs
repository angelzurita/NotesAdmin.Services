using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Notes.Commands.CreateNote;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Note> _noteRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNoteCommandHandler(
        IGenericRepository<Note> noteRepository,
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _noteRepository = noteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // The category must be one of the user's own (the query filter hides the rest)
            if (request.CategoryId.HasValue
                && await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken) is null)
                return ApiResponse<Guid>.ErrorResponse("Category not found");

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
