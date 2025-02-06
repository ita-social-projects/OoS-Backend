using OutOfSchool.AikomApiClient.Models.Contract;

namespace OutOfSchool.AikomApiClient.Models.Requests;

public class GetUniversityRequest(int id) : ApiRequest<GetUniversityRequestData>(
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

public class GetUniversityRequestData
{
    public required int Id { get; set; }
}