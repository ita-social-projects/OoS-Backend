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
        var imageIds = (externalStorageIds ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var result = imageIds.ToDictionary(x => x, _ => 0, StringComparer.Ordinal);
        if (result.Count == 0)
            return result;

        async Task<List<(string Key, int Count)>> CountAllAsync<TMain, TDraft>()
            where TMain : class, IKeyedEntity, IImageDependentEntity<TMain>
            where TDraft : class, IKeyedEntity, IImageDependentEntity<TDraft>
        {
            var c1 = await db.Set<TMain>().AsNoTracking()
                .Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var c2 = await db.Set<TDraft>().AsNoTracking()
                .Where(x => imageIds.Contains(x.CoverImageId))
                .GroupBy(x => x.CoverImageId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var g1 = await db.Set<Image<TMain>>().AsNoTracking()
                .Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var g2 = await db.Set<Image<TDraft>>().AsNoTracking()
                .Where(i => imageIds.Contains(i.ExternalStorageId))
                .GroupBy(i => i.ExternalStorageId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return new[] { c1, c2, g1, g2 }
                .SelectMany(l => l)
                .Where(r => !string.IsNullOrEmpty(r.Key))
                .GroupBy(r => r.Key, StringComparer.Ordinal)
                .Select(g => (g.Key, Count: g.Sum(x => x.Count)))
                .ToList();
        }

        List<(string Key, int Count)> counts;

        if (type == typeof(Workshop) || type == typeof(WorkshopDraft))
        {
            counts = await CountAllAsync<Workshop, WorkshopDraft>();
        }
        else
        {
            counts = await CountAllAsync<CompetitiveEvent, CompetitiveEventDraft>();
        }

        foreach (var (key, count) in counts)
            result[key] = result.GetValueOrDefault(key) + count;

        return result;
    }
}
