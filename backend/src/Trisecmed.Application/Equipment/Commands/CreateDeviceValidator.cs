using FluentValidation;

namespace Trisecmed.Application.Equipment.Commands;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.InventoryNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Manufacturer).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(255);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0).When(x => x.PurchasePrice.HasValue);
    }
}
