using System.Linq;
using NUnit.Framework;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;
[TestFixture]
public class ElasticSearchMappingExtensionsTests
{
    [Test]
    public void Mapping_WorkshopDto_ToESModel_IsCorrect()
    {
        var workshopDto = WorkshopDtoGenerator.Generate();

        var workshopES = workshopDto.ToESModel();

        Assert.That(
            workshopDto.WorkshopDescriptionItems.ToList()
                .Aggregate(string.Empty, (accumulator, dto) =>
                    $"{accumulator}{dto.SectionName}¤{dto.Description}¤"),
            Is.EqualTo(workshopES.Description));
        Assert.That(
            string.Join('¤', workshopDto.Keywords.Distinct()),
            Is.EqualTo(workshopES.Keywords));
    }

    [Test]
    public void Mapping_WorkshopES_ToDto_IsCorrect()
    {
        var workshopES = WorkshopESGenerator.Generate();

        var workshopCardDto = workshopES.ToCardDto();

        Assert.That(
            workshopES.Address.Point.Latitude,
            Is.EqualTo(workshopCardDto.Address.Latitude));

        Assert.That(
            workshopES.Address.Point.Longitude,
            Is.EqualTo(workshopCardDto.Address.Longitude));
    }
}
