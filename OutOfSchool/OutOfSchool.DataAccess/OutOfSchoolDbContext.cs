using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Configurations;
using OutOfSchool.Services.Models.Configurations.Images;
using OutOfSchool.Services.Models.Configurations.WorkshopDraftConfig;
using OutOfSchool.Services.Models.Configurations.WorkshopDrafts;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.Services.Models.SubordinationStructure;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services;

public partial class OutOfSchoolDbContext : IdentityDbContext<User>, IDataProtectionKeyContext
{
    public OutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options)
        : base(options)
    {
    }

    public DbSet<Individual> Individuals { get; set; }

    public DbSet<Parent> Parents { get; set; }

    public DbSet<Provider> Providers { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<ChatRoomWorkshop> ChatRoomWorkshops { get; set; }

    public DbSet<ChatMessageWorkshop> ChatMessageWorkshops { get; set; }

    public DbSet<Child> Children { get; set; }

    public DbSet<Workshop> Workshops { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Direction> Directions { get; set; }

    public DbSet<SocialGroup> SocialGroups { get; set; }

    public DbSet<InstitutionStatus> InstitutionStatuses { get; set; }

    public DbSet<PermissionsForRole> PermissionsForRoles { get; set; }

    public DbSet<Address> Addresses { get; set; }

    public DbSet<Application> Applications { get; set; }

    public DbSet<Rating> Ratings { get; set; }

    public DbSet<Favorite> Favorites { get; set; }

    public DbSet<DateTimeRange> DateTimeRanges { get; set; }

    public DbSet<Image<Workshop>> WorkshopImages { get; set; }

    public DbSet<Image<Provider>> ProviderImages { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<CompanyInformation> CompanyInformation { get; set; }

    public DbSet<CompanyInformationItem> CompanyInformationItems { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<BlockedProviderParent> BlockedProviderParents { get; set; }

    public DbSet<Institution> Institutions { get; set; }

    public DbSet<InstitutionHierarchy> InstitutionHierarchies { get; set; }

    public DbSet<InstitutionFieldDescription> InstitutionFieldDescriptions { get; set; }

    public DbSet<InstitutionAdmin> InstitutionAdmins { get; set; }

    public DbSet<ElasticsearchSyncRecord> ElasticsearchSyncRecords { get; set; }

    public DbSet<ProviderSectionItem> ProviderSectionItems { get; set; }

    public DbSet<WorkshopDescriptionItem> WorkshopDescriptionItems { get; set; }

    public DbSet<ChangesLog> ChangesLog { get; set; }

    public DbSet<EmployeeChangesLog> EmployeeChangesLog { get; set; }

    public DbSet<CATOTTG> CATOTTGs { get; set; }

    public DbSet<AchievementType> AchievementTypes { get; set; }

    public DbSet<AchievementTeacher> AchievementTeachers { get; set; }

    public DbSet<Achievement> Achievements { get; set; }

    public DbSet<ProviderType> ProviderTypes { get; set; }

    public DbSet<StatisticReport> StatisticReports { get; set; }

    public DbSet<StatisticReportCSV> StatisticReportsCSV { get; set; }

    public DbSet<FileInDb> FilesInDb { get; set; }

    public DbSet<RegionAdmin> RegionAdmins { get; set; }

    public DbSet<AreaAdmin> AreaAdmins { get; set; }

    public DbSet<AverageRating> AverageRatings { get; set; }

    public DbSet<OperationWithObject> OperationsWithObjects { get; set; }

    public DbSet<QuartzJob> QuartzJobs { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<ParentBlockedByAdminLog> ParentBlockedByAdminLog { get; set; }

    public DbSet<CompetitiveEvent> CompetitiveEvents { get; set; }

    public DbSet<CompetitiveEventAccountingType> CompetitiveEventAccountingTypes { get; set; }

    public DbSet<CompetitiveEventCoverage> CompetitiveEventCoverages { get; set; }

    public DbSet<CompetitiveEventDescriptionItem> CompetitiveEventDescriptionItems { get; set; }

    public DbSet<CompetitiveEventRegistrationDeadline> CompetitiveEventRegistrationDeadlines { get; set; }

    public DbSet<Judge> Judges { get; set; }

    public DbSet<Official> Officials { get; set; }
    
    public DbSet<Position> Positions { get; set; }

    public DbSet<StudySubject> StudySubjects { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<WorkshopDraft> WorkshopDrafts { get; set; }

    public DbSet<Image<WorkshopDraft>> WorkshopDraftImages { get; set; }
    

    public async Task<int> CompleteAsync() => await this.SaveChangesAsync();

    public int Complete() => this.SaveChanges();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DateTimeRange>()
            .ToTable(dtr => dtr.HasCheckConstraint("CK_DateTimeRanges_EndTimeIsAfterStartTime", "EndTime >= StartTime"));

        builder.ApplyConfiguration(new AchievementConfiguration());
        builder.ApplyConfiguration(new AchievementTeacherConfiguration());
        builder.ApplyConfiguration(new AchievementTypeConfiguration());
        builder.ApplyConfiguration(new AddressConfiguration());
        builder.ApplyConfiguration(new ApplicationConfiguration());
        builder.ApplyConfiguration(new AreaAdminConfiguration());
        builder.ApplyConfiguration(new AverageRatingConfiguration());
        builder.ApplyConfiguration(new BlockedProviderParentConfiguration());
        builder.ApplyConfiguration(new ChatMessageWorkshopConfiguration());
        builder.ApplyConfiguration(new ChatRoomWorkshopConfiguration());
        builder.ApplyConfiguration(new ChildConfiguration());
        builder.ApplyConfiguration(new CodeficatorConfiguration());
        builder.ApplyConfiguration(new CompetitiveEventConfiguration());
        builder.ApplyConfiguration(new CompetitiveEventAccountingTypeConfiguration());
        builder.ApplyConfiguration(new CompetitiveEventCoverageConfiguration());
        builder.ApplyConfiguration(new CompetitiveEventDescriptionItemConfiguration());
        builder.ApplyConfiguration(new CompetitiveEventRegistrationDeadlineConfiguration());
        builder.ApplyConfiguration(new DateTimeRangeConfiguration());
        builder.ApplyConfiguration(new DirectionConfiguration());
        builder.ApplyConfiguration(new EmployeeConfiguration());
        builder.ApplyConfiguration(new EntityImagesConfiguration<Provider>());
        builder.ApplyConfiguration(new EntityImagesConfiguration<Workshop>());
        builder.ApplyConfiguration(new FavoriteConfiguration());
        builder.ApplyConfiguration(new IndividualConfiguration());
        builder.ApplyConfiguration(new InstitutionAdminConfiguration());
        builder.ApplyConfiguration(new InstitutionConfiguration());
        builder.ApplyConfiguration(new InstitutionFieldDescriptionConfiguration());
        builder.ApplyConfiguration(new InstitutionHierarchyConfiguration());
        builder.ApplyConfiguration(new InstitutionStatusConfiguration());
        builder.ApplyConfiguration(new LanguageConfiguration());
        builder.ApplyConfiguration(new NotificationConfiguration());
        builder.ApplyConfiguration(new OfficialConfiguration());
        builder.ApplyConfiguration(new OperationWithObjectConfiguration());
        builder.ApplyConfiguration(new ParentConfiguration());
        builder.ApplyConfiguration(new PositionConfiguration());
        builder.ApplyConfiguration(new ProviderConfiguration());
        builder.ApplyConfiguration(new ProviderSectionItemConfiguration());
        builder.ApplyConfiguration(new QuartzJobConfiguration());
        builder.ApplyConfiguration(new RatingConfiguration());
        builder.ApplyConfiguration(new RegionAdminConfiguration());
        builder.ApplyConfiguration(new SocialGroupConfiguration());
        builder.ApplyConfiguration(new StudySubjectConfiguration());
        builder.ApplyConfiguration(new TagConfiguration());
        builder.ApplyConfiguration(new TeacherConfiguration());
        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new WorkshopConfiguration());
        builder.ApplyConfiguration(new WorkshopDescriptionItemConfiguration());
        builder.ApplyConfiguration(new EntityImagesConfiguration<WorkshopDraft>());
        builder.ApplyConfiguration(new WorkshopDraftConfiguration());
        builder.ApplyConfiguration(new TeacherDraftConfiguration());

        builder.Seed();
        builder.UpdateIdentityTables();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .AddInterceptors(new CalculateGeoHashInterceptor());
    }
}
