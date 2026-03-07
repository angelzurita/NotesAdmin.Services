using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Queries.GetFolderTree;

public class GetFolderTreeQueryHandler : IRequestHandler<GetFolderTreeQuery, ApiResponse<List<FolderTreeDto>>>
{
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IGenericRepository<LocalDocument> _documentRepository;

    public GetFolderTreeQueryHandler(
        IGenericRepository<StorageFolder> folderRepository,
        IGenericRepository<LocalDocument> documentRepository)
    {
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
    }

    public async Task<ApiResponse<List<FolderTreeDto>>> Handle(GetFolderTreeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allFolders = (await _folderRepository.GetAllAsync(cancellationToken)).ToList();
            var allDocuments = (await _documentRepository.GetAllAsync(cancellationToken)).ToList();

            // Build a lookup: folderId -> document count
            var docCountByFolder = allDocuments
                .Where(d => d.FolderId.HasValue)
                .GroupBy(d => d.FolderId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build tree starting from the root level (or a specific folder's children)
            var tree = BuildTree(request.RootFolderId, allFolders, docCountByFolder);

            return ApiResponse<List<FolderTreeDto>>.SuccessResponse(tree);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FolderTreeDto>>.ErrorResponse($"Error retrieving folder tree: {ex.Message}");
        }
    }

    private static List<FolderTreeDto> BuildTree(
        Guid? parentId,
        List<StorageFolder> allFolders,
        Dictionary<Guid, int> docCountByFolder)
    {
        return allFolders
            .Where(f => f.ParentFolderId == parentId)
            .Select(f => new FolderTreeDto
            {
                Id = f.Id,
                Name = f.Name,
                ParentFolderId = f.ParentFolderId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                DocumentCount = docCountByFolder.GetValueOrDefault(f.Id, 0),
                // Recursive call for children
                Children = BuildTree(f.Id, allFolders, docCountByFolder)
            })
            .OrderBy(f => f.Name)
            .ToList();
    }
}
