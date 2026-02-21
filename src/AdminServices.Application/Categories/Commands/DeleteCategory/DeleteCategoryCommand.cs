using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Categories.Commands.DeleteCategory;

/// <summary>
/// Command to delete a category
/// </summary>
public record DeleteCategoryCommand(Guid Id) : IRequest<ApiResponse<Guid>>;
