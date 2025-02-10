using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.ExternalFileStore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Repository.Files;

/// <summary>
/// Represents an images sync repository.
/// </summary>
public class ObjectImagesSyncDataRepository : IObjectImagesSyncDataRepository
{
    private readonly DbSet<Workshop> workshopSet;
    private readonly DbSet<Teacher> teacherSet;
    private readonly DbSet<Provider> providerSet;
    private readonly DbSet<Image<Workshop>> workshopImagesSet;
    private readonly DbSet<Image<Provider>> providerImagesSet;

    public ObjectImagesSyncDataRepository(OutOfSchoolDbContext dbContext)
    {
        _ = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        workshopSet = dbContext.Set<Workshop>();
        teacherSet = dbContext.Set<Teacher>();
        providerSet = dbContext.Set<Provider>();
        workshopImagesSet = dbContext.Set<Image<Workshop>>();
        providerImagesSet = dbContext.Set<Image<Provider>>();
    }

    #region EntityCoverImages

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(workshopSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectTeacherCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(teacherSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectProviderCoverImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityCoverImagesIds(providerSet, searchIds).ConfigureAwait(false);

    #endregion

    #region EntityImages

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectWorkshopImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(workshopImagesSet, searchIds).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<List<string>> GetIntersectProviderImagesIds(IEnumerable<string> searchIds)
        => await GetIntersectEntityImagesIds(providerImagesSet, searchIds).ConfigureAwait(false);

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