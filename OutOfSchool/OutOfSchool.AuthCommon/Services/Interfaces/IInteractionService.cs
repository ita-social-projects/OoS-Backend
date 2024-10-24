namespace OutOfSchool.AuthCommon.Services.Interfaces;

public interface IInteractionService
{
    public Task<string> GetPostLogoutRedirectUri(string logoutId);
}