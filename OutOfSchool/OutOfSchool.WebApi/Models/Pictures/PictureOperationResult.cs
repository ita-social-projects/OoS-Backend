using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models.Pictures
{
    public class PictureOperationResult
    {
        private List<PictureOperationError> _errors = new List<PictureOperationError>();

        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="PictureOperationError"/>s containing an errors
        /// that occurred during the picture operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="PictureOperationError"/>s.</value>
        public IEnumerable<PictureOperationError> Errors => _errors;

        /// <summary>
        /// Returns an <see cref="PictureOperationResult"/> indicating a successful picture operation.
        /// </summary>
        /// <returns>An <see cref="PictureOperationResult"/> indicating a successful operation.</returns>
        public static PictureOperationResult Success { get; } = new PictureOperationResult { Succeeded = true };

        /// <summary>
        /// Creates an <see cref="PictureOperationResult"/> indicating a failed picture operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="PictureOperationError"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="PictureOperationResult"/> indicating a failed picture operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static PictureOperationResult Failed(params PictureOperationError[] errors)
        {
            var result = new PictureOperationResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="PictureOperationResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="PictureOperationResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned 
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                   "Succeeded" : $"{"Failed"} : {string.Join(",", Errors.Select(x => x.Code).ToList())}";
        }
    }
}
