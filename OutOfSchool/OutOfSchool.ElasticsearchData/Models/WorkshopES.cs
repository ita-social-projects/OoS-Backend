using System;
using System.Collections.Generic;
using Nest;
using OutOfSchool.Common.Enums;

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

    public long DirectionId { get; set; }

    public string Direction { get; set; }

    public long DepartmentId { get; set; }

    public long ClassId { get; set; }

    public bool WithDisabilityOptions { get; set; }

    public string Keywords { get; set; }

    public List<DirectionES> Directions { get; set; }

    [Nested]
    public List<DateTimeRangeES> DateTimeRanges { get; set; }

    public List<TeacherES> Teachers { get; set; }

    public WorkshopStatus Status { get; set; }
}