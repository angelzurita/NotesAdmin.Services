using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApiResponse<List<UserDto>>>
{
    private readonly IGenericRepository<User> _userRepository;

    public GetAllUsersQueryHandler(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return ApiResponse<List<UserDto>>.SuccessResponse(userDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<UserDto>>.ErrorResponse($"Error retrieving users: {ex.Message}");
        }
    }
}
