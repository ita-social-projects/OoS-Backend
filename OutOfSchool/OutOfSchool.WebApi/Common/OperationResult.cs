using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.WebApi.Common
{
    /// <summary>
    /// Represents the result of an operation.
    /// </summary>
    public class OperationResult
    {
        private readonly List<OperationError> errors = new List<OperationError>();

        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="OperationError"/>s containing an errors
        /// that occurred during the operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="OperationError"/>s.</value>
        public IEnumerable<OperationError> Errors => errors;

        /// <summary>
        /// Returns an <see cref="OperationResult"/> indicating a successful operation.
        /// </summary>
        /// <returns>An <see cref="OperationResult"/> indicating a successful operation.</returns>
        public static OperationResult Success { get; } = new OperationResult { Succeeded = true };

        /// <summary>
        /// Creates an <see cref="OperationResult"/> indicating a failed operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="OperationError"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="OperationResult"/> indicating a failed operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static OperationResult Failed(params OperationError[] errors)
        {
            var result = new OperationResult { Succeeded = false };
            if (errors != null)
            {
                result.errors.AddRange(errors);
            }

            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="OperationResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="OperationResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned 
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                   "Succeeded" : $"Failed : {string.Join(",", Errors.Select(x => x.Code).ToList())}";
        }
    }
}
