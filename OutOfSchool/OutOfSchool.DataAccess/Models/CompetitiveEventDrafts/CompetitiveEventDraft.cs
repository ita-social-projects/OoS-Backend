using OutOfSchool.Common;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.BaseEntities;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.CompetitiveEventDrafts;

/// <summary>
/// Model for storing drafts of competitive events before moderation.
/// Сan be hard deleted from the database if needed.
/// </summary>
public class CompetitiveEventDraft : TrackableBaseEntity,
    IImageDependentEntity<CompetitiveEventDraft>,
    IHasEntityImages<CompetitiveEventDraft>,
    IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    
    public Guid? CompetitiveEventId { get; set; }

    public Guid ProviderId { get; set; }

    public CompetitiveEventDraftStatus DraftStatus { get; set; } = CompetitiveEventDraftStatus.Draft;

    [MaxLength(Constants.CompetitiveEventDraftMaxRejectionMessageLength)]
    public string RejectionMessage { get; set; }

    public byte[] Version { get; set; }

    public string CoverImageId { get; set; }

    public CompetitiveEventDraftContent CompetitiveEventDraftContent { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }

    public virtual List<Image<CompetitiveEventDraft>> Images { get; set; }
}
