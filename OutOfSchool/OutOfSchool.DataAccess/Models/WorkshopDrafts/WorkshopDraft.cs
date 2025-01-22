using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.BaseEntities;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
/// Model for storing drafts of workshops before moderation.
/// Сan be hard deleted from the database if needed.
/// </summary>
public class WorkshopDraft :
    TrackableBaseEntity,
    IImageDependentEntity<WorkshopDraft>,
    IHasEntityImages<WorkshopDraft>,
    IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid ProviderId { get; set; }

    public Guid? WorkshopId { get; set; }

    public string CoverImageId { get; set; }

    public WorkshopDraftStatus DraftStatus { get; set; }

    public byte[] Version { get; set; }

    [MaxLength(Constants.WorkshopDraftMaxRejectionMessageLength)]
    public string RejectionMessage { get; set; }

    // TODO: Ensure that the content matches the new workshop model after the contact model is added.
    public WorkshopDraftContent WorkshopDraftContent { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual Workshop Workshop { get; set; }

    public virtual List<TeacherDraft> Teachers { get; set; }

    public virtual List<Image<WorkshopDraft>> Images { get; set; }
}
