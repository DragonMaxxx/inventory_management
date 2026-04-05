using FluentAssertions;
using Trisecmed.Application.Equipment.Commands;

namespace Trisecmed.Unit.Tests.Application;

public class CreateInspectionValidatorTests
{
    private readonly CreateInspectionValidator _validator = new();

    [Fact]
    public async Task ValidCommand_ShouldPass()
    {
        var cmd = new CreateInspectionCommand
        {
            DeviceId = Guid.NewGuid(),
            InspectionDate = DateOnly.FromDateTime(DateTime.Today),
            PerformedBy = "Serwis ABC",
        };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EmptyDeviceId_ShouldFail()
    {
        var cmd = new CreateInspectionCommand
        {
            DeviceId = Guid.Empty,
            InspectionDate = DateOnly.FromDateTime(DateTime.Today),
        };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task PerformedByTooLong_ShouldFail()
    {
        var cmd = new CreateInspectionCommand
        {
            DeviceId = Guid.NewGuid(),
            InspectionDate = DateOnly.FromDateTime(DateTime.Today),
            PerformedBy = new string('A', 256),
        };
        var result = await _validator.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }
}
