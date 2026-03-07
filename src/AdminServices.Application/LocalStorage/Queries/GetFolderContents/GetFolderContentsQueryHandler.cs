using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetFolderContents;

public class GetFolderContentsQueryHandler : IRequestHandler<GetFolderContentsQuery, ApiResponse<FolderContentsDto>>
{
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IGenericRepository<LocalDocument> _documentRepository;

    public GetFolderContentsQueryHandler(
        IGenericRepository<StorageFolder> folderRepository,
        IGenericRepository<LocalDocument> documentRepository)
    {
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
    }

    public async Task<ApiResponse<FolderContentsDto>> Handle(GetFolderContentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            string? folderName = null;

            if (request.FolderId.HasValue)
            {
                var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value, cancellationToken);
                if (folder is null)
                    return ApiResponse<FolderContentsDto>.ErrorResponse("Folder not found");

                folderName = folder.Name;
            }

            var allFolders = (await _folderRepository.GetAllAsync(cancellationToken)).ToList();
            var allDocuments = (await _documentRepository.GetAllAsync(cancellationToken)).ToList();

            // Direct subfolders of this folder
            var subFolders = allFolders
                .Where(f => f.ParentFolderId == request.FolderId)
                .Select(f => new FolderSummaryDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    CreatedAt = f.CreatedAt,
                    ChildFolderCount = allFolders.Count(c => c.ParentFolderId == f.Id),
                    DocumentCount = allDocuments.Count(d => d.FolderId == f.Id)
                })
                .OrderBy(f => f.Name)
                .ToList();

            // Documents directly in this folder
            var documents = allDocuments
                .Where(d => d.FolderId == request.FolderId)
                .Select(d => new FolderDocumentDto
                {
                    Id = d.Id,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    FileSizeBytes = d.FileSizeBytes,
                    Description = d.Description,
                    Tags = d.Tags,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    FolderId = d.FolderId
                })
                .OrderBy(d => d.FileName)
                .ToList();

            var result = new FolderContentsDto
            {
                FolderId = request.FolderId,
                FolderName = folderName ?? "Root",
                SubFolders = subFolders,
                Documents = documents
            };

            return ApiResponse<FolderContentsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<FolderContentsDto>.ErrorResponse($"Error retrieving folder contents: {ex.Message}");
        }
    }
}
