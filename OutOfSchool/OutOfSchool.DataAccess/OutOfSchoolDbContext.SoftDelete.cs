using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services;

public partial class OutOfSchoolDbContext
{
    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public Task<int> HardDeleteSaveChangesAsync()
    {
        return base.SaveChangesAsync(true, default);
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted))
        {
            if (entry.OriginalValues.Properties.Any(p => p.Name == "IsDeleted"))
            {
                entry.State = EntityState.Modified;
                entry.CurrentValues["IsDeleted"] = true;
            }
        }
    }
}
