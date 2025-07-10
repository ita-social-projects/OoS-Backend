using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

public static class WorkshopV2CreateRequestDtoGenerator
{
    private static readonly Faker<WorkshopV2CreateRequestDto> Faker = new Faker<WorkshopV2CreateRequestDto>()
        .RuleFor(x => x.TagIds, f => f.Make(5, () => f.Random.Long(1, 100)))
        .CustomInstantiator(f =>
        {
            var dto = new WorkshopV2CreateRequestDto();
            WorkshopRequiredPropertiesDtoGenerator.Populate(dto);
            return dto;
        });

    public static WorkshopV2CreateRequestDto Generate() => Faker.Generate();

    public static List<WorkshopV2CreateRequestDto> Generate(int count) => Faker.Generate(count);

    public static WorkshopV2CreateRequestDto FromModel(this Workshop model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            DateTimeRanges = model.DateTimeRanges?.ToDto(),
            FormOfLearning = model.FormOfLearning,
            CompetitiveSelection = model.CompetitiveSelection,
            CompetitiveSelectionDescription = model.CompetitiveSelectionDescription,
            ProviderId = model.ProviderId,
            StudyPeriodDates = model.ToStudyPeriodDatesDto(),

            WorkshopType = model.WorkshopType,
            ParentWorkshopId = model.ParentWorkshopId,
            IsPaid = model.IsPaid,
            Price = model.Price,
            PayRate = model.PayRate,
            AreThereBenefits = model.AreThereBenefits,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,

            WorkshopDescriptionItems = model.WorkshopDescriptionItems?.ToDto(),
            InstitutionHierarchyId = model.InstitutionHierarchyId,
            Keywords = model.Keywords?.Split(OutOfSchool.Common.Constants.MappingSeparator, StringSplitOptions.RemoveEmptyEntries).ToList(),
            EnrollmentProcedureDescription = model.EnrollmentProcedureDescription,
            Coverage = model.Coverage,

            DefaultTeacher = model.DefaultTeacher?.ToDto(),
            DefaultTeacherId = model.DefaultTeacherId,

            AvailableSeats = model.AvailableSeats,
            Contacts = model.Contacts?.ToDto(),
            LanguageOfEducationId = model.LanguageOfEducationId,
            IsChampionPath = model.IsChampionPath,

            TagIds = model.Tags.Select(t => t.Id).ToList() ?? [ ]
        };

    public static void Populate(WorkshopV2CreateRequestDto dto) => Faker.Populate(dto);
}