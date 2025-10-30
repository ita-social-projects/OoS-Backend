using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Services.Images;

public sealed class ImageReferenceService<TEntity>(OutOfSchoolDbContext db) : IImageReferenceService<TEntity>
    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
{
    private readonly OutOfSchoolDbContext db = db ?? throw new ArgumentNullException(nameof(db));
    private readonly Type type = typeof(TEntity);

    public async Task<int> CountReferencesAsync(string externalStorageId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(externalStorageId)) return 0;
        var lists = await CountInternalAsync([externalStorageId], ct);
        return lists.TryGetValue(externalStorageId, out var n) ? n : 0;
    }

    public Task<IDictionary<string, int>> CountReferencesAsync(IEnumerable<string> externalStorageIds, CancellationToken ct = default)
        => CountInternalAsync(externalStorageIds, ct);

    private async Task<IDictionary<string, int>> CountInternalAsync(IEnumerable<string> externalStorageIds, CancellationToken ct)
    {
        var imageIds = (externalStorageIds ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal).ToList();
        var result = imageIds.ToDictionary(x => x, _ => 0, StringComparer.Ordinal);
        if (result.Count == 0) return result;

        // Workshop group
        if (type == typeof(Workshop) || type == typeof(WorkshopDraft))
        {
            var c1 = await db.Set<Workshop>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var c2 = await db.Set<WorkshopDraft>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var g1 = await db.Set<Image<Workshop>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var g2 = await db.Set<Image<WorkshopDraft>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);

            foreach (var l in new[] { c1, c2, g1, g2 })
                foreach (var row in l) 
                    if (!string.IsNullOrEmpty(row.Key))
                        result[row.Key] = result.GetValueOrDefault(row.Key) + row.Count;

            return result;
        }

        // CompetitiveEvent group
        {
            var c1 = await db.Set<CompetitiveEvent>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var c2 = await db.Set<CompetitiveEventDraft>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var g1 = await db.Set<Image<CompetitiveEvent>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var g2 = await db.Set<Image<CompetitiveEventDraft>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);

            foreach (var l in new[] { c1, c2, g1, g2 })
                foreach (var row in l)
                    if (!string.IsNullOrEmpty(row.Key))
                        result[row.Key] = result.GetValueOrDefault(row.Key) + row.Count;

            return result;
        }
    }
}


//public sealed class ImageReferenceService<TEntity>(OutOfSchoolDbContext db) : IImageReferenceService<TEntity>
//    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
//{
//    private readonly OutOfSchoolDbContext db = db ?? throw new ArgumentNullException(nameof(db));
//    private readonly Type type = typeof(TEntity);

//    public async Task<int> CountReferencesAsync(string externalStorageId, CancellationToken ct = default)
//    {
//        if (string.IsNullOrWhiteSpace(externalStorageId)) return 0;
//        var lists = await CountInternalAsync([externalStorageId], ct);
//        return lists.TryGetValue(externalStorageId, out var n) ? n : 0;
//    }

//    public Task<IDictionary<string, int>> CountReferencesAsync(IEnumerable<string> externalStorageIds, CancellationToken ct = default)
//        => CountInternalAsync(externalStorageIds, ct);

//    private async Task<IDictionary<string, int>> CountInternalAsync(IEnumerable<string> externalStorageIds, CancellationToken ct)
//    {
//        var imageIds = (externalStorageIds ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s))
//            .Distinct(StringComparer.Ordinal).ToList();
//        var result = imageIds.ToDictionary(x => x, _ => 0, StringComparer.Ordinal);
//        if (result.Count == 0) return result;

//        // Workshop group
//        if (type == typeof(Workshop) || type == typeof(WorkshopDraft))
//        {
//            var c1 = db.Set<Workshop>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
//                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var c2 = db.Set<WorkshopDraft>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
//                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var g1 = db.Set<Image<Workshop>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
//                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var g2 = db.Set<Image<WorkshopDraft>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
//                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);

//            await Task.WhenAll(c1, c2, g1, g2);
//            foreach (var l in new[] { c1.Result, c2.Result, g1.Result, g2.Result })
//                foreach (var row in l) if (!string.IsNullOrEmpty(row.Key)) result[row.Key] += row.Count;
//            return result;
//        }

//        // CompetitiveEvent group
//        {
//            var c1 = db.Set<CompetitiveEvent>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
//                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var c2 = db.Set<CompetitiveEventDraft>().AsNoTracking().Where(x => imageIds.Contains(x.CoverImageId))
//                .GroupBy(x => x.CoverImageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var g1 = db.Set<Image<CompetitiveEvent>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
//                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
//            var g2 = db.Set<Image<CompetitiveEventDraft>>().AsNoTracking().Where(i => imageIds.Contains(i.ExternalStorageId))
//                .GroupBy(i => i.ExternalStorageId).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);

