namespace Trisecmed.Application.Notifications.DTOs;

public record NotificationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Body { get; init; } = null!;
    public string RecipientEmail { get; init; } = null!;
    public Guid? RecipientUserId { get; init; }
    public bool IsSent { get; init; }
    public DateTime? SentAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
}
