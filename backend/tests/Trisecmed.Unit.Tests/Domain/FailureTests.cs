using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;

namespace Trisecmed.Unit.Tests.Domain;

public class FailureTests
{
    [Fact]
    public void NewFailure_ShouldHaveOpenStatus_ByDefault()
    {
        var failure = new Failure();
        Assert.Equal(FailureStatus.Open, failure.Status);
    }

    [Fact]
    public void NewFailure_ShouldHaveMediumPriority_ByDefault()
    {
        var failure = new Failure();
        Assert.Equal(FailurePriority.Medium, failure.Priority);
    }

    [Fact]
    public void FailureStatus_ShouldHaveAllValues()
    {
        var statuses = Enum.GetValues<FailureStatus>();
        Assert.Equal(5, statuses.Length);
    }

    [Fact]
    public void FailurePriority_ShouldHaveAllValues()
    {
        var priorities = Enum.GetValues<FailurePriority>();
        Assert.Equal(4, priorities.Length);
    }
}
