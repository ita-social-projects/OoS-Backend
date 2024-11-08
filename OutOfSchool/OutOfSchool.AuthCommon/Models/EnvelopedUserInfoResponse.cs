using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Models;

public class EnvelopedUserInfoResponse : IResponse
{
    public string? EncryptedUserInfo { get; set; } = null;
}