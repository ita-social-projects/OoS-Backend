using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }

    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

    [Required]
    [EnumDataType(typeof(NotificationType), ErrorMessage = Constants.EnumErrorMessage)]
    public NotificationType Type { get; set; }

    [Required]
    [EnumDataType(typeof(NotificationAction), ErrorMessage = Constants.EnumErrorMessage)]
    public NotificationAction Action { get; set; }

    [Required]
    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? ReadDateTime { get; set; }

    public Guid? ObjectId { get; set; }
}

public static class NotificationDtoExtensions
{
    public static Notification ToModel(this NotificationDto dto)
        => new()
        {
            UserId = dto.UserId,
            Data = dto.Data,
            Type = dto.Type,
            Action = dto.Action,
            CreatedDateTime = dto.CreatedDateTime,
            ReadDateTime = dto.ReadDateTime,
        };

    public static NotificationDto ToDto(this Notification model)
        => new()
        {
            Id = model.Id,
            UserId = model.UserId,
            Data = model.Data,
            Type = model.Type,
            Action = model.Action,
            CreatedDateTime = model.CreatedDateTime,
            ReadDateTime = model.ReadDateTime,
        };

    public static List<NotificationDto> ToDto(this IEnumerable<Notification> list)
        => list.MapToList(ToDto);
}
