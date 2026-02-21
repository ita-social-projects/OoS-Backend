using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Repository.Files;

/// <summary>
/// Represents an images sync repository.
/// </summary>
public class ObjectImagesSyncDataRepository : IObjectImagesSyncDataRepository
{
    private readonly DbSet<Workshop> workshopSet;
    private readonly DbSet<WorkshopDraft> workshopDraftSet;
    private readonly DbSet<Teacher> teacherSet;
    private readonly DbSet<Provider> providerSet;
    private readonly DbSet<CompetitiveEvent> competitiveEventSet;
    private readonly DbSet<CompetitiveEventDraft> competitiveEventDraftSet;
    private readonly DbSet<Image<Workshop>> workshopImagesSet;
    private readonly DbSet<Image<WorkshopDraft>> workshopDraftImagesSet;
    private readonly DbSet<Image<Provider>> providerImagesSet;
    private readonly DbSet<Image<CompetitiveEvent>> competitiveEventImagesSet;
    private readonly DbSet<Image<CompetitiveEventDraft>> competitiveEventDraftImagesSet;

    public ObjectImagesSyncDataRepository(OutOfSchoolDbContext dbContext)
    {
        _ = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        workshopSet = dbContext.Set<Workshop>();
        workshopDraftSet = dbContext.Set<WorkshopDraft>();
        teacherSet = dbContext.Set<Teacher>();
        providerSet = dbContext.Set<Provider>();
        competitiveEventSet = dbContext.Set<CompetitiveEvent>();
        competitiveEventDraftSet = dbContext.Set<CompetitiveEventDraft>();
        workshopImagesSet = dbContext.Set<Image<Workshop>>();
        workshopDraftImagesSet = dbContext.Set<Image<WorkshopDraft>>();
        providerImagesSet = dbContext.Set<Image<Provider>>();
        competitiveEventImagesSet = dbContext.Set<Image<CompetitiveEvent>>();
        competitiveEventDraftImagesSet = dbContext.Set<Image<CompetitiveEventDraft>>();
    }

    #region EntityCoverImages

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(workshopSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopDraftCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(workshopDraftSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectTeacherCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(teacherSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectProviderCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(providerSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectCompetitiveEventCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(competitiveEventSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectCompetitiveEventDraftCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(competitiveEventDraftSet, searchIds).ConfigureAwait(false);

    #endregion

    #region EntityImages

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(workshopImagesSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopDraftImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(workshopDraftImagesSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectProviderImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(providerImagesSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectCompetitiveEventImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(competitiveEventImagesSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectCompetitiveEventDraftImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(competitiveEventDraftImagesSet, searchIds).ConfigureAwait(false);

    #endregion

    private Task<List<string>> GetIntersectEntityCoverImagesIds<TEntity>(
        IQueryable<TEntity> dbSet,
        IEnumerable<string> searchIds)
        where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
    {
        _ = searchIds ?? throw new ArgumentNullException(nameof(searchIds));

        return dbSet
            .Where(x => searchIds.Contains(x.CoverImageId))
            .Select(x => x.CoverImageId)
            .ToListAsync();
    }

    private Task<List<string>> GetIntersectEntityImagesIds<TEntity>(
        IQueryable<Image<TEntity>> dbSet,
        IEnumerable<string> searchIds)
        where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
    {
        _ = searchIds ?? throw new ArgumentNullException(nameof(searchIds));

        return dbSet
            .Select(x => x.ExternalStorageId)
            .Where(x => searchIds.Contains(x))
            .ToListAsync();
    }
}