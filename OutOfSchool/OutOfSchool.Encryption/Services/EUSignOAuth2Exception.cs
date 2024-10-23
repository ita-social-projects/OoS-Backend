namespace OutOfSchool.Encryption.Services;

public class EUSignOAuth2Exception : Exception
{
    public EUSignOAuth2Exception(
        string message)
        : base(message)
    {
    }

    public EUSignOAuth2Exception(
        string message,
        Exception inner)
        : base(message, inner)
    {
    }
}