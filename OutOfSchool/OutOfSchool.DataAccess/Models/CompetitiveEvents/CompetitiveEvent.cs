using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.CompetitiveEvents;

public class CompetitiveEvent : BusinessEntity, IHasContacts, IImageDependentEntity<CompetitiveEvent>, IHasEntityImages<CompetitiveEvent>
{
    [Required(ErrorMessage = "Title is required")]
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
    
    [Required]
    public int CoverageId { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;


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
    public string Benefits {  get; set; }

    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinimumAge { get; set; }

    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaximumAge { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public int Price { get; set; } = default;

    public bool CompetitiveSelection { get; set; }

    // owned entities
    public List<Contacts> Contacts { get; set; } = [];

    //nav props
    public virtual List<Image<CompetitiveEvent>> Images { get; set; }
    public virtual ICollection<Judge> Judges { get; set; }
    public virtual CompetitiveEventCoverage Coverage { get; set; }

    public virtual Provider OrganizerOfTheEvent { get; set; }
    public virtual ICollection<CompetitiveEventDescriptionItem> CompetitiveEventDescriptionItems { get; set; }
    public virtual CompetitiveEventAccountingType CompetitiveEventAccountingType { get; set; }

    public virtual List<SubDirection> SubDirections { get; set; } = [];
}
