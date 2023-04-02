using IdentityServer4.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;

namespace OutOfSchool.IdentityServer.Services;

public class InteractionService : IInteractionService
{
    private readonly IIdentityServerInteractionService interactionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractionService"/> class.
    /// </summary>
    /// <param name="interactionService"> Identity Server 4 interaction service.</param>
    public InteractionService(IIdentityServerInteractionService interactionService)
    {
        this.interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
    }

    public Task<string> GetPostLogoutRedirectUri(string logoutId)
    {
        return interactionService.GetLogoutContextAsync(logoutId)
            .ContinueWith(
                (logoutRequest) => logoutRequest.Result.PostLogoutRedirectUri,
                TaskContinuationOptions.OnlyOnRanToCompletion);
    }
}