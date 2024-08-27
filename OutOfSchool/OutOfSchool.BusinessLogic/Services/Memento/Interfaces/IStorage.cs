namespace OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

public interface IStorage
{
    Task SetMementoValueAsync(KeyValuePair<string, string?> keyValue);

    Task<KeyValuePair<string, string?>> GetMementoValueAsync(string key);

    Task RemoveMementoAsync(string key);
}
