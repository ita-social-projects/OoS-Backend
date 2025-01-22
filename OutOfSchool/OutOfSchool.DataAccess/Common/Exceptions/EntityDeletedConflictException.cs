#nullable enable
using System;

namespace OutOfSchool.Services.Common.Exceptions;

/// <summary>
///    Exception when a concurrency conflict occurs during an attempt to delete an entity from the database.
/// </summary>
public class EntityDeletedConflictException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityDeletedConflictException" /> class.
    /// </summary>
    public EntityDeletedConflictException()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="EntityDeletedConflictException" /> class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">A message describing the concurrency conflict error.</param>
    public EntityDeletedConflictException(string? message)
        : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDeletedConflictException" /> class with a specified
    /// error message and an inner exception.
    /// </summary>
    /// <param name="message">A message describing the concurrency conflict error.</param>
    /// <param name="innerException">The original exception that caused this conflict.</param>
    public EntityDeletedConflictException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
