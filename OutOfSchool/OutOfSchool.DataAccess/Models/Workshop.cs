using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Models;

public class Workshop : IKeyedEntity<Guid>, IImageDependentEntity<Workshop>
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(1)]
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.PhoneNumber)]
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(
        Constants.PhoneNumberRegexModel,
        ErrorMessage = Constants.PhoneErrorMessage)]
    [DisplayFormat(DataFormatString = Constants.PhoneNumberFormat)]
    [MaxLength(Constants.UnifiedPhoneLength)]
    public string Phone { get; set; } = string.Empty;

    [DataType(DataType.EmailAddress)]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Website { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Facebook { get; set; } = string.Empty;

    [DataType(DataType.Url)]
    [MaxLength(Constants.UnifiedUrlLength)]
    public string Instagram { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 100, ErrorMessage = "Min age should be a number from 0 to 100")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 100, ErrorMessage = "Max age should be a number from 0 to 100")]
    public int MaxAge { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal Price { get; set; } = default;

    public virtual ICollection<WorkshopDescriptionItem> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string ProviderTitle { get; set; } = string.Empty;

    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    [MaxLength(200)]
    public string Keywords { get; set; } = string.Empty;

    [Required]
    public PayRateType PayRate { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    [Required]
    public long AddressId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public WorkshopStatus Status { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public virtual Provider Provider { get; set; }

    public virtual InstitutionHierarchy InstitutionHierarchy { get; set; }

    public virtual Address Address { get; set; }

    public virtual List<ProviderAdmin> ProviderAdmins { get; set; }

    public virtual List<Teacher> Teachers { get; set; }

    public virtual List<Application> Applications { get; set; }

    public virtual List<DateTimeRange> DateTimeRanges { get; set; }

    // These properties are only for navigation EF Core.
    public virtual ICollection<ChatRoomWorkshop> ChatRooms { get; set; }

    public virtual List<Image<Workshop>> Images { get; set; }
}