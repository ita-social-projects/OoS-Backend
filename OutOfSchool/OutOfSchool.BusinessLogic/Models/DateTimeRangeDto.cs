using System.ComponentModel.DataAnnotations;
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

        var existingDtos = dtoList.Where(dto => dto.Id > 0).ToList();
        var newDtos = dtoList.Where(dto => dto.Id <= 0).ToList();

        var existingDtoDict = existingDtos.ToDictionary(dto => dto.Id);

        // Update existing models that have matching DTOs
        foreach (var model in activeModels)
        {
            if (existingDtoDict.TryGetValue(model.Id, out var dto))
            {
                result.Add(dto.SetToModel(model));
            }
        }

        // Add new models for all new DTOs (those with ID = 0)
        result.AddRange(newDtos.Select(newDto => newDto.ToModel()));

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
}