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
    /// <summary>
        /// Converts a CompetitiveEventCreateUpdateDto into a new CompetitiveEvent domain model.
        /// </summary>
        /// <remarks>
        /// Maps DTO properties to a new OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent instance,
        /// applying sensible defaults where the DTO uses nullable properties:
        /// - RegistrationStartTime/RegistrationEndTime and PlannedFormatOfClasses default to their type default when null.
        /// - AreThereBenefits and CompetitiveSelection default to false when null.
        /// - MaximumAge and Price default to 0 when null.
        /// Contacts are converted via Contacts.ToModel(); SubDirectionIds are projected into SubDirection entries.
        /// Note: fields such as AdditionalDescription and PreferentialTermsOfParticipation are not mapped from the DTO.
        /// </remarks>
        /// <returns>A newly created CompetitiveEvent populated from the DTO.</returns>
        public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent ToModel(this CompetitiveEventCreateUpdateDto dto)
        => new()
        {
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            State = dto.State,
            RegistrationStartTime = dto.RegistrationStartTime ?? default,
            RegistrationEndTime = dto.RegistrationEndTime ?? default,
            ParentId = dto.ParentId,
            CoverageId = dto.CoverageId,
            ScheduledStartTime = dto.ScheduledStartTime,
            ScheduledEndTime = dto.ScheduledEndTime,
            NumberOfSeats = dto.NumberOfSeats,
            CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId,
            DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure,
            OrganizerOfTheEventId = dto.OrganizerOfTheEventId,
            PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? default,
            VenueName = dto.VenueName,
            TermsOfParticipation = dto.TermsOfParticipation,
            AreThereBenefits = dto.AreThereBenefits ?? false,
            Benefits = dto.Benefits,
            MinimumAge = dto.MinimumAge,
            MaximumAge = dto.MaximumAge ?? 0,
            Price = dto.Price ?? 0,
            CompetitiveSelection = dto.CompetitiveSelection ?? false,
            Contacts = dto.Contacts?.ToModel(),
            SubDirections = dto.SubDirectionIds.Select(id => new SubDirection { DirectionId = id }).ToList(),
            CoverImageId = dto.CoverageId.ToString(),
        };

    /// <summary>
    /// Copies values from a CompetitiveEventCreateUpdateDto into an existing CompetitiveEvent model instance and returns that instance.
    /// </summary>
    /// <param name="dto">Source DTO providing updated values.</param>
    /// <param name="model">Target model to be updated (mutated in-place and returned).</param>
    /// <returns>The same <see cref="OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent"/> instance passed in via <paramref name="model"/> after applying updates.</returns>
    /// <remarks>
    /// Fields that retain the model's existing value when the DTO value is null: RegistrationStartTime, RegistrationEndTime, PlannedFormatOfClasses, AreThereBenefits, MaximumAge, Price, CompetitiveSelection, and Contacts (Contacts is mapped via <c>dto.Contacts?.ToModel()</c>).
    /// SubDirections is replaced with a new list built from <c>dto.SubDirectionIds</c>. CoverImageId is set to <c>dto.CoverageId.ToString()</c>.
    /// </remarks>
    public static OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent SetToModel(this CompetitiveEventCreateUpdateDto dto, OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
    {
        model.Title = dto.Title;
        model.ShortTitle = dto.ShortTitle;
        model.State = dto.State;
        model.RegistrationStartTime = dto.RegistrationStartTime ?? model.RegistrationStartTime;
        model.RegistrationEndTime = dto.RegistrationEndTime ?? model.RegistrationEndTime;
        model.ParentId = dto.ParentId;
        model.CoverageId = dto.CoverageId;
        model.ScheduledStartTime = dto.ScheduledStartTime;
        model.ScheduledEndTime = dto.ScheduledEndTime;
        model.NumberOfSeats = dto.NumberOfSeats;
        model.CompetitiveEventAccountingTypeId = dto.CompetitiveEventAccountingTypeId;
        model.DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure;
        model.OrganizerOfTheEventId = dto.OrganizerOfTheEventId;
        model.PlannedFormatOfClasses = dto.PlannedFormatOfClasses ?? model.PlannedFormatOfClasses;
        model.VenueName = dto.VenueName;
        model.TermsOfParticipation = dto.TermsOfParticipation;
        model.AreThereBenefits = dto.AreThereBenefits ?? model.AreThereBenefits;
        model.Benefits = dto.Benefits;
        model.MinimumAge = dto.MinimumAge;
        model.MaximumAge = dto.MaximumAge ?? model.MaximumAge;
        model.Price = dto.Price ?? model.Price;
        model.CompetitiveSelection = dto.CompetitiveSelection ?? model.CompetitiveSelection;
        model.Contacts = dto.Contacts?.ToModel() ?? model.Contacts;
        model.SubDirections = dto.SubDirectionIds.Select(id => new SubDirection { DirectionId = id }).ToList();
        model.CoverImageId = dto.CoverageId.ToString();

        return model;
    }
}