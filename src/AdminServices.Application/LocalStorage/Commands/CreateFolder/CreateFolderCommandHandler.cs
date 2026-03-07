using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.CreateFolder;

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFolderCommandHandler(
        IGenericRepository<StorageFolder> folderRepository,
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate parent exists when creating a subfolder
            if (request.ParentFolderId.HasValue)
            {
                var parent = await _folderRepository.GetByIdAsync(request.ParentFolderId.Value, cancellationToken);
                if (parent is null)
                    return ApiResponse<Guid>.ErrorResponse("Parent folder not found");
            }

            var folder = new StorageFolder(request.Name, request.ParentFolderId);

            await _folderRepository.AddAsync(folder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(folder.Id, "Folder created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error creating folder: {ex.Message}");
        }
    }
}
