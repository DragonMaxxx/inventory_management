using Trisecmed.Domain.Enums;

namespace Trisecmed.Application.Identity.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    Guid? DepartmentId,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
