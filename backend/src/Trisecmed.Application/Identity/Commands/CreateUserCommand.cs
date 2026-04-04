using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Identity.DTOs;
using Trisecmed.Application.Identity.Interfaces;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Commands;

public record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    Guid? DepartmentId) : IRequest<Result<UserDto>>;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (existing is not null)
            return Result.Failure<UserDto>($"Użytkownik z emailem '{request.Email}' już istnieje.");

        var activationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash("TEMP_" + Guid.NewGuid()), // placeholder — user sets password on activation
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            ActivationToken = activationToken
        };

        await _userRepo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.Role, user.DepartmentId, user.IsActive, user.LastLoginAt, user.CreatedAt));
    }
}
