namespace OutOfSchool.BusinessLogic.Models.Official;
public class TransferDirectorResponseDto
{
    public Guid NewDirectorIndividualId { get; set; }
    public string NewDirectorFullName { get; set; }

    public Guid PreviousDirectorIndividualId { get; set; }
    public string PreviousDirectorFullName { get; set; }

    public DateOnly TransferredAt { get; set; }
    public Guid ProviderId { get; set; }
}

public  static class TransferDirectorExtensions
{
    public static TransferDirectorResponseDto ToTransferDirectorDto(
        this OutOfSchool.Services.Models.Official fromOfficial,
        OutOfSchool.Services.Models.Official toOfficial,
        Guid providerId)
    {
        return new TransferDirectorResponseDto
        {
            ProviderId = providerId,
            PreviousDirectorIndividualId = fromOfficial.IndividualId,
            PreviousDirectorFullName = $"{fromOfficial.Individual.LastName} {fromOfficial.Individual.FirstName}",
            NewDirectorIndividualId = toOfficial.IndividualId,
            NewDirectorFullName = $"{toOfficial.Individual.LastName} {toOfficial.Individual.FirstName}",
            TransferredAt = toOfficial.ActiveFrom
        };
    }
}
