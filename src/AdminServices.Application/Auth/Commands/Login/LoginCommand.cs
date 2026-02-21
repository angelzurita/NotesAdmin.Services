using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and get a JWT token
/// </summary>
public record LoginCommand : IRequest<ApiResponse<LoginResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Login response with token
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
