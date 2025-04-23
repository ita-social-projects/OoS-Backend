namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ApplicationChangesLogDto : ChangesLogDtoBase
{
    public Guid ApplicationId { get; set; }

    public string WorkshopTitle { get; set; }

    public string WorkshopCity { get; set; }

    public string ProviderTitle { get; set; }
}

public static class ApplicationChangesLogDtoExtensions
{
    public static ApplicationChangesLogDto ToDto(this ChangesLog changesLog, OutOfSchool.Services.Models.Application application)
        => new()
        {
            FieldName = changesLog.PropertyName,
            OldValue = changesLog.OldValue,
            NewValue = changesLog.NewValue,
            UpdatedDate = DateTime.SpecifyKind(changesLog.UpdatedDate, DateTimeKind.Utc),
            User = changesLog.User.ToShortUser(),
            ApplicationId = changesLog.EntityIdGuid.Value,
            WorkshopTitle = application.Workshop.Title,
            WorkshopCity = application.Workshop.Contacts.SingleOrDefault(c => c.IsDefault).Address.CATOTTG.Name,
            ProviderTitle = application.Workshop.ProviderTitle,
        };
}