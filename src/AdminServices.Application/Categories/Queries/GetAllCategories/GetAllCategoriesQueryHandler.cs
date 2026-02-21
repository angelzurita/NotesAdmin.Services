using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, ApiResponse<List<CategoryDto>>>
{
    private readonly IGenericRepository<Category> _categoryRepository;

    public GetAllCategoriesQueryHandler(IGenericRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);

            var categoryDtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy
            }).ToList();

            return ApiResponse<List<CategoryDto>>.SuccessResponse(categoryDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<CategoryDto>>.ErrorResponse($"Error retrieving categories: {ex.Message}");
        }
    }
}
