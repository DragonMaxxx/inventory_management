using FluentValidation.TestHelper;
using Trisecmed.Application.Failures.Commands;

namespace Trisecmed.Unit.Tests.Application;

public class ResolveFailureValidatorTests
{
    private readonly ResolveFailureValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new ResolveFailureCommand
        {
            FailureId = Guid.NewGuid(),
            ResolvedByUserId = Guid.NewGuid(),
            RepairCost = 1500m,
            RepairDescription = "Wymiana zasilacza",
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyFailureId_ShouldFail()
    {
        var command = new ResolveFailureCommand
        {
            FailureId = Guid.Empty,
            ResolvedByUserId = Guid.NewGuid(),
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FailureId);
    }

    [Fact]
    public void Validate_NegativeCost_ShouldFail()
    {
        var command = new ResolveFailureCommand
        {
            FailureId = Guid.NewGuid(),
            ResolvedByUserId = Guid.NewGuid(),
            RepairCost = -100m,
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RepairCost);
    }

    [Fact]
    public void Validate_NullCost_ShouldPass()
    {
        var command = new ResolveFailureCommand
        {
            FailureId = Guid.NewGuid(),
            ResolvedByUserId = Guid.NewGuid(),
            RepairCost = null,
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.RepairCost);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        var command = new ResolveFailureCommand
        {
            FailureId = Guid.NewGuid(),
            ResolvedByUserId = Guid.NewGuid(),
            RepairDescription = new string('X', 2001),
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RepairDescription);
    }
}
