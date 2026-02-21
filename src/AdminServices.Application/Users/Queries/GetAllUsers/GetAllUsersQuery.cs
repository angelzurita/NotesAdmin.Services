using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Users.Queries.GetAllUsers;

/// <summary>
/// Query to get all users
/// </summary>
public record GetAllUsersQuery : IRequest<ApiResponse<List<UserDto>>>;

/// <summary>
/// User DTO (excludes sensitive data)
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
