using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Commands;

public record LogoutCommand(Guid UserId) : IRequest<Result>;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure("Użytkownik nie znaleziony.");

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
