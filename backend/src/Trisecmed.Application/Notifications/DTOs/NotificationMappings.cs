using Trisecmed.Domain.Entities;

namespace Trisecmed.Application.Notifications.DTOs;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Subject = n.Subject,
        Body = n.Body,
        RecipientEmail = n.RecipientEmail,
        RecipientUserId = n.RecipientUserId,
        IsSent = n.IsSent,
        SentAt = n.SentAt,
        ErrorMessage = n.ErrorMessage,
        CreatedAt = n.CreatedAt,
    };
}
