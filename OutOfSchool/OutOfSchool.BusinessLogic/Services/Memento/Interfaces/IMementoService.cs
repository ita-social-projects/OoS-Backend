namespace OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
public interface IMementoService<T>
{
    Task<T> RestoreMemento(string key);

    Task CreateMemento(string key, T value);

    Task RemoveMementoAsync(string key);
}