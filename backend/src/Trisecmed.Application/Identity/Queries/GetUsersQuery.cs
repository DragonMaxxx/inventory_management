using MediatR;
using Trisecmed.Application.Identity.DTOs;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Identity.Queries;

public record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepo;

    public GetUsersHandler(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetAllAsync(cancellationToken);
        return users.Select(u => new UserDto(
            u.Id, u.Email, u.FirstName, u.LastName,
            u.Role, u.DepartmentId, u.IsActive, u.LastLoginAt, u.CreatedAt)).ToList();
    }
}
