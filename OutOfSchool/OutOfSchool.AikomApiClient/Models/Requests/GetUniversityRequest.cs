using OutOfSchool.AikomApiClient.Models.Contract;

namespace OutOfSchool.AikomApiClient.Models.Requests;

internal class GetUniversityRequest(long id) : ApiRequest<GetUniversityRequestData>(
    BusinessProcessKeys.GetUniversity,
    new ()
    { 
        Request = new GetUniversityRequestData
        {
            Id = id
        }
    })
{
}

internal class GetUniversityRequestData
{
    public required long Id { get; set; }
}