using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OutOfSchool.WebApi.Common;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <typeparam name="T">Used to make the result concrete.</typeparam>
public class Result<T>
{
    private T value;

    /// <summary>
    /// Contains a value for a result. It must be checked with <see cref="Succeeded"/> field before.
    /// </summary>
    /// <returns>If <see cref="Succeeded"/> is true, it returns the value, else an exception <see cref="InvalidOperationException"/>.</returns>
    public T Value
    {
        get
        {
            if (!OperationResult.Succeeded)
            {
                throw new InvalidOperationException($"Cannot get value because of failed {nameof(OperationResult)}");
            }

            return value;
        }
        private set => this.value = value;
    }

    /// <summary>
    /// Represents the result of an operation. See <see cref="Common.OperationResult"/>.
    /// </summary>
    public OperationResult OperationResult { get; private set; } = new OperationResult();

    /// <summary>
    /// Flag indicating whether if the operation succeeded or not.
    /// </summary>
    /// <value>True if the operation succeeded, otherwise false.</value>
    public bool Succeeded => OperationResult.Succeeded;

    /// <summary>
    /// Set result as success with a given value.
    /// </summary>
    /// <param name="value">The value which must be returned with the success result from an operation.</param>
    /// <returns>The <see cref="Result{T}"/> value, marked as Success.</returns>
    public static Result<T> Success(T value) => new Result<T> { Value = value, OperationResult = OperationResult.Success };

    /// <summary>
    /// Set result as failed with a given value.
    /// </summary>
    /// <param name="errors">Contains all errors that must be returned with the failed result from an operation.</param>
    /// <returns>The <see cref="Result{T}"/> value, marked as Failed.</returns>
    public static Result<T> Failed(params OperationError[] errors) => new Result<T> { OperationResult = OperationResult.Failed(errors) };
}