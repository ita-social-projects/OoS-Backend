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

        if (fromOfficial?.Individual == null)
        {
            throw new ArgumentNullException(nameof(fromOfficial), "From official and its individual cannot be null");
        }

        if (toOfficial?.Individual == null)
        {
            throw new ArgumentNullException(nameof(toOfficial), "To official and its individual cannot be null");
        }
                   
        return new TransferDirectorResponseDto
        {
            ProviderId = providerId,
            PreviousDirectorIndividualId = fromOfficial.IndividualId,
            PreviousDirectorFullName = $"{fromOfficial.Individual.LastName?.Trim()} {fromOfficial.Individual.FirstName?.Trim()}".Trim(),
            NewDirectorIndividualId = toOfficial.IndividualId,
            NewDirectorFullName = $"{toOfficial.Individual.LastName?.Trim()} {toOfficial.Individual.FirstName?.Trim()}".Trim(),
            TransferredAt = toOfficial.ActiveFrom
        };
    }
}
