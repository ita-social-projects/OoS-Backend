using System;
using System.Collections.Generic;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Services.Models.CompetitiveEventDrafts;

/// <summary>
///     Will be stored as nested objects in the JSON format of the competitive event draft.
///     This entity is specific to the draft and can be hard-deleted if the draft is removed.
/// </summary>
public class CompetitiveEventDraftContent : IHasContacts
{
    public string Title { get; set; }

    public string ShortTitle { get; set; }

    public DateTimeOffset RegistrationStartTime { get; set; }

    public DateTimeOffset RegistrationEndTime { get; set; }

    public Guid? ParentId { get; set; }

    public DateTimeOffset ScheduledStartTime { get; set; }

    public DateTimeOffset ScheduledEndTime { get; set; }

    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public Guid OrganizerOfTheEventId { get; set; }

    public FormOfLearning PlannedFormatOfClasses { get; set; }

    public string VenueName { get; set; }

    public string CompetitiveSelectionDescription { get; set; }

    public bool AreThereBenefits { get; set; }

    public string Benefits { get; set; }

    public int MinimumAge { get; set; }

    public int MaximumAge { get; set; }

    public decimal Price { get; set; } = default;

    public bool CompetitiveSelection { get; set; }

    public List<Contacts> Contacts { get; set; }

    public List<CompetitiveEventDescriptionItem> CompetitiveEventDescriptionItems { get; set; }
    
    public List<long> SubDirectionIds { get; set; }       
}
