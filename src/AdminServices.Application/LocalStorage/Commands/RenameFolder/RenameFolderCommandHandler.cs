using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.LocalStorage.Commands.RenameFolder;

public class RenameFolderCommandHandler : IRequestHandler<RenameFolderCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<StorageFolder> _folderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RenameFolderCommandHandler(
        IGenericRepository<StorageFolder> folderRepository,
        IUnitOfWork unitOfWork)
    {
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(RenameFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _folderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (folder is null)
                return ApiResponse<Guid>.ErrorResponse("Folder not found");

            folder.Rename(request.Name);
            _folderRepository.Update(folder);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(folder.Id, "Folder renamed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error renaming folder: {ex.Message}");
        }
    }
}
