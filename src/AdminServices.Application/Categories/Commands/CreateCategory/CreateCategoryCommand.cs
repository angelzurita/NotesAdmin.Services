using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category
/// </summary>
public record CreateCategoryCommand : IRequest<ApiResponse<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
