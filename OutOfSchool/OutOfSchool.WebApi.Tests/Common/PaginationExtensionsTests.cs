using NUnit.Framework;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Tests.Common;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class PaginationExtensionsTests
{
    [Test]
    public async Task Paginate_ReturnsCorrectSubset()
    {
        // Arrange
        var data = Enumerable.Range(1, 10).ToList();
        var query = data.AsTestAsyncEnumerableQuery();
        int page = 2, pageSize = 3;

        // Act
        var result = await query.ToPaginatedResultAsync(page, pageSize);

        // Assert
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, result.Items);
        Assert.AreEqual(10, result.TotalCount);
        Assert.AreEqual(4, result.TotalPages);
        Assert.AreEqual(2, result.Page);
        Assert.AreEqual(3, result.PageSize);
        Assert.IsTrue(result.HasPreviousPage);
        Assert.IsTrue(result.HasNextPage);
    }

    [Test]
    public async Task Paginate_EmptySource_ReturnsEmptyResult()
    {
        // Arrange
        var query = Enumerable.Empty<int>().AsTestAsyncEnumerableQuery();

        // Act
        var result = await query.ToPaginatedResultAsync(1, 5);

        // Assert
        Assert.IsEmpty(result.Items);
        Assert.AreEqual(0, result.TotalCount);
        Assert.AreEqual(1, result.TotalPages);
        Assert.AreEqual(1, result.Page);
        Assert.IsFalse(result.HasPreviousPage);
        Assert.IsFalse(result.HasNextPage);
    }

    [Test]
    public async Task Paginate_PageIsZero_ClampsToFirstPage()
    {
        // Arrange
        var query = Enumerable.Range(1, 5).AsTestAsyncEnumerableQuery();

        // Act
        var result = await query.ToPaginatedResultAsync(0, 2);

        // Assert
        Assert.AreEqual(1, result.Page);
        CollectionAssert.AreEqual(new[] { 1, 2 }, result.Items);
    }

    [Test]
    public async Task Paginate_PageExceedsTotalPages_ClampsToLastPage()
    {
        // Arrange
        var query = Enumerable.Range(1, 5).AsTestAsyncEnumerableQuery();

        // Act
        var result = await query.ToPaginatedResultAsync(10, 2);

        // Assert
        Assert.AreEqual(3, result.Page);
        CollectionAssert.AreEqual(new[] { 5 }, result.Items);
    }
}
