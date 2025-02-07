using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopProviderViewCard : WorkshopBaseCard
{
    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public int AmountOfPendingApplications { get; set; }

    public WorkshopStatus Status { get; set; }
    
    public int UnreadMessages { get; set; }
}