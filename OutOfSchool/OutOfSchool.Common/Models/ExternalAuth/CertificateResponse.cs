namespace OutOfSchool.Common.Models.ExternalAuth;

public class CertificateResponse : IResponse
{
    public string CertBase64 { get; set; }
}