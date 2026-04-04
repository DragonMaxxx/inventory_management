using FluentValidation.TestHelper;
using Trisecmed.Application.Equipment.Commands;

namespace Trisecmed.Unit.Tests.Application;

public class CreateDeviceValidatorTests
{
    private readonly CreateDeviceValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateDeviceCommand
        {
            Name = "Aparat RTG",
            InventoryNumber = "RTG-001",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var command = new CreateDeviceCommand
        {
            Name = "",
            InventoryNumber = "RTG-001",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyInventoryNumber_ShouldFail()
    {
        var command = new CreateDeviceCommand
        {
            Name = "RTG",
            InventoryNumber = "",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InventoryNumber);
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldFail()
    {
        var command = new CreateDeviceCommand
        {
            Name = "RTG",
            InventoryNumber = "RTG-001",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.Empty,
            DepartmentId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_NegativePrice_ShouldFail()
    {
        var command = new CreateDeviceCommand
        {
            Name = "RTG",
            InventoryNumber = "RTG-001",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            PurchasePrice = -100m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice);
    }

    [Fact]
    public void Validate_NameTooLong_ShouldFail()
    {
        var command = new CreateDeviceCommand
        {
            Name = new string('A', 256),
            InventoryNumber = "RTG-001",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
