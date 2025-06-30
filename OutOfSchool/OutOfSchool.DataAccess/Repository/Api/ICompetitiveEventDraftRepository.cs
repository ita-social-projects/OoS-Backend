using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.Base.Api;
using System;

namespace OutOfSchool.Services.Repository.Api;
public interface ICompetitiveEventDraftRepository : IEntityRepository<Guid, CompetitiveEventDraft>
{
}
