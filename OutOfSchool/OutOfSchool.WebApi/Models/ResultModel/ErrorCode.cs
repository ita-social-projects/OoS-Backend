namespace OutOfSchool.WebApi.Models.ResultModel
{
    public enum ErrorCode
    {
        ValidationError,
        Unauthorized,
        InternalServerError,
        NotFound,
        UnprocessableEntity,
        Conflict,
        ForgotPasswordExpired
    }
}