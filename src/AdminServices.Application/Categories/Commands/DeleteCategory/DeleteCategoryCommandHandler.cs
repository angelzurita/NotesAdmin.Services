using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return ApiResponse<Guid>.ErrorResponse("Category not found");

            _categoryRepository.Remove(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(request.Id, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error deleting category: {ex.Message}");
        }
    }
}