//            await Task.WhenAll(c1, c2, g1, g2);
//            foreach (var l in new[] { c1.Result, c2.Result, g1.Result, g2.Result })
//                foreach (var row in l) if (!string.IsNullOrEmpty(row.Key)) result[row.Key] += row.Count;
//            return result;
//        }
//    }
//}

//public sealed class ImageReferenceService<TEntity>(IDbContextFactory<OutOfSchoolDbContext> contextFactory) : IImageReferenceService<TEntity>
//    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>, new()
//{
//    private readonly IDbContextFactory<OutOfSchoolDbContext> contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
//    private readonly Type type = typeof(TEntity);

//    public async Task<int> CountReferencesAsync(string externalStorageId, CancellationToken ct = default)
//    {
//        if (string.IsNullOrWhiteSpace(externalStorageId)) return 0;
//        var lists = await CountInternalAsync([externalStorageId], ct);
//        return lists.TryGetValue(externalStorageId, out var n) ? n : 0;
//    }

//    public Task<IDictionary<string, int>> CountReferencesAsync(IEnumerable<string> externalStorageIds, CancellationToken ct = default)
//        => CountInternalAsync(externalStorageIds, ct);

//    private async Task<IDictionary<string, int>> CountInternalAsync(IEnumerable<string> externalStorageIds, CancellationToken ct)
//    {
//        var imageIds = (externalStorageIds ?? Enumerable.Empty<string>()).Where(s => !string.IsNullOrWhiteSpace(s))
//            .Distinct(StringComparer.Ordinal).ToList();
//        var result = imageIds.ToDictionary(x => x, _ => 0, StringComparer.Ordinal);
//        if (result.Count == 0) return result;

//        // Workshop group
//        if (type == typeof(Workshop) || type == typeof(WorkshopDraft))
//        {
//            var c1 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<Workshop>()
//                    .AsNoTracking()
//                    .Where(x => imageIds.Contains(x.CoverImageId))
//                    .GroupBy(x => x.CoverImageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var c2 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<WorkshopDraft>()
//                    .AsNoTracking()
//                    .Where(x => imageIds.Contains(x.CoverImageId))
//                    .GroupBy(x => x.CoverImageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var g1 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<Image<Workshop>>()
//                    .AsNoTracking()
//                    .Where(i => imageIds
//                    .Contains(i.ExternalStorageId))
//                    .GroupBy(i => i.ExternalStorageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var g2 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<Image<WorkshopDraft>>()
//                    .AsNoTracking()
//                    .Where(i => imageIds
//                    .Contains(i.ExternalStorageId))
//                    .GroupBy(i => i.ExternalStorageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });


//            await Task.WhenAll(c1, c2, g1, g2);

//            foreach (var l in new[] { c1.Result, c2.Result, g1.Result, g2.Result })
//                foreach (var row in l)
//                    if (!string.IsNullOrEmpty(row.Key))
//                        result[row.Key] = result.GetValueOrDefault(row.Key) + row.Count;

//            return result;
//        }

//        // CompetitiveEvent group
//        {
//            var c1 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<CompetitiveEvent>()
//                    .AsNoTracking()
//                    .Where(x => imageIds.Contains(x.CoverImageId))
//                    .GroupBy(x => x.CoverImageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var c2 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<CompetitiveEventDraft>()
//                    .AsNoTracking()
//                    .Where(x => imageIds.Contains(x.CoverImageId))
//                    .GroupBy(x => x.CoverImageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var g1 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<Image<CompetitiveEvent>>()
//                    .AsNoTracking()
//                    .Where(i => imageIds
//                    .Contains(i.ExternalStorageId))
//                    .GroupBy(i => i.ExternalStorageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            var g2 = Task.Run(async () =>
//            {
//                await using var db = contextFactory.CreateDbContext();
//                return await db.Set<Image<CompetitiveEventDraft>>()
//                    .AsNoTracking()
//                    .Where(i => imageIds
//                    .Contains(i.ExternalStorageId))
//                    .GroupBy(i => i.ExternalStorageId)
//                    .Select(g => new { g.Key, Count = g.Count() })
//                    .ToListAsync(ct);
//            });

//            await Task.WhenAll(c1, c2, g1, g2);

//            foreach (var l in new[] { c1.Result, c2.Result, g1.Result, g2.Result })
//                foreach (var row in l)
//                    if (!string.IsNullOrEmpty(row.Key))
//                        result[row.Key] = result.GetValueOrDefault(row.Key) + row.Count;

//            return result;
//        }
//    }
//}
