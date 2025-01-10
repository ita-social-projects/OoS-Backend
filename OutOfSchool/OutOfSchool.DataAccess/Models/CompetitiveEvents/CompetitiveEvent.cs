using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEvent : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(250)]
    [MinLength(1)]
    public string Title { get; set; }

    [Required(ErrorMessage = "ShortTitle is required")]
    [DataType(DataType.Text)]
    [MaxLength(100)]
    [MinLength(1)]
    public string ShortTitle { get; set; }

    [Required]
    public CompetitiveEventStates State { get; set; } = CompetitiveEventStates.Draft;

    public DateTimeOffset RegistrationStartTime { get; set; }

    public DateTimeOffset RegistrationEndTime { get; set; }

    public Guid? ParentId { get; set; } = null; // ?

    [ForeignKey(nameof(ParentId))]
    public virtual CompetitiveEvent Parent { get; set; }

    public Guid? BuildingHoldingId { get; set; }

    //[ForeignKey(nameof(BuildingHoldingId))]
    //public Building BuildingHolding { get; set; }

    public Guid? ChildParticipantId { get; set; }

    //[ForeignKey(nameof(ChildParticipantId))]
    //public virtual Individual ChildParticipant { get; set; }

    //public Guid ChiefJudgeId { get; set; }

    //[ForeignKey(nameof(ChiefJudgeId))]
    //public virtual Individual ChiefJudgeId { get; set; }

    public virtual ICollection<CompetitiveEventDescriptionItem> CompetitiveEventDescriptionItems { get; set; }

    [MaxLength(2000)]
    public string AdditionalDescription { get; set; }

    [Required]
    public DateTimeOffset ScheduledStartTime { get; set; }

    [Required]
    public DateTimeOffset ScheduledEndTime { get; set; }

    [Required]
    public uint NumberOfSeats { get; set; } = uint.MaxValue;

    [Required]
    public int CompetitiveEventAccountingTypeId { get; set;}

    public virtual CompetitiveEventAccountingType CompetitiveEventAccountingType { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    [MaxLength(2000)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public Guid? OrganizerOfTheEventId { get; set; }

    [ForeignKey(nameof(OrganizerOfTheEventId))]
    public virtual Provider OrganizerOfTheEvent { get; set; }

    public FormOfLearning PlannedFormatOfClasses { get; set; }

    public Guid? VenueId { get; set; }

    [MaxLength(200)]
    public string VenueName { get; set; }
    //[ForeignKey(nameof(VenueId))]
    //public virtual Premises Venue { get; set; }

    [MaxLength(2000)]
    public string PreferentialTermsOfParticipation { get; set; }

    public virtual ICollection<Judge> Judges { get; set; }

    public virtual ICollection<Provider> ParticipantsOfTheEvent { get; set; }
    
    public bool AreThereBenefits { get; set; }

    [MaxLength(2000)]
    public string Benefits {  get; set; }

    public uint Rating { get; set; }

    public uint NumberOfRatings { get; set; }

    public bool OptionsForPeopleWithDisabilities { get; set; }

    [MaxLength(2000)]
    public string DescriptionOfOptionsForPeopleWithDisabilities { get; set; }

    public long? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public virtual Direction Category { get; set; }

    [MaxLength(250)]
    public string Subcategory { get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaximumAge { get; set; }

    public virtual ICollection<CompetitiveEventCoverage> Coverage { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int Price { get; set; } = default;

    public bool CompetitiveSelection { get; set; }

    public uint NumberOfOccupiedSeats { get; set; }
}
