using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Application.Identity.Interfaces;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Commands;

public record ActivateAccountCommand(string Token, string NewPassword) : IRequest<Result>;

public class ActivateAccountHandler : IRequestHandler<ActivateAccountCommand, Result>
{
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public ActivateAccountHandler(IUserRepository userRepo, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByActivationTokenAsync(request.Token, cancellationToken);

        if (user is null)
            return Result.Failure("Token aktywacyjny jest nieprawidłowy.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.ActivationToken = null;
        _userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
