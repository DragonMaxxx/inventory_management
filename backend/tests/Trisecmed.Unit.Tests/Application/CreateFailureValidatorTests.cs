using FluentValidation.TestHelper;
using Trisecmed.Application.Failures.Commands;

namespace Trisecmed.Unit.Tests.Application;

public class CreateFailureValidatorTests
{
    private readonly CreateFailureValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateFailureCommand
        {
            DeviceId = Guid.NewGuid(),
            ReportedByUserId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Description = "Aparat nie włącza się",
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyDeviceId_ShouldFail()
    {
        var command = new CreateFailureCommand
        {
            DeviceId = Guid.Empty,
            ReportedByUserId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Description = "Awaria",
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId);
    }

    [Fact]
    public void Validate_EmptyDescription_ShouldFail()
    {
        var command = new CreateFailureCommand
        {
            DeviceId = Guid.NewGuid(),
            ReportedByUserId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Description = "",
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        var command = new CreateFailureCommand
        {
            DeviceId = Guid.NewGuid(),
            ReportedByUserId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            Description = new string('A', 2001),
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
