using AdminServices.Application.Common.Models;
using MediatR;

namespace AdminServices.Application.Auth.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterCommand : IRequest<ApiResponse<Guid>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = "User";
}
