using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using OutOfSchool.ExternalFileStore.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class AsyncEnumerableExtensionsTests
{
    [Test]
    public async Task BatchAsync_WithLargeCollection_ShouldReturnDataBatches()
    {
        var batches = new List<int[]>();
        
        await foreach (var batch in Enumerable.Range(0, 10).ToAsyncEnumerable().BatchAsync(4).ConfigureAwait(false))
        {
            batches.Add(batch);
        }
        
        batches.Should().BeEquivalentTo(new List<int[]>
        {
            new []{ 0, 1, 2, 3},
            new []{ 4, 5, 6, 7},
            new []{ 8, 9}
        });
    }
    
    [Test]
    public async Task BatchAsync_WithCancellation_ShouldCancelGracefully()
    {
        var batches = new List<int[]>();

        var cts = new CancellationTokenSource();
        
        await foreach (var batch in Enumerable.Range(0, 10).ToAsyncEnumerable().BatchAsync(4, cts.Token))
        {
            batches.Add(batch);
            
            await cts.CancelAsync();
        }
        
        batches.Should().BeEquivalentTo(new List<int[]>
        {
            new []{ 0, 1, 2, 3}
        });
    }
}