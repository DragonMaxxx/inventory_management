using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Identity.DTOs;
using Trisecmed.Application.Identity.Interfaces;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokens>>;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthTokens>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthTokens>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive)
            return Result.Failure<AuthTokens>("Nieprawidłowy email lub hasło.");

        if (user.ActivationToken is not null)
            return Result.Failure<AuthTokens>("Konto nie zostało aktywowane.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthTokens>("Nieprawidłowy email lub hasło.");

        var tokens = _jwtService.GenerateTokens(user);

        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }
}
