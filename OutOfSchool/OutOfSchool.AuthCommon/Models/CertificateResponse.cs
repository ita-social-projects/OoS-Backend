using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Models;

public class CertificateResponse : IResponse
{
    public string CertBase64 { get; set; }
}