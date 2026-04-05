using FluentAssertions;
using Trisecmed.Application.Equipment.Commands;

namespace Trisecmed.Unit.Tests.Application;

public class UpdateDeviceValidatorTests
{
    private readonly UpdateDeviceValidator _validator = new();

    private static UpdateDeviceCommand ValidCommand => new()
    {
        Id = Guid.NewGuid(),
        Name = "Aparat RTG",
        InventoryNumber = "RTG-001",
        Manufacturer = "Siemens",
        Model = "Multix Impact",
        CategoryId = Guid.NewGuid(),
        DepartmentId = Guid.NewGuid(),
    };

    [Fact]
    public async Task ValidCommand_ShouldPass()
    {
        var result = await _validator.ValidateAsync(ValidCommand);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyId_ShouldFail()
    {
        var cmd = ValidCommand with { Id = Guid.Empty };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task EmptyName_ShouldFail()
    {
        var cmd = ValidCommand with { Name = "" };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task NegativePrice_ShouldFail()
    {
        var cmd = ValidCommand with { PurchasePrice = -100 };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }
}
