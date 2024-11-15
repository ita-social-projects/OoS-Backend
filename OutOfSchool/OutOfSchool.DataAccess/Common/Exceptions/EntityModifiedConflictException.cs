#nullable enable
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace OutOfSchool.Services.Common.Exceptions;

/// <summary>
/// Exception to handle entity modification conflicts, due to concurrent updates.
/// </summary>
public class EntityModifiedConflictException : Exception
{
    [NonSerialized]
    private readonly PropertyValues? propertyValues;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityModifiedConflictException" /> class.
    /// </summary>
    public EntityModifiedConflictException()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="EntityModifiedConflictException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EntityModifiedConflictException(string? message)
        : base(message)
    {
    }

    /// <summary>
    ///    Initializes a new instance of the <see cref="EntityModifiedConflictException" /> class with a specified
    /// error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EntityModifiedConflictException(
        string? message,
        Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///    Initializes a new instance of the <see cref="EntityModifiedConflictException" /> class with a specified
    /// error message, a reference to the inner exception, and the property values for an entity
    /// that was involved in the concurrency conflict.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="propertiesValues">Property values were involved in the conflict.</param>

    public EntityModifiedConflictException(
        string? message,
        Exception? innerException,
        PropertyValues? propertiesValues)
       : base(message, innerException)
    {
        this.propertyValues = propertiesValues;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityModifiedConflictException" /> class with a specified
    /// error message and property values associated with the conflict, defaulting the inner exception to null.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="propertiesValues">Property values were involved in the conflict.</param>
    public EntityModifiedConflictException(
        string? message,
        PropertyValues? propertiesValues)
        : this(message, null, propertiesValues)
    {
    }

    /// <summary>
    ///     Gets the values represents a snapshot of property values for an entity that were involved in the error.
    /// </summary>
    public virtual PropertyValues? PropertyValues
     => propertyValues;
}
