using AdminServices.Application.Common.Models;
using AdminServices.Application.LocalStorage.Queries.GetDocumentById;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.UploadDocument;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, ApiResponse<LocalDocumentDetailDto>>
{
    private readonly IGenericRepository<LocalDocument> _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UploadDocumentCommandHandler(
        IGenericRepository<LocalDocument> documentRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<LocalDocumentDetailDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var document = new LocalDocument(
                request.FileName,
                request.ContentType,
                request.FileDataBase64,
                request.FileSizeBytes,
                request.Description,
                request.Tags,
                request.FolderId);

            await _documentRepository.AddAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new LocalDocumentDetailDto
            {
                Id = document.Id,
                FileName = document.FileName,
                ContentType = document.ContentType,
                FileDataBase64 = document.FileDataBase64,
                FileSizeBytes = document.FileSizeBytes,
                Description = document.Description,
                Tags = document.Tags,
                FolderId = document.FolderId,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                CreatedBy = document.CreatedBy
            };

            return ApiResponse<LocalDocumentDetailDto>.SuccessResponse(dto, "Document uploaded successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<LocalDocumentDetailDto>.ErrorResponse($"Error uploading document: {ex.Message}");
        }
    }
}
