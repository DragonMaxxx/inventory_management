namespace Trisecmed.Application.Notifications;

public interface IFailureNotificationJob
{
    Task ExecuteAsync(Guid failureId);
}
