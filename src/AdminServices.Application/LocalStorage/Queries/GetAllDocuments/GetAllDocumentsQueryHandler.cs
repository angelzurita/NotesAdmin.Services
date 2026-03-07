using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetAllDocuments;

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, ApiResponse<List<LocalDocumentDto>>>
{
    private readonly IGenericRepository<LocalDocument> _documentRepository;

    public GetAllDocumentsQueryHandler(IGenericRepository<LocalDocument> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ApiResponse<List<LocalDocumentDto>>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync(cancellationToken);

            var dtos = documents.Select(d => new LocalDocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                ContentType = d.ContentType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                Tags = d.Tags,
                FolderId = d.FolderId,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                CreatedBy = d.CreatedBy
            }).ToList();

            return ApiResponse<List<LocalDocumentDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LocalDocumentDto>>.ErrorResponse($"Error retrieving documents: {ex.Message}");
        }
    }
}
