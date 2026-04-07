using Trisecmed.Domain.Entities;

namespace Trisecmed.Unit.Tests.Domain;

public class NotificationTests
{
    [Fact]
    public void NewNotification_ShouldNotBeSent()
    {
        var notification = new Notification();
        Assert.False(notification.IsSent);
        Assert.Null(notification.SentAt);
    }

    [Fact]
    public void NewNotification_ShouldHaveNoError()
    {
        var notification = new Notification();
        Assert.Null(notification.ErrorMessage);
    }
}
