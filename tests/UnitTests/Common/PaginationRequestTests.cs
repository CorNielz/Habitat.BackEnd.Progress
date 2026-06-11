using Habitat.BackEnd.Progress.Application.Common;

namespace UnitTests.Common;

public sealed class PaginationRequestTests
{
    [Fact]
    public void SafePage_NormalizesValuesBelowOne()
    {
        var pagination = new PaginationRequest(0, 20);
        Assert.Equal(1, pagination.SafePage);
    }

    [Fact]
    public void SafePageSize_CapsValuesAboveOneHundred()
    {
        var pagination = new PaginationRequest(1, 500);
        Assert.Equal(100, pagination.SafePageSize);
    }
}
