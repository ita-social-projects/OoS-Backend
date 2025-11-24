using OutOfSchool.AikomApiClient.Models.Contract;

namespace OutOfSchool.AikomApiClient.Models.Requests;

internal class GetUserRequest(string rnokpp) : ApiRequest<GetUserRequestData>(
    BusinessProcessKeys.GetUser,
    new()
    {
        Request = new GetUserRequestData
        {
            Rnokpp = rnokpp
        }
    })
{
}

internal class GetUserRequestData
{
    public required string Rnokpp { get; set; }
}
