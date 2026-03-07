using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.MoveDocument;

public class MoveDocumentCommandHandler : IRequestHandler<MoveDocumentCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<LocalDocument> _documentRepository;
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MoveDocumentCommandHandler(
        IGenericRepository<LocalDocument> documentRepository,
        IGenericRepository<StorageFolder> folderRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(MoveDocumentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
            if (document is null)
                return ApiResponse<Guid>.ErrorResponse("Document not found");

            if (request.TargetFolderId.HasValue)
            {
                var folder = await _folderRepository.GetByIdAsync(request.TargetFolderId.Value, cancellationToken);
                if (folder is null)
                    return ApiResponse<Guid>.ErrorResponse("Target folder not found");
            }

            document.MoveToFolder(request.TargetFolderId);
            _documentRepository.Update(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(document.Id, "Document moved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error moving document: {ex.Message}");
        }
    }
}
