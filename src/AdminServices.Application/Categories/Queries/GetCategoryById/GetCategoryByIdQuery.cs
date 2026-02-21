using AdminServices.Application.Categories.Queries.GetAllCategories;
using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Categories.Queries.GetCategoryById;

/// <summary>
/// Query to get a category by ID
/// </summary>
public record GetCategoryByIdQuery(Guid Id) : IRequest<ApiResponse<CategoryDto>>;
