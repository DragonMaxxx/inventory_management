using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Unit.Tests.Domain;

public class UserTests
{
    [Fact]
    public void NewUser_ShouldBeActive_ByDefault()
    {
        var user = new User();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void User_AllRoles_ShouldBeDefined()
    {
        var roles = Enum.GetValues<UserRole>();
        Assert.Equal(4, roles.Length);
        Assert.Contains(UserRole.Nurse, roles);
        Assert.Contains(UserRole.EquipmentWorker, roles);
        Assert.Contains(UserRole.EquipmentManager, roles);
        Assert.Contains(UserRole.Administrator, roles);
    }
}
