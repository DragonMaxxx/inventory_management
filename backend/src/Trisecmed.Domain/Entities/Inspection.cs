namespace Trisecmed.Domain.Entities;

public class Inspection : BaseEntity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DateOnly InspectionDate { get; set; }
    public DateOnly? NextInspectionDate { get; set; }
    public string? Result { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
