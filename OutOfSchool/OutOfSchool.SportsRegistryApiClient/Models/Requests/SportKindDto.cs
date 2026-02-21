using OutOfSchool.Common.Models;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;
public class SportKindDto
{
    public Guid Id { get; set; }
    public long IdCode { get; set; }
    public string Name { get; set; }
    public string SportKindSectionName { get; set; }
    public string SportKindSectionNumeral { get; set; }
    public long SportKindSectionIdCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SportKindListResponse : IResponse
{
    public List<SportKindDto> Content { get; set; }
    public int TotalPages { get; set; }
    public int PageNo { get; set; }
}
