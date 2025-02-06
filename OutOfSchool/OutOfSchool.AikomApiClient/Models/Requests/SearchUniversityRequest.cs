using OutOfSchool.AikomApiClient.Models.Contract;

namespace OutOfSchool.AikomApiClient.Models.Requests;

public class SearchUniversityRequest(string edrpou) : ApiRequest<SearchUniversityRequestData>(
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

public class SearchUniversityRequestData
{
    public required string Edrpou { get; set; }
}