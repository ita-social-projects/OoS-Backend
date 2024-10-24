using OutOfSchool.AuthCommon.Services.Interfaces;

namespace OutOfSchool.AuthorizationServer.Services;

public class InteractionService : IInteractionService
{
    public Task<string> GetPostLogoutRedirectUri(string logoutId)
    {
        return Task.FromResult(string.Empty);
    }
}