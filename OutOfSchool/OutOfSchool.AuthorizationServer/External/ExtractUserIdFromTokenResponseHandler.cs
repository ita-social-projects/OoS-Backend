using System.Security.Claims;
using OpenIddict.Client;
using OutOfSchool.AuthCommon;

namespace OutOfSchool.AuthorizationServer.External;

public class ExtractUserIdFromTokenResponseHandler : IOpenIddictClientHandler<OpenIddictClientEvents.ProcessAuthenticationContext>
{
    /// <summary>
    /// Gets the default descriptor definition assigned to this handler.
    /// </summary>
    public static OpenIddictClientHandlerDescriptor Descriptor { get; }
        = OpenIddictClientHandlerDescriptor.CreateBuilder<OpenIddictClientEvents.ProcessAuthenticationContext>()
            .UseSingletonHandler<ExtractUserIdFromTokenResponseHandler>()
            .SetOrder(OpenIddictClientHandlers.PopulateMergedPrincipal.Descriptor.Order + 500)
            .SetType(OpenIddictClientHandlerType.Custom)
            .Build();

    public ValueTask HandleAsync(OpenIddictClientEvents.ProcessAuthenticationContext context)
    {
        if (context.TokenResponse.TryGetParameter(AuthServerConstants.ClaimTypes.UserId, out var userId))
        {
            var principal = context.MergedPrincipal.Identity as ClaimsIdentity;
            principal.AddClaim(new Claim(AuthServerConstants.ExternalAuthUserIdKey, userId.Value.ToString()));
        }

        return default;
    }
}