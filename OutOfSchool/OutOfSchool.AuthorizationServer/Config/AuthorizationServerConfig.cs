namespace OutOfSchool.AuthorizationServer.Config;

public class AuthorizationServerConfig
{
    public const string Name = "AuthorizationServer";

    public string EncryptionKey { get; set; }

    public string SigningKey { get; set; }
}