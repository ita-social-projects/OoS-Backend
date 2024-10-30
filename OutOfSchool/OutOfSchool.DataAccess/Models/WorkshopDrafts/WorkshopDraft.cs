using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
/// Model for storing drafts of workshops before moderation.
/// Сan be hard deleted from the database if needed.
/// </summary>
public class WorkshopDraft : IKeyedEntity<long>, IImageDependentEntity<WorkshopDraft>, IHasEntityImages<WorkshopDraft>
{
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid ProviderId { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public string CoverImageId { get; set; }

    public WorkshopDraftContent WorkshopDraftContent { get; set; }

    public virtual Provider Provider { get; set; }

    public virtual ProviderAdmin ProviderEmployee { get; set; }

    public virtual List<Image<WorkshopDraft>> Images { get; set; }
}
