using System;
using System.Collections.Generic;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;

namespace OutOfSchool.ElasticsearchData.Models;

 public class WorkshopES : IHasRating
{
    public const string KeywordSuffix = "keyword";
    public const string SortSuffix = "sort";
    public const string TextSuffix = "text";

    public Guid Id { get; set; }

    public string Title { get; set; }

    public string ShortTitle { get; set; }

    public string CoverImageId { get; set; }

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    public Guid ProviderId { get; set; }

    public string ProviderTitle { get; set; }

    public string ProviderTitleEn { get; set; }

    public ProviderStatus ProviderStatus { get; set; }

    public OwnershipType ProviderOwnership { get; set; }

    public string Description { get; set; } // Sum of all WorkshopDescriptionItems (SectionName + Description)

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public bool CompetitiveSelection { get; set; }

    public decimal Price { get; set; }

    public PayRateType PayRate { get; set; }

    public long AddressId { get; set; }

    public AddressES Address { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public bool WithDisabilityOptions { get; set; }

    public string Keywords { get; set; }

    public List<long> DirectionIds { get; set; }

    public List<DateTimeRangeES> DateTimeRanges { get; set; }

    public WorkshopStatus Status { get; set; }

    public bool IsBlocked { get; set; }

    public uint AvailableSeats { get; set; }

    public uint TakenSeats { get; set; }

    public ProviderLicenseStatus ProviderLicenseStatus { get; set; }

    public FormOfLearning FormOfLearning { get; set; }
}