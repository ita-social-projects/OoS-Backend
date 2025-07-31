using System;

namespace OutOfSchool.Services.Common.Exceptions;
public class InstitutionIdDifferenceException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionIdDifferenceException"/> class.
    /// </summary>
    public InstitutionIdDifferenceException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionIdDifferenceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">A message describing the InstitutionId difference error.</param>
    public InstitutionIdDifferenceException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstitutionIdDifferenceException"/> class with a specified 
    /// error message and an inner exception.
    /// </summary>
    /// <param name="message">A message describing the InstitutionId difference error.</param>
    /// <param name="innerException">The original exception that caused this conflict.</param>
    public InstitutionIdDifferenceException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
