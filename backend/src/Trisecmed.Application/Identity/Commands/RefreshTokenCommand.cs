using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Identity.DTOs;
using Trisecmed.Application.Identity.Interfaces;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokens>>;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokens>>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthTokens>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || !user.IsActive || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return Result.Failure<AuthTokens>("Refresh token jest nieprawidłowy lub wygasł.");

        var tokens = _jwtService.GenerateTokens(user);

        user.RefreshToken = tokens.RefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }
}
