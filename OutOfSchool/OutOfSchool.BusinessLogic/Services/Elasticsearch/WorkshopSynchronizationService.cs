using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class WorkshopSynchronizationService(
    IWorkshopService workshopService,
    IElasticsearchSyncRecordRepository elasticsearchSyncRecordRepository,
    IElasticsearchProvider<WorkshopES, WorkshopFilterES> esProvider,
    ILogger<WorkshopSynchronizationService> logger,
    IOptions<ElasticsearchSynchronizationSchedulerConfig> options,
    IAddNewRecordToESSynchronizationTableService addNewRecordToESSynchronizationTableService
) : ElasticsearchSynchronizationService<IWorkshopService, Workshop, WorkshopES, WorkshopFilterES>(
    workshopService, 
    elasticsearchSyncRecordRepository, 
    esProvider, 
    logger, 
    WorkshopESExtensions.ToES, 
    options, 
    addNewRecordToESSynchronizationTableService
)
{
    public override Func<IWorkshopService, List<Guid>, Task<IEnumerable<Workshop>>> GetbyIds 
        => (service, ids) => service.GetByIds(ids);
}

public static class WorkshopESExtensions
{
    private const double Epsilon = 0.1d;

    public static CodeficatorAddressES ToES(this CATOTTG catottg)
        => new()
        {
            // FullAddress - ignored in original AM mapping 
            Id = catottg.Id,
            Category = catottg.Category,
            ParentId = catottg.ParentId,
            Parent = catottg.Parent?.ToES(),
            Region = catottg.GetRegionName(),
            District = catottg.GetDistrictName(),
            TerritorialCommunity = catottg.GetTerritorialCommunityName(),
            Settlement = catottg.GetSettlementName(),
            CityDistrict = catottg.GetCityDistrictName(),
            Latitude = catottg.Latitude,
            Longitude = catottg.Longitude,
            Order = catottg.Order,
            // FullName - ignored in original AM mapping
        };

    public static AddressES ToES(this ContactsAddress contactsAddress)
        => new()
        {
            City = contactsAddress.CATOTTG.Name,
            Latitude = contactsAddress.Latitude,
            Longitude = contactsAddress.Longitude,
            CATOTTGId = contactsAddress.CATOTTGId,
            CodeficatorAddressES = contactsAddress.CATOTTG.ToES(),
            Street = contactsAddress.Street,
            BuildingNumber = contactsAddress.BuildingNumber,
            Point = GeoLocation.LatitudeLongitude(new LatLonGeoLocation()
            {
                Lat = Math.Abs(contactsAddress.Latitude - 0d) < Epsilon ? contactsAddress.CATOTTG.Latitude : contactsAddress.Latitude,
                Lon = Math.Abs(contactsAddress.Longitude - 0d) < Epsilon ? contactsAddress.CATOTTG.Longitude : contactsAddress.Longitude,
            }),
        };

    public static DateTimeRangeES ToES(this DateTimeRange range)
        => new()
        {
            Id = range.Id,
            StartTime = range.StartTime,
            EndTime = range.EndTime,
            WorkshopId = range.WorkshopId,
            Workdays = string.Join(" ", range.Workdays.ToDaysBitMaskEnumerable()),
        };

    public static List<DateTimeRangeES> ToES(this IEnumerable<DateTimeRange> list)
        => list.MapToList(ToES);

    public static WorkshopES ToES(this Workshop dto)
        => new()
        {
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            CoverImageId = dto.CoverImageId,
            ProviderId = dto.ProviderId,
            ProviderTitle = dto.ProviderTitle,
            ProviderTitleEn = dto.ProviderTitleEn,
            // ProviderStatus = dto.Provider.Status, - was absent in original mapping
            ProviderOwnership = dto.ProviderOwnership,
            Description = dto.WorkshopDescriptionItems.Where(x => !x.IsDeleted)
                .Aggregate(string.Empty, (accumulator, wdi) =>
                    $"{accumulator}{wdi.SectionName}{Constants.MappingSeparator}{wdi.Description}{Constants.MappingSeparator}"),
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            CompetitiveSelection = dto.CompetitiveSelection,
            Price = dto.Price,
            PayRate = dto.PayRate,
            Address = dto.Contacts?.FirstOrDefault(c => c.IsDefault)?.Address?.ToES(),
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            InstitutionHierarchy = dto.InstitutionHierarchy.Title,
            InstitutionId = dto.InstitutionHierarchy.InstitutionId,
            Institution = dto.InstitutionHierarchy.Institution.Title,
            Keywords = dto.Keywords,
            DirectionIds = dto.InstitutionHierarchy.SubDirections.Where(x => !x.Direction.IsDeleted).Select(d => d.DirectionId).ToList(),
            DateTimeRanges = dto.DateTimeRanges.ToES(),
            Status = dto.Status,
            IsBlocked = dto.IsBlocked,
            AvailableSeats = dto.AvailableSeats,
    
            // TODO: Copied this from base MappingProfile but this looks like some messed up lazy loading thing :)
            TakenSeats = (uint) dto.Applications.Count(x =>
                                !x.IsDeleted && (x.Status == ApplicationStatus.Approved
                                || x.Status == ApplicationStatus.StudyingForYears)),

            //ProviderLicenseStatus = dto.Provider.LicenseStatus, - was absent in original mapping
            FormOfLearning = dto.FormOfLearning,
            AgeComposition = dto.AgeComposition,
            EducationalShift = dto.EducationalShift,
            IsSelfFinanced = dto.IsSelfFinanced,
            IsPaid = dto.IsPaid,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            SpecialNeedsType = dto.SpecialNeedsType,
            IsInclusive = dto.IsInclusive,
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            Coverage = dto.Coverage,
            Tags = dto.Tags.Select(x => x.Name).ToList(),
            LanguageOfEducationId = dto.LanguageOfEducationId,
        };

    public static List<WorkshopES> ToES(this IEnumerable<Workshop> list)
        => list.MapToList(ToES);
}
