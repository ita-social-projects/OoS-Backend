using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.ContactInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.CompetitiveEventDrafts;
public class CompetitiveEventDraftContent : IHasContacts
{
    [Required]
    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string ShortTitle { get; set; }

    public DateTimeOffset RegistrationStartTime { get; set; }

    public DateTimeOffset RegistrationEndTime { get; set; }

    public Guid? ParentId { get; set; }

    [MaxLength(2000)]
    public string AdditionalDescription { get; set; }

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    [MaxLength(2000)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public Guid OrganizerOfTheEventId { get; set; }

    public FormOfLearning PlannedFormatOfClasses { get; set; }

    [MaxLength(200)]
    public string VenueName { get; set; }

    [MaxLength(2000)]
    public string TermsOfParticipation { get; set; }

    [MaxLength(2000)]
    public string PreferentialTermsOfParticipation { get; set; }

    public bool AreThereBenefits { get; set; }

    [MaxLength(2000)]
    public string Benefits { get; set; }

    public bool OptionsForPeopleWithDisabilities { get; set; }

    [MaxLength(2000)]
    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    [Range(0, 120)]
    public int MinimumAge { get; set; }

    [Range(0, 120)]
    public int MaximumAge { get; set; }

    [Range(0, 100000)]
    public int Price { get; set; } = default;

    public bool CompetitiveSelection { get; set; }

    public List<Contacts> Contacts { get; set; }
}
