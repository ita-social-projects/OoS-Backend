#nullable enable

namespace OutOfSchool.BusinessLogic.Services.TempSave;

public interface ITempSaveService<T>
{
    Task<T?> RestoreAsync(string key);

    Task StoreAsync(string key, T value);

    Task RemoveAsync(string key);

    Task<TimeSpan?> GetTimeToLiveAsync(string key);
}