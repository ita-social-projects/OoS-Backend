using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.SubordinationStructure;

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

    private void ApplySoftDelete(ModelBuilder builder)
    {
        builder
            .ApplySoftDelete<User>()
            .ApplySoftDelete<Achievement>()
            .ApplySoftDelete<AchievementTeacher>()
            .ApplySoftDelete<AchievementType>()
            .ApplySoftDelete<Address>()
            .ApplySoftDelete<Application>()
            .ApplySoftDelete<BlockedProviderParent>()
            .ApplySoftDelete<ChatMessageWorkshop>()
            .ApplySoftDelete<ChatRoomWorkshop>()
            .ApplySoftDelete<Child>()
            .ApplySoftDelete<CATOTTG>()
            .ApplySoftDelete<CompanyInformation>()
            .ApplySoftDelete<CompanyInformationItem>()
            .ApplySoftDelete<DateTimeRange>()
            .ApplySoftDelete<Direction>()
            .ApplySoftDelete<Favorite>()
            //.ApplySoftDelete<Institution>()
            .ApplySoftDelete<InstitutionFieldDescription>()
            .ApplySoftDelete<InstitutionHierarchy>()
            .ApplySoftDelete<InstitutionStatus>()
            .ApplySoftDelete<Parent>()
            .ApplySoftDelete<Provider>()
            .ApplySoftDelete<ProviderAdmin>()
            .ApplySoftDelete<ProviderSectionItem>()
            .ApplySoftDelete<Rating>()
            .ApplySoftDelete<RegionAdmin>()
            .ApplySoftDelete<SocialGroup>()
            .ApplySoftDelete<Teacher>()
            .ApplySoftDelete<Workshop>()
            .ApplySoftDelete<WorkshopDescriptionItem>()
            .ApplySoftDelete<AverageRating>()
            .ApplySoftDelete<QuartzJob>();
    }
}
