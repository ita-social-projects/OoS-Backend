using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventCreateUpdateDto : CompetitiveEventBaseDto, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ScheduledEndTime <= ScheduledStartTime)
        {
            yield return new ValidationResult("Scheduled end time must be after start time.");
        }
        
        if (RegistrationEndTime <= RegistrationStartTime)
        {
            yield return new ValidationResult("Registration end time must be after start time.");
        }
    }
}

public static class CompetitiveEventCreateUpdateDtoExtensions
{
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent ToModel(this CompetitiveEventCreateUpdateDto dto)
        => new()
        {
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            State = dto.State,
            RegistrationStartTime = dto.RegistrationStartTime ?? default,
            RegistrationEndTime = dto.RegistrationEndTime ?? default,
            ParentId = dto.ParentId,
            BuildingHoldingId = dto.BuildingHoldingId,
            ChildParticipantId = dto.ChildParticipantId,
            CoverageId = dto.CoverageId,
            AdditionalDescription = dto.AdditionalDescription,
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            NumberOfSeats = dto.NumberOfSeats,
            CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = dto.OrganizerOfTheEventId,
            PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? default,
            VenueId = dto.VenueId,
            VenueName = dto.VenueName,
            TermsOfParticipation = dto.TermsOfParticipation,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            AreThereBenefits = dto.AreThereBenefits ?? false,
            Benefits = dto.Benefits,
            OptionsForPeopleWithDisabilities = dto.OptionsForPeopleWithDisabilities ?? false,
            DescriptionOfOptionsForPeopleWithDisabilities = dto.DescriptionOfOptionsForPeopleWithDisabilities,
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            MinimumAge = dto.MinimumAge,
            MaximumAge = dto.MaximumAge ?? 0,
            Price = dto.Price ?? 0,
            CompetitiveSelection = dto.CompetitiveSelection ?? false,
            NumberOfOccupiedSeats = dto.NumberOfOccupiedSeats ?? 0,
            Contacts = dto.Contacts?.ToModel(),
            CoverImageId = dto.CoverageId.ToString(),
        };

    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventCreateUpdateDto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
    {
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.State = dto.State;
        model.RegistrationStartTime = dto.RegistrationStartTime ?? model.RegistrationStartTime;
        model.RegistrationEndTime = dto.RegistrationEndTime ?? model.RegistrationEndTime;
        model.ParentId = dto.ParentId;
        model.BuildingHoldingId = dto.BuildingHoldingId;
        model.ChildParticipantId = dto.ChildParticipantId;
        model.CoverageId = dto.CoverageId;
        model.AdditionalDescription = dto.AdditionalDescription;
        model.ScheduledStartTime = dto.ScheduledStartTime;
        model.ScheduledEndTime = dto.ScheduledEndTime;
        model.NumberOfSeats = dto.NumberOfSeats;
        model.CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId;
        model.DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure;
        model.OrganizerOfTheEventId = dto.OrganizerOfTheEventId;
        model.PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? model.PlannedFormatOfClasses;
        model.VenueId = dto.VenueId;
        model.VenueName = dto.VenueName;
        model.TermsOfParticipation = dto.TermsOfParticipation;
        model.PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation;
        model.AreThereBenefits = dto.AreThereBenefits ?? model.AreThereBenefits;
        model.Benefits = dto.Benefits;
        model.OptionsForPeopleWithDisabilities = dto.OptionsForPeopleWithDisabilities ?? model.OptionsForPeopleWithDisabilities;
        model.DescriptionOfOptionsForPeopleWithDisabilities = dto.DescriptionOfOptionsForPeopleWithDisabilities;
        model.InstitutionHierarchyId = dto.InstitutionHierarchyId;
        model.MinimumAge = dto.MinimumAge;
        model.MaximumAge = dto.MaximumAge ?? model.MaximumAge;
        model.Price = dto.Price ?? model.Price;
        model.CompetitiveSelection = dto.CompetitiveSelection ?? model.CompetitiveSelection;
        model.NumberOfOccupiedSeats = dto.NumberOfOccupiedSeats ?? model.NumberOfOccupiedSeats;
        model.Contacts = dto.Contacts?.ToModel() ?? model.Contacts;
        model.CoverImageId = dto.CoverageId.ToString();

        return model;
    }
}