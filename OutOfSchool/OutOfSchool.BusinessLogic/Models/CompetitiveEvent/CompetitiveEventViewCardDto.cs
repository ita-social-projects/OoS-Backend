using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventViewCardDto
{
    public Guid Id { get; set; }

    [DataType(DataType.Text)]
    public string Title { get; set; }
    
    [DataType(DataType.Text)]
    public string ShortTitle { get; set; }
    
    public string CoverImageId { get; set; }
}

public static class CompetitiveEventViewCardDtoExtensions
{
    public static CompetitiveEventViewCardDto ToViewCardDto(this OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent model)
        => new()
        {
            Id = model.Id,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            CoverImageId = model.CoverImageId,
        };

    public static List<CompetitiveEventViewCardDto> ToViewCardDto(this IEnumerable<OutOfSchool.Services.Models.CompetitiveEvents.CompetitiveEvent> list)
        => list.MapToList(ToViewCardDto);
}