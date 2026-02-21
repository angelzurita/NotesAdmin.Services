using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return ApiResponse<Guid>.ErrorResponse("Category not found");

            category.Update(request.Name, request.Description);
            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(category.Id, "Category updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error updating category: {ex.Message}");
        }
    }
}
