using Trisecmed.Domain.Enums;

namespace Trisecmed.Domain.Entities;

public class FailureStatusHistory
{
    public Guid Id { get; set; }
    public Guid FailureId { get; set; }
    public Failure Failure { get; set; } = null!;

    public FailureStatus OldStatus { get; set; }
    public FailureStatus NewStatus { get; set; }

    public Guid ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
