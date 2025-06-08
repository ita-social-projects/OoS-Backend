using OutOfSchool.Common;
using OutOfSchool.Common.Enums.CompetitiveEvent;
using OutOfSchool.Services.Models.BaseEntities;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.CompetitiveEventDrafts;
public class CompetitiveEventDraft : TrackableBaseEntity,
    IImageDependentEntity<CompetitiveEventDraft>,
    IHasEntityImages<CompetitiveEventDraft>,
    IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid? CompetitiveEventId { get; set; }

    public Guid ProviderId { get; set; }

    public CompetitiveEventStates DraftStatus { get; set; }

    public string RejectionMessage { get; set; }

    public byte[] Version { get; set; }

    public string CoverImageId { get; set; }

    [MaxLength(Constants.CompetitiveEventDraftMaxRejectionMessageLength)]
    public CompetitiveEventDraftContent CompetitiveEventDraftContent { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }

    public virtual List<Image<CompetitiveEventDraft>> Images { get; set; }
}
