using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Categories.Commands.UpdateCategory;

/// <summary>
/// Command to update an existing category
/// </summary>
public record UpdateCategoryCommand : IRequest<ApiResponse<Guid>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
