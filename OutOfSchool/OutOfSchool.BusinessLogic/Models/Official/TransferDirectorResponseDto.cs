namespace OutOfSchool.BusinessLogic.Models.Official;
public class TransferDirectorResponseDto
{
    public Guid NewDirectorOfficialId { get; set; }
    //public Guid NewDirectorIndividualId { get; set; }
    public string NewDirectorFullName { get; set; }

    public Guid PreviousDirectorOfficialId { get; set; }
    //public Guid PreviousDirectorIndividualId { get; set; }
    public string PreviousDirectorFullName { get; set; }

    //public DateOnly TransferredAt { get; set; }
    public Guid ProviderId { get; set; }
}
