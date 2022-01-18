using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Common
{
    /// <summary>
    /// Represents the multiple result of an operation.
    /// </summary>
    public class MultipleKeyValueOperationResult
    {
        /// <summary>
        /// Gets or sets a message that is common for all results.
        /// </summary>
        public string GeneralResultMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether the multiple result of the operation is succeeded.
        /// </summary>
        public bool Succeeded => HasResults && !HasBadResults;

        /// <summary>
        /// Gets a value indicating whether the <see cref="Results"/> isn't empty.
        /// </summary>
        public bool HasResults => Results.Any();

        /// <summary>
        /// Gets a value indicating whether the <see cref="Results"/> has bad operation results.
        /// </summary>
        public bool HasBadResults => Results.Any(x => !x.Value.Succeeded);

        /// <summary>
        /// Gets a dictionary of operation results.
        /// </summary>
        public IDictionary<short, OperationResult> Results { get; } = new Dictionary<short, OperationResult>();
    }
}
