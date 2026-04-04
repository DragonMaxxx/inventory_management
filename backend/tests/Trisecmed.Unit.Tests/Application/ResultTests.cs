using Trisecmed.Application.Common;

namespace Trisecmed.Unit.Tests.Application;

public class ResultTests
{
    [Fact]
    public void Success_ShouldBeSuccessful()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ShouldContainError()
    {
        var result = Result.Failure("Coś poszło nie tak");
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Coś poszło nie tak", result.Error);
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        var result = Result.Success(42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFailure_ShouldHaveNullValue()
    {
        var result = Result.Failure<int>("Błąd");
        Assert.True(result.IsFailure);
        Assert.Equal("Błąd", result.Error);
    }
}
