namespace OutOfSchool.AikomApiClient.Models;

public class SearchUniversityRequest(string edrpou) 
    : ApiRequest(
        BusinessProcessKeys.SearchUniversity,
        new StartVariables
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