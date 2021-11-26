using System;
using System.Security.Claims;
using OutOfSchool.Common.PermissionsModule;

namespace OutOfSchool.Common.Extensions
{
    /// <summary>
    /// Extention methods for the <see cref="ClaimsPrincipal"/> class.
    /// </summary>
    public static class ClaimsPrincipalExtentions
    {
        /// <summary>
        /// Gets the property value from the <see cref="ClaimsPrincipal"/> by the identity resource claim type.
        /// </summary>
        /// <param name="principal"><see cref="ClaimsPrincipal"/> object.</param>
        /// <param name="identityResourceClaim">Identity resource claim type.</param>
        /// <returns>Property value if exist, otherwise <see langword="null"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// thrown when:
        /// <paramref name="principal"/> is null
        /// or
        /// <paramref name="identityResourceClaim"/> is null or empty.
        /// </exception>
        public static string GetUserPropertyByClaimType(this ClaimsPrincipal principal, string identityResourceClaim)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (string.IsNullOrEmpty(identityResourceClaim))
            {
                throw new ArgumentNullException(nameof(identityResourceClaim));
            }

            return principal.FindFirst(identityResourceClaim)?.Value;
        }
    }
}
