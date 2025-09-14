using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public bool IsDeleted { get; set; }
    public uint Rating { get; set; } = 0;
    public uint NumberOfRatings { get; set; } = 0;

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }
    public CompetitiveEventCoverageDto Coverage { get; set; }
    public IList<DirectionSubDirectionIdsDto> DirectionSubDirectionIds { get; set; } = [];
}

public static class CompetitiveEventDtoExtensions
{
    public static CompetitiveEventDto ToDto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.IsDeleted,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            State = model.State,
            RegistrationStartTime = model.RegistrationStartTime,
            RegistrationEndTime = model.RegistrationEndTime,
            ParentId = model.ParentId,
            CoverageId = model.CoverageId,
            CompetitiveEventDescriptionItems = model.CompetitiveEventDescriptionItems?.ToDto(),
            AdditionalDescription = model.AdditionalDescription,
            ScheduledStartTime = model.ScheduledStartTime,
            ScheduledEndTime = model.ScheduledEndTime,
            NumberOfSeats = model.NumberOfSeats,
            CompetitiveEventAccountingTypeId = model.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = model.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = model.OrganizerOfTheEventId,
            PlannedFormatOfClasses = model.PlannedFormatOfClasses,
            VenueName = model.VenueName,
            TermsOfParticipation = model.TermsOfParticipation,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            AreThereBenefits = model.AreThereBenefits,
            Benefits = model.Benefits,
            MinimumAge = model.MinimumAge,
            MaximumAge = model.MaximumAge,
            Price = model.Price,
            CompetitiveSelection = model.CompetitiveSelection,
            Contacts = model.Contacts?.ToDto(),
            SubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(s => s.Id).ToList() ?? [],
            Coverage = model.Coverage?.ToDto(),
            DirectionSubDirectionIds = model.SubDirections?.Where(s => !s.IsDeleted).Select(
                s => new DirectionSubDirectionIdsDto
                {
                    DirectionId = s.DirectionId,
                    SubDirectionId = s.Id
                })
            .ToList() ?? [],
            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(i => i.ExternalStorageId).ToList(),
        };

    public static List<CompetitiveEventDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToDto);
}