using AdminServices.Application.Common.Interfaces;
using AdminServices.Application.Common.Models;
using AdminServices.Domain.Entities;
using AdminServices.Domain.Repositories;
using MediatR;

namespace AdminServices.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<Guid>>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IGenericRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingUsers = await _userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
            if (existingUsers.Any())
                return ApiResponse<Guid>.ErrorResponse("A user with this email already exists");

            var passwordHash = _passwordHasher.Hash(request.Password);
            var user = new User(request.Email, passwordHash, request.FullName, request.Role);

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<Guid>.SuccessResponse(user.Id, "User registered successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<Guid>.ErrorResponse($"Error registering user: {ex.Message}");
        }
    }
}
