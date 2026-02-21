using AdminServices.Application.Categories.Queries.GetAllCategories;
using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryDto>>
{
    private readonly IGenericRepository<Category> _categoryRepository;

    public GetCategoryByIdQueryHandler(IGenericRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ApiResponse<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (category is null)
                return ApiResponse<CategoryDto>.ErrorResponse("Category not found");

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                CreatedBy = category.CreatedBy
            };

            return ApiResponse<CategoryDto>.SuccessResponse(categoryDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<CategoryDto>.ErrorResponse($"Error retrieving category: {ex.Message}");
        }
    }
}
