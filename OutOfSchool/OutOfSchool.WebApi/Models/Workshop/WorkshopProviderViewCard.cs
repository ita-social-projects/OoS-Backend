using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopProviderViewCard : WorkshopBaseCard
{
    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public uint TakenSeats { get; set; } = 0;

    public int AmountOfPendingApplications { get; set; }

    public WorkshopStatus Status { get; set; }
}