namespace OutOfSchool.BusinessLogic.Services.Memento.Interfaces;
public interface IMementoService<T>
{
    T? State { get; set; }

    IMemento CreateMemento(string key, T value);

    void RestoreMemento(KeyValuePair<string, string?> memento);

    string GetMementoKey(string key);
}