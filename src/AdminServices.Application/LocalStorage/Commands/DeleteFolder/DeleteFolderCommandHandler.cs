using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.DeleteFolder;

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IGenericRepository<LocalDocument> _documentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFolderCommandHandler(
        IGenericRepository<StorageFolder> folderRepository,
        IGenericRepository<LocalDocument> documentRepository,
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _folderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (folder is null)
                return ApiResponse<Guid>.ErrorResponse("Folder not found");

            // Load all folders and documents to build deletion lists recursively
            var allFolders = (await _folderRepository.GetAllAsync(cancellationToken)).ToList();
            var allDocuments = (await _documentRepository.GetAllAsync(cancellationToken)).ToList();

            var foldersToDelete = CollectDescendants(request.Id, allFolders);
            foldersToDelete.Add(folder);

            var folderIds = foldersToDelete.Select(f => f.Id).ToHashSet();
            var documentsToDelete = allDocuments.Where(d => d.FolderId.HasValue && folderIds.Contains(d.FolderId.Value)).ToList();

            if (documentsToDelete.Count > 0)
                _documentRepository.RemoveRange(documentsToDelete);

            _folderRepository.RemoveRange(foldersToDelete);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(request.Id, $"Folder and {foldersToDelete.Count - 1} subfolder(s) with {documentsToDelete.Count} document(s) deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error deleting folder: {ex.Message}");
        }
    }

    /// <summary>
    /// Recursively collects all descendant folders (does NOT include the root itself)
    /// </summary>
    private static List<StorageFolder> CollectDescendants(Guid parentId, List<StorageFolder> allFolders)
    {
        var result = new List<StorageFolder>();
        var directChildren = allFolders.Where(f => f.ParentFolderId == parentId).ToList();

        foreach (var child in directChildren)
        {
            result.Add(child);
            result.AddRange(CollectDescendants(child.Id, allFolders));
        }

        return result;
    }
}
