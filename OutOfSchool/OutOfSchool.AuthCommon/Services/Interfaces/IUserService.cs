namespace OutOfSchool.AuthCommon.Services.Interfaces;
public interface IUserService
{
    Task<ResponseDto> DeleteUserById(string userId);
}
