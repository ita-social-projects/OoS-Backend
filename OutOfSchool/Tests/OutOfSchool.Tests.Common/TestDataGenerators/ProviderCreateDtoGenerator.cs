using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.Common.TestDataGenerators;
public static class ProviderCreateDtoGenerator
{
    private static readonly Faker<ProviderCreateDto> faker = new Faker<ProviderCreateDto>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
        .RuleFor(x => x.ShortTitle, f => f.Company.CompanySuffix())
        .RuleFor(x => x.Ownership, f => f.Random.ArrayElement((OwnershipType[])Enum.GetValues(typeof(OwnershipType))))
        .RuleFor(x => x.Type, _ => new ProviderTypeDto { Id = 1, Name = "pro" })
        .RuleFor(x => x.Status, f => f.Random.ArrayElement((ProviderStatus[])Enum.GetValues(typeof(ProviderStatus))))
        .RuleFor(x => x.License, f => f.Random.AlphaNumeric(15))
        .RuleFor(x => x.InstitutionType, f => f.PickRandom<InstitutionType>());

    public static ProviderCreateDto FromModel(Provider model)
        => new()
        {
            Id = model.Id,
            FullTitle = model.FullTitle,
            ShortTitle = model.ShortTitle,
            FullTitleEn = model.FullTitleEn,
            ShortTitleEn = model.ShortTitleEn,
            GeneralWorkSchedule = model.GeneralWorkSchedule,
            TypeId = model.TypeId,
            Type = model.Type?.ToDto(),
            License = model.License,
            InstitutionStatusId = model.InstitutionStatusId,
            InstitutionId = model.InstitutionId,
            InstitutionType = model.InstitutionType,
            ProviderSectionItems = model.ProviderSectionItems?.ToDto(),
            UsesOutsourcingServices = model.UsesOutsourcingServices,
            InstitutionCode = model.InstitutionCode,
            IsStructuralUnit = model.IsStructuralUnit,
            IsLocatedInMountainousArea = model.IsLocatedInMountainousArea,
            Ownership = model.Ownership,
        };

    /// <summary>
    /// Creates new instance of the <see cref="Provider"/> class with random data.
    /// </summary>
    /// <returns><see cref="Provider"/> object.</returns>
    public static ProviderCreateDto Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of the <see cref="Provider"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<ProviderCreateDto> Generate(int count) => faker.Generate(count);
    
    public static ProviderCreateDto WithAddress(this ProviderCreateDto provider, ContactsAddressDto address = null)
    {
        address ??= ContactsAddressDtoGenerator.Generate();
        var contacts = new ContactsDto()
        {
            Title = "Test",
            IsDefault = true,
            Address = address,
        };
        return TestDataHelper.ApplyOnItem(provider, (provider, contacts) => { provider.Contacts = [contacts]; }, contacts);
    }

    public static List<ProviderCreateDto> WithAddress(this List<ProviderCreateDto> providers, ContactsAddressDto address = null)
        => providers.Select(x => WithAddress(x, address)).ToList();
}
