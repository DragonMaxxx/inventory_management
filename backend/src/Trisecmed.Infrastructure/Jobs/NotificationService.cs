using Microsoft.Extensions.Logging;
using Trisecmed.Application.Notifications;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Infrastructure.Jobs;

public class NotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepo,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendAndLogAsync(string type, string recipientEmail, Guid? recipientUserId, string subject, string htmlBody)
    {
        var notification = new Notification
        {
            Type = type,
            Subject = subject,
            Body = htmlBody,
            RecipientEmail = recipientEmail,
            RecipientUserId = recipientUserId,
            IsSent = false,
        };

        await _notificationRepo.AddAsync(notification);

        try
        {
            await _emailService.SendAsync(recipientEmail, subject, htmlBody);
            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to send {Type} notification to {Email}", type, recipientEmail);
        }

        _notificationRepo.Update(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}
