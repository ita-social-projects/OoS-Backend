using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.SportsRegistryApiClient.Models.Responses;
public class SportsSectionListResponse : IResponse
{
    public List<SportsSectionUpdateRequest> Content { get; set; }
    public int TotalPages { get; set; }
    public int PageNo { get; set; }
}
