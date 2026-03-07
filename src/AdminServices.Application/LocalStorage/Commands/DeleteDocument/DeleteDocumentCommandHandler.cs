using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.DeleteDocument;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<LocalDocument> _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDocumentCommandHandler(
        IGenericRepository<LocalDocument> documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (document is null)
                return ApiResponse<Guid>.ErrorResponse("Document not found");

            _documentRepository.Remove(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(request.Id, "Document deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error deleting document: {ex.Message}");
        }
    }
}
