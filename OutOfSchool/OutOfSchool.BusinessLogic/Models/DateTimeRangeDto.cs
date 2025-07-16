using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models;

public class DateTimeRangeDto
{
    public long Id { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan StartTime { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan EndTime { get; set; }

    [Required]
    public List<DaysBitMask> Workdays { get; set; }
}

public static class DateTimeRangeDtoExtensions
{
    public static DateTimeRangeDraft ToDraft(this DateTimeRangeDto dto)
        => new()
        {
            StartTime = TimeOnly.FromTimeSpan(dto.StartTime),
            EndTime = TimeOnly.FromTimeSpan(dto.EndTime),
            Workdays = dto.Workdays?.ToHashSet() ?? []
        };

    public static List<DateTimeRangeDraft> ToDraft(this IEnumerable<DateTimeRangeDto> list)
        => list.MapToList(ToDraft);

    public static DateTimeRangeES ToES(this DateTimeRangeDto dto)
        => new()
        {
            Id = dto.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Workdays = string.Join(' ', dto.Workdays ?? []),
            // WorkshopId - ignored in original AM mapping
        };

    public static List<DateTimeRangeES> ToES(this IEnumerable<DateTimeRangeDto> list)
        => list.MapToList(ToES);

    public static DateTimeRange ToModel(this DateTimeRangeDto dto)
        => new()
        {
            Id = dto.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Workdays = dto.Workdays?.ToDaysBitMask() ?? default,
        };

    public static List<DateTimeRange> ToModel(this IEnumerable<DateTimeRangeDto> list)
        => list.MapToList(ToModel);

    public static DateTimeRange SetToModel(this DateTimeRangeDto dto, DateTimeRange model)
    {
        model.Id = dto.Id;
        model.StartTime = dto.StartTime;
        model.EndTime = dto.EndTime;
        model.Workdays = dto.Workdays?.ToDaysBitMask() ?? default;

        return model;
    }

    public static List<DateTimeRange> SetToModel(this IEnumerable<DateTimeRangeDto> list, IEnumerable<DateTimeRange> modelList)
    {
        var result = new List<DateTimeRange>(modelList);

        foreach (var item in list)
        {
            var newModelItem = new DateTimeRange();
            result.Add(item.SetToModel(newModelItem));
        }

        modelList = result;

        return (List<DateTimeRange>)modelList;
    }

    public static DateTimeRangeDto ToDto(this DateTimeRangeDraft model)
        => new()
        {
            StartTime = model.StartTime.ToTimeSpan(),
            EndTime = model.EndTime.ToTimeSpan(),
            Workdays = model.Workdays?.ToList()
        };

    public static List<DateTimeRangeDto> ToDto(this IEnumerable<DateTimeRangeDraft> list)
        => list.MapToList(ToDto);

    public static DateTimeRangeDto ToDto(this DateTimeRange model)
        => new()
        {
            Id = model.Id,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Workdays = model.Workdays.ToDaysBitMaskEnumerable().ToList()
        };

    public static List<DateTimeRangeDto> ToDto(this IEnumerable<DateTimeRange> list)
        => list.MapToList(ToDto);

    public static List<DateTimeRangeDto> ToNotDeletedDto(this IEnumerable<DateTimeRange> list)
        => list.MapNonDeletedToList(ToDto);
}