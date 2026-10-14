using AdminServices.Application.Common.Interfaces;
using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly ISessionService _sessionService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IGenericRepository<User> userRepository,
        ISessionService sessionService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _sessionService = sessionService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            var user = users.FirstOrDefault();

            if (user is null)
                return ApiResponse<LoginResponse>.ErrorResponse("Invalid email or password");

            if (!user.IsActive)
                return ApiResponse<LoginResponse>.ErrorResponse("User account is deactivated");

            // Lab-account users have no local password
            if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                return ApiResponse<LoginResponse>.ErrorResponse("Invalid email or password");

            var issued = await _sessionService.SignInAsync(user, cancellationToken: cancellationToken);

            var response = new LoginResponse
            {
                Token = issued.Token,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = issued.ExpiresAt
            };

            return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Error during login", new List<string> { ex.Message });
        }
    }
}
