using AdminServices.Application.Common.Interfaces;
using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IGenericRepository<User> userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
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

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
                return ApiResponse<LoginResponse>.ErrorResponse("Invalid email or password");

            var token = _jwtTokenService.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            return ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponse>.ErrorResponse($"Error during login: {ex.Message}");
        }
    }
}
