using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Unit.Tests.Domain;

public class DeviceTests
{
    [Fact]
    public void NewDevice_ShouldHaveActiveStatus_ByDefault()
    {
        var device = new Device();
        Assert.Equal(DeviceStatus.Active, device.Status);
    }

    [Fact]
    public void NewDevice_ShouldHaveEmptyCollections()
    {
        var device = new Device();
        Assert.Empty(device.Inspections);
        Assert.Empty(device.Failures);
    }

    [Fact]
    public void Device_ShouldStoreAllProperties()
    {
        var categoryId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();

        var device = new Device
        {
            Name = "RTG Siemens",
            InventoryNumber = "RTG-001",
            SerialNumber = "SN-123",
            Manufacturer = "Siemens",
            Model = "Multix",
            CategoryId = categoryId,
            DepartmentId = departmentId,
            PurchasePrice = 250000m,
            Status = DeviceStatus.InRepair
        };

        Assert.Equal("RTG Siemens", device.Name);
        Assert.Equal("RTG-001", device.InventoryNumber);
        Assert.Equal("SN-123", device.SerialNumber);
        Assert.Equal("Siemens", device.Manufacturer);
        Assert.Equal(categoryId, device.CategoryId);
        Assert.Equal(250000m, device.PurchasePrice);
        Assert.Equal(DeviceStatus.InRepair, device.Status);
    }
}
