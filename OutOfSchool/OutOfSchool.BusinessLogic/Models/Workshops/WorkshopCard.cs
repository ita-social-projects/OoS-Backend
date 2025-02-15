namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopCard : WorkshopBaseCard
{
    public Guid? InstitutionHierarchyId { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;
}
