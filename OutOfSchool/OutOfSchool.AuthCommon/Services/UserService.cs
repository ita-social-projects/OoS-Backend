namespace OutOfSchool.AuthCommon.Services;
public class UserService : IUserService
{
    private readonly UserManager<User> userManager;
    private readonly SignInManager<User> signInManager;
    //private readonly IAuthenticationManager authenticationManager; це якийсь наче старий менеджер

    public UserService(UserManager<User> userManager, SignInManager<User> signInManager/*, AuthenticationManager authenticationManager*/)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        //this.authenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
    }

    public async Task<ResponseDto> DeleteUserById(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        var result = await userManager.DeleteAsync(user); // цього достатньо, чи робити як у себе?
        await signInManager.SignOutAsync(); // це карент юзер
        //authenticationManager.

        if (!result.Succeeded)
        {
            // error
        }

        return new ResponseDto()
        {
            HttpStatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = result,
            Message = "Success",
        };
    }
}
