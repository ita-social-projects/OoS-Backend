using System;
using System.Collections.Generic;
using Nest;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.ElasticsearchData.Models;

// TODO: check Nested attribute
public class WorkshopES
{
    public const string TitleKeyword = "title.keyword";

    [Keyword]
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string CoverImageId { get; set; }

    public float Rating { get; set; }

    [Keyword]
    public Guid ProviderId { get; set; }

    public string ProviderTitle { get; set; }

    public ProviderStatus ProviderStatus { get; set; }

    public OwnershipType ProviderOwnership { get; set; }

    public string Description { get; set; } // Sum of all WorkshopDescriptionItems (SectionName + Description)

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public decimal Price { get; set; }

    public PayRateType PayRate { get; set; }

    public long AddressId { get; set; }

    public AddressES Address { get; set; }

    [Keyword]
    public Guid? InstitutionHierarchyId { get; set; }

    public string InstitutionHierarchy { get; set; }

    [Keyword]
    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public bool WithDisabilityOptions { get; set; }

    public string Keywords { get; set; }

    public List<long> DirectionIds { get; set; }

    [Nested]
    public List<DateTimeRangeES> DateTimeRanges { get; set; }

    public WorkshopStatus Status { get; set; }
    public bool IsBlocked { get; set; }
}