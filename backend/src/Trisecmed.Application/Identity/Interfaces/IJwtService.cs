using Trisecmed.Application.Identity.DTOs;
using Trisecmed.Domain.Entities;

namespace Trisecmed.Application.Identity.Interfaces;

public interface IJwtService
{
    AuthTokens GenerateTokens(User user);
    string GenerateRefreshToken();
}
