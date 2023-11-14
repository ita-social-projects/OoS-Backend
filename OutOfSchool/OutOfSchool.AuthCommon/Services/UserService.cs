using OpenIddict.Abstractions;

namespace OutOfSchool.AuthCommon.Services;
public class UserService : IUserService
{
    private readonly UserManager<User> userManager;
    private readonly ILogger<UserService> logger;
    private readonly IOpenIddictTokenManager tokenManager;

    public UserService(UserManager<User> userManager, ILogger<UserService> logger, IOpenIddictTokenManager tokenManager)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.logger = logger;
        this.tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    public async Task<ResponseDto> DeleteUserById(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            logger.LogError("User with id : {userId} not found", userId);
            return new ResponseDto() { HttpStatusCode = HttpStatusCode.NotFound };
        }

        var tokens = await tokenManager.FindBySubjectAsync(userId).ToListAsync();
        foreach (var token in tokens)
        {
            await tokenManager.TryRevokeAsync(token);
        }

        return new ResponseDto()
        {
            HttpStatusCode = HttpStatusCode.OK,
            IsSuccess = true,
        };
    }
}
