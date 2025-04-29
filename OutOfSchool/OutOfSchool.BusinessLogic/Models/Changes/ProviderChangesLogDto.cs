namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ProviderChangesLogDto : ChangesLogDtoBase
{
    public Guid ProviderId { get; set; }

    public string ProviderTitle { get; set; }

    public string ProviderCity { get; set; }
}

public static class ProviderChangesLogDtoExtensions
{
    public static ProviderChangesLogDto ToDto(this ChangesLog changesLog, Provider provider)
        => new()
        {
            FieldName = changesLog.PropertyName,
            OldValue = changesLog.OldValue,
            NewValue = changesLog.NewValue,
            UpdatedDate = DateTime.SpecifyKind(changesLog.UpdatedDate, DateTimeKind.Utc),
            User = changesLog.User?.ToShortUser(),
            InstitutionTitle = provider.Institution.Title,
            ProviderId = changesLog.EntityIdGuid ?? default,
            ProviderTitle = provider.FullTitle,
            ProviderCity = provider.Contacts?.SingleOrDefault(c => c.IsDefault)?.Address?.CATOTTG?.Name,
        };
}