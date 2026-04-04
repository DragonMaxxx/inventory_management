namespace Trisecmed.Domain.Entities;

public class ServiceProvider : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }

    public ICollection<Failure> Failures { get; set; } = [];
    public ICollection<ServiceContract> ServiceContracts { get; set; } = [];
}
