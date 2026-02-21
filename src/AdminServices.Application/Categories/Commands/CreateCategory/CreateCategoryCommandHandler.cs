using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        IGenericRepository<Category> categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var category = new Category(request.Name, request.Description);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(category.Id, "Category created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error creating category: {ex.Message}");
        }
    }
}
