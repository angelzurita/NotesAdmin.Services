using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetDocumentById;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, ApiResponse<LocalDocumentDetailDto>>
{
    private readonly IGenericRepository<LocalDocument> _documentRepository;

    public GetDocumentByIdQueryHandler(IGenericRepository<LocalDocument> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ApiResponse<LocalDocumentDetailDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (document is null)
                return ApiResponse<LocalDocumentDetailDto>.ErrorResponse("Document not found");

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

            return ApiResponse<LocalDocumentDetailDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<LocalDocumentDetailDto>.ErrorResponse($"Error retrieving document: {ex.Message}");
        }
    }
}
