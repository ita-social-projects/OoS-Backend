using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using System;

namespace OutOfSchool.ElasticsearchData.Models;

public class CompetitiveEventES
{
    public const string SortSuffix = "sort";
    public const string TextSuffix = "text";

    public Guid Id { get; set; }

    public string Title { get; set; }

    public string ShortTitle { get; set; }

    public CompetitiveEventStates State { get; set; }

    public DateTimeOffset RegistrationStartTime { get; set; }

    public DateTimeOffset RegistrationEndTime { get; set; }

    public string CompetitiveEventDescriptionItems { get; set; } //sum of all competitive event descriptions

    public string AdditionalDescription { get; set; }

    public DateTimeOffset ScheduledStartTime { get; set; }

    public DateTimeOffset ScheduledEndTime { get; set; }

    public uint NumberOfSeats { get; set; }

    public int CompetitiveEventAccountingTypeId { get; set; }

    public string CompetitiveEventAccountingType { get; set; } //sum of all accounting type titles

    public string Description { get; set; }

    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public Guid? OrganizerOfTheEventId { get; set; }

    public FormOfLearning PlannedFormatOfClasses { get; set; }

    public string VenueName { get; set; }

    public string TermsOfParticipation { get; set; }

    public string PreferentialTermsOfParticipation { get; set; }

    public bool AreThereBenefits { get; set; }

    public string Benefits { get; set; }

    public bool OptionsForPeopleWithDisabilities { get; set; }

    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    public int MinimumAge { get; set; }

    public int MaximumAge { get; set; }

    public string Coverage { get; set; } //sum of all coverage titles

    public int Price { get; set; }

    public bool CompetitiveSelection { get; set; }

    public uint NumberOfOccupiedSeats { get; set; }
}
