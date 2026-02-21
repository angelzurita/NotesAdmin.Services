using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Categories.Queries.GetAllCategories;

/// <summary>
/// Query to get all categories
/// </summary>
public record GetAllCategoriesQuery : IRequest<ApiResponse<List<CategoryDto>>>;

/// <summary>
/// Category DTO
/// </summary>
public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
