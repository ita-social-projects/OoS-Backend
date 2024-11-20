namespace OutOfSchool.AikomApiClient.Models;

public class GetUniversityRequest(int id) : ApiRequest(
    BusinessProcessKeys.GetUniversity,
    new StartVariables 
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