using Trisecmed.Domain.Enums;

namespace Trisecmed.Domain.Entities;

public class Failure : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public Guid ReportedByUserId { get; set; }
    public User ReportedByUser { get; set; } = null!;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public string Description { get; set; } = null!;
    public FailureStatus Status { get; set; } = FailureStatus.Open;
    public FailurePriority Priority { get; set; } = FailurePriority.Medium;

    public Guid? ServiceProviderId { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }

    public decimal? RepairCost { get; set; }
    public string? RepairDescription { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public ICollection<FailureStatusHistory> StatusHistory { get; set; } = [];
}
