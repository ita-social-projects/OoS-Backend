using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models;

public class DateTimeRangeDto : IValidatableObject
{
    public long Id { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan StartTime { get; set; }

    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan EndTime { get; set; }

    [Required]
    public List<DaysBitMask> Workdays { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime >= EndTime)
            yield return new ValidationResult("The end time cannot be equal to or earlier than the start time.", new[] { nameof(EndTime) });

        if (Workdays.IsNullOrEmpty() || Workdays.Any(workday => workday == DaysBitMask.None))
        {
            yield return new ValidationResult("Workdays are required.", new[] { nameof(Workdays) });
        } else
        {
            const DaysBitMask allValidDays = DaysBitMask.Monday | DaysBitMask.Tuesday |
                                         DaysBitMask.Wednesday | DaysBitMask.Thursday |
                                         DaysBitMask.Friday | DaysBitMask.Saturday |
                                         DaysBitMask.Sunday;

            foreach ( var dayValue in Workdays)
            {
                if ((dayValue & ~allValidDays) != 0)
                {
                    yield return new ValidationResult($"The value '{(int)dayValue}' contains undefined day bits.",
                        new[] { nameof(Workdays) });
                }
            }

            Workdays = Workdays.Distinct().ToList();
        }
    }
}

public static class DateTimeRangeDtoExtensions
{
    public static DateTimeRangeDraft ToDraft(this DateTimeRangeDto dto)
        => new()
        {
            StartTime = TimeOnly.FromTimeSpan(dto.StartTime),
            EndTime = TimeOnly.FromTimeSpan(dto.EndTime),
            Workdays = dto.Workdays?.ToDaysBitMask()
                .ToDaysBitMaskEnumerable()
                .ToHashSet() ?? []
        };

    public static List<DateTimeRangeDraft> ToDraft(this IEnumerable<DateTimeRangeDto> list)
        => list.MapToList(ToDraft);

    public static DateTimeRangeES ToES(this DateTimeRangeDto dto)
    {
        var workdays = dto.Workdays?.ToDaysBitMask()
            .ToDaysBitMaskEnumerable()
            .ToHashSet();

        return new()
        {
            Id = dto.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Workdays = string.Join(' ', workdays ?? []),
            // WorkshopId - ignored in original AM mapping
        };
    }

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

    public static List<DateTimeRange> SetToModel(this List<DateTimeRangeDto> dtoList, IEnumerable<DateTimeRange> modelList)
    {
        var activeModels = modelList.Where(dtr => dtr.IsDeleted == false).ToList();
        var result = new List<DateTimeRange>();
        var dtoListDict = dtoList.ToDictionary(DateTimeRangeDtoToString);

        // Add active models that have matching DTOs to the result
        foreach (var model in activeModels)
        {
            var key = DateTimeRangeToString(model);
            if (dtoListDict.ContainsKey(key))
            {
                result.Add(model);
                dtoListDict.Remove(key);
            }
        }

        // Add new models for all new DTOs to the result
        result.AddRange(dtoListDict.Values.Select(dto => dto.ToModel()));

        return result;
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

    private static string DateTimeRangeDtoToString(DateTimeRangeDto dto)
    {
        var builder = new StringBuilder(dto.StartTime.ToString());
        builder.Append(dto.EndTime.ToString());
        builder.Append(dto.Workdays?.ToDaysBitMask().GetHashCode());

        return builder.ToString();
    }

    private static string DateTimeRangeToString(DateTimeRange model)
    {
        var builder = new StringBuilder(model.StartTime.ToString());
        builder.Append(model.EndTime.ToString());
        builder.Append(model.Workdays.GetHashCode());

        return builder.ToString();
    }
}