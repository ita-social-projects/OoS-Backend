using OutOfSchool.AikomApiClient.Models.Contract;

namespace OutOfSchool.AikomApiClient.Models.Requests;

internal class SearchUniversityRequest(string edrpou) : ApiRequest<SearchUniversityRequestData>(
        BusinessProcessKeys.SearchUniversity,
        new ()
        {
            Request = new SearchUniversityRequestData
            { 
                Edrpou = edrpou
            }
        })
{
}

internal class SearchUniversityRequestData
{
    public required string Edrpou { get; set; }
}