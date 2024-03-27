using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Resources.Codes;

namespace OutOfSchool.WebApi.Extensions;

/// <summary>
/// Extensions for app service operations.
/// </summary>
public static class OperationExtensions
{
    /// <summary>
    /// Returns the <see cref="OperationError"/> localized for the specified culture by the <see cref="ImagesOperationErrorCode"/> code.
    /// </summary>
    /// <param name="code">The <see cref="ImagesOperationErrorCode"/> code.</param>
    /// <returns>The <see cref="OperationError"/> localized for the specified culture.</returns>
    public static OperationError GetOperationError(this ImagesOperationErrorCode code)
    {
        return CreateOperationError(code.ToString(), code.GetResourceValue());
    }

    /// <summary>
    /// Creates a new instance of <see cref="OperationError"/>.
    /// </summary>
    /// <param name="code">Code of the error.</param>
    /// <param name="description">Error's description.</param>
    /// <returns>The instance of <see cref="OperationError"/>.</returns>
    /// <exception cref="ArgumentException">When <c>code</c> is null.</exception>
    public static OperationError CreateOperationError(string code, string description)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException(@$"Code [{code}] cannot be null or empty.", nameof(code));
        }

        return new OperationError
        {
            Code = code,
            Description = description ?? string.Empty,
        };
    }
}