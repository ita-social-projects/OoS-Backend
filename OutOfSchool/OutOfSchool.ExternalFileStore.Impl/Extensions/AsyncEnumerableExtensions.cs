using System.Runtime.CompilerServices;

namespace OutOfSchool.ExternalFileStore.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Batches the elements of an async sequence into arrays of a specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The async sequence to batch.</param>
    /// <param name="batchSize">The size of each batch. Must be greater than 0.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>An async sequence of arrays, each containing at most <paramref name="batchSize"/> elements from the input sequence.</returns>
    /// <remarks>
    /// The last batch may contain fewer elements than the batch size if there are not enough elements in the source sequence.
    /// If the source sequence is empty, no batches will be produced.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="batchSize"/> is less than or equal to 0.</exception>
    public static async IAsyncEnumerable<T[]> BatchAsync<T>(
        this IAsyncEnumerable<T> source,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than 0.");

        var batch = new List<T>(batchSize);

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                yield return batch.ToArray();

                batch.Clear();
            }
        }

        if (batch.Count > 0)
            yield return batch.ToArray();
    }
}