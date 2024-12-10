namespace OutOfSchool.Common.Models.ExternalAuth;

public class EnvelopedUserInfoResponse : IResponse
{
    public string? EncryptedUserInfo { get; set; } = null;
}