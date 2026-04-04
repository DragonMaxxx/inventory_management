namespace Trisecmed.Domain.Entities;

public class ServiceContract : BaseEntity
{
    public Guid ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;

    public string ContractNumber { get; set; } = null!;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Value { get; set; }
    public string? Notes { get; set; }
}
