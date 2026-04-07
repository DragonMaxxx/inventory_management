using Trisecmed.Domain.Entities;

namespace Trisecmed.Unit.Tests.Domain;

public class ServiceProviderTests
{
    [Fact]
    public void NewServiceProvider_ShouldHaveEmptyCollections()
    {
        var sp = new ServiceProvider();
        Assert.Empty(sp.Failures);
        Assert.Empty(sp.ServiceContracts);
    }

    [Fact]
    public void ServiceContract_ShouldHaveRequiredFields()
    {
        var contract = new ServiceContract
        {
            ContractNumber = "SRV-001",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2027, 1, 1),
            Value = 50000m,
        };

        Assert.Equal("SRV-001", contract.ContractNumber);
        Assert.Equal(50000m, contract.Value);
    }
}
