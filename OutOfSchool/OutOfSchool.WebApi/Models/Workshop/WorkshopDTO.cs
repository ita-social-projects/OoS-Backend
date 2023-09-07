using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopDTO : WorkshopBaseDTO
{
    public uint TakenSeats { get; set; } = 0;

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    [JsonIgnore]
    public bool IsBlocked { get; set; }

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus ProviderStatus { get; set; } = ProviderStatus.Pending;
}