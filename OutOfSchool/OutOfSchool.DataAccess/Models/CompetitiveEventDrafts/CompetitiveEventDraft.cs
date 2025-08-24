using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.BaseEntities;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;

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

    public int CoverageId { get; set; }
    
    public int CompetitiveEventAccountingTypeId { get; set; }

    public byte[] Version { get; set; }

    public string CoverImageId { get; set; }

    public CompetitiveEventDraftContent CompetitiveEventDraftContent { get; set; }
    
    /// <summary>
    /// This property is used for searching, as nested JSON queries are not supported by EF/Pomelo at the moment.
    /// Do not use this property for anything else.
    /// </summary>
    public long CATOTTGId { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }

    public virtual List<Image<CompetitiveEventDraft>> Images { get; set; }
}
