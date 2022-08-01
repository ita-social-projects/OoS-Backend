using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Models;

public class WorkshopCard
{
    [Required]
    public Guid WorkshopId { get; set; }

    [Required]
    [MaxLength(60)]
    public string ProviderTitle { get; set; } = string.Empty;

    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    public float Rating { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(1)]
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public PayRateType PayRate { get; set; }

    public string CoverImageId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 18, ErrorMessage = "Min age should be a number from 0 to 18")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 18, ErrorMessage = "Max age should be a number from 0 to 18")]
    public int MaxAge { get; set; }

    [Required(ErrorMessage = "CompetitiveSelection is required")]
    public bool CompetitiveSelection { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 10000, ErrorMessage = "Field value should be in a range from 1 to 10 000")]
    public decimal Price { get; set; } = default;

    public Guid? InstitutionHierarchyId { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public List<long> DirectionsId { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public AddressDto Address { get; set; }

    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    public bool WithDisabilityOptions { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public uint TakenSeats { get; set; } = 0;
}