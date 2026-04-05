using FluentAssertions;
using Trisecmed.Application.Common;

namespace Trisecmed.Unit.Tests.Application;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_CalculatedCorrectly()
    {
        var result = new PagedResult<string>
        {
            Items = ["a", "b"],
            TotalCount = 50,
            Page = 1,
            PageSize = 25,
        };

        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void LastPage_HasNoNextPage()
    {
        var result = new PagedResult<string>
        {
            Items = ["a"],
            TotalCount = 50,
            Page = 2,
            PageSize = 25,
        };

        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }
}
