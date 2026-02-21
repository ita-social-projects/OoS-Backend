using System;
using System.Threading.Tasks;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Extensions;

/// <summary>
/// Provides asynchronous extension methods for the Either type.
/// </summary>
public static class EitherAsyncExtensions
{
    /// <summary>
    /// Asynchronously maps the Right value of an <see cref="Either{TL, TR}"/> wrapped in a <see cref="Task{TResult}"/>
    /// using a synchronous mapping function.
    /// </summary>
    /// <typeparam name="TL">The type of the Left value.</typeparam>
    /// <typeparam name="TR">The type of the Right value.</typeparam>
    /// <typeparam name="TResult">The type of the result after mapping.</typeparam>
    /// <param name="eitherTask">A task that represents the asynchronous operation, containing an <see cref="Either{TL, TR}"/>.</param>
    /// <param name="fn">A function to apply to the Right value.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing an <see cref="Either{TL, TResult}"/> with the mapped result.
    /// </returns>
    public static async Task<Either<TL, TResult>> MapAsync<TL, TR, TResult>(
        this Task<Either<TL, TR>> eitherTask,
        Func<TR, TResult> fn)
    {
        var result = await eitherTask.ConfigureAwait(false);
        return result.Map(fn);
    }

    /// <summary>
    /// Asynchronously maps the Right value of an <see cref="Either{TL, TR}"/> wrapped in a <see cref="Task{TResult}"/>
    /// using an asynchronous mapping function with an optional exception handler.
    /// </summary>
    /// <typeparam name="TL">The type of the Left value.</typeparam>
    /// <typeparam name="TR">The type of the Right value.</typeparam>
    /// <typeparam name="TResult">The type of the result after mapping.</typeparam>
    /// <param name="eitherTask">A task that represents the asynchronous operation, containing an <see cref="Either{TL, TR}"/>.</param>
    /// <param name="fn">An asynchronous function to apply to the Right value.</param>
    /// <param name="exceptionHandler">
    /// An optional function to handle exceptions that occur during the mapping,
    /// returning a Left value of type <typeparamref name="TL"/>.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing an <see cref="Either{TL, TResult}"/> with the mapped result
    /// or a Left value if an exception occurs.
    /// </returns>
    public static async Task<Either<TL, TResult>> MapAsync<TL, TR, TResult>(
        this Task<Either<TL, TR>> eitherTask,
        Func<TR, Task<TResult>> fn,
        Func<Exception, TL> exceptionHandler = null)
    {
        try
        {
            var either = await eitherTask.ConfigureAwait(false);
            return await either.MapAsync(fn);
        }
        catch (Exception ex) when (exceptionHandler != null)
        {
            // Implicit operator
            return exceptionHandler(ex);
        }
    }

    /// <summary>
    /// Asynchronously applies a function that returns an <see cref="Either{TL, TResult}"/> to the Right value
    /// of an <see cref="Either{TL, TR}"/> wrapped in a <see cref="Task{TResult}"/>.
    /// </summary>
    /// <typeparam name="TL">The type of the Left value.</typeparam>
    /// <typeparam name="TR">The type of the Right value.</typeparam>
    /// <typeparam name="TResult">The type of the result after applying the function.</typeparam>
    /// <param name="eitherTask">A task that represents the asynchronous operation, containing an <see cref="Either{TL, TR}"/>.</param>
    /// <param name="fn">A function to apply to the Right value that returns an <see cref="Either{TL, TResult}"/>.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing an <see cref="Either{TL, TResult}"/> resulting from the function.
    /// </returns>
    public static async Task<Either<TL, TResult>> FlatMapAsync<TL, TR, TResult>(
        this Task<Either<TL, TR>> eitherTask,
        Func<TR, Either<TL, TResult>> fn)
    {
        var result = await eitherTask.ConfigureAwait(false);
        return result.FlatMap(fn);
    }

    /// <summary>
    /// Asynchronously applies an asynchronous function that returns a <see cref="Task{TResult}"/> of an <see cref="Either{TL, TResult}"/>
    /// to the Right value of an <see cref="Either{TL, TR}"/> wrapped in a <see cref="Task{TResult}"/>
    /// with an optional exception handler.
    /// </summary>
    /// <typeparam name="TL">The type of the Left value.</typeparam>
    /// <typeparam name="TR">The type of the Right value.</typeparam>
    /// <typeparam name="TResult">The type of the result after applying the function.</typeparam>
    /// <param name="eitherTask">A task that represents the asynchronous operation, containing an <see cref="Either{TL, TR}"/>.</param>
    /// <param name="fn">An asynchronous function to apply to the Right value that returns a <see cref="Task{TResult}"/> of an <see cref="Either{TL, TResult}"/>.</param>
    /// <param name="exceptionHandler">
    /// An optional function to handle exceptions that occur during the mapping,
    /// returning a Left value of type <typeparamref name="TL"/>.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing an <see cref="Either{TL, TResult}"/> resulting from the function
    /// or a Left value if an exception occurs.
    /// </returns>
    public static async Task<Either<TL, TResult>> FlatMapAsync<TL, TR, TResult>(
        this Task<Either<TL, TR>> eitherTask,
        Func<TR, Task<Either<TL, TResult>>> fn,
        Func<Exception, TL> exceptionHandler = null)
    {
        try
        {
            var either = await eitherTask.ConfigureAwait(false);
            return await either.FlatMapAsync(fn);
        }
        catch (Exception ex) when (exceptionHandler != null)
        {
            // Implicit operator
            return exceptionHandler(ex);
        }
    }
}