namespace OutOfSchool.Common.Models.ExternalAuth;

public class UserInfoResponse : IResponse
{
    public string Issuer { get; set; }

    public string IssuerCn { get; set; }

    public string Serial { get; set; }

    public string Subject { get; set; }

    public string SubjectCn { get; set; }

    public string Locality { get; set; }

    public string State { get; set; }

    public string O { get; set; }

    public string Ou { get; set; }

    public string Title { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string GivenName { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public string Phone { get; set; }

    public string Dns { get; set; }

    public string EdrpouCode { get; set; }

    public string DrfoCode { get; set; }
    
    public string Unzr { get; set; }
}