namespace OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

public interface IDraftStorageService<T>
{
    Task<T> RestoreAsync(string key);

    Task CreateAsync(string key, T value);

    Task RemoveAsync(string key);
}