namespace OutOfSchool.Encryption.Models;

public class EnvelopedUserInfoResponse
{
    public string EncryptedUserInfo { get; set; }

    public int Error { get; set; }

    public string Message { get; set; }
}