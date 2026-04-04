namespace Trisecmed.Domain.Entities;

public class Notification : BaseEntity
{
    public string Type { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string RecipientEmail { get; set; } = null!;
    public Guid? RecipientUserId { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
