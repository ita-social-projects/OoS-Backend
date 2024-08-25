using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;
public class MementoService<T> : IMementoService<T>
{
    private readonly IMemento memento;

    public MementoService(IMemento memento, ILogger<MementoService<T>> logger)
    {
        this.memento = memento ?? throw new ArgumentNullException(nameof(memento));
    }

    public T? State { get; set; }

    public void RestoreMemento(KeyValuePair<string, string?> memento)
    {
        State = JsonConvert.DeserializeObject<T>(memento.Value ?? string.Empty);
    }

    public IMemento CreateMemento(string key, T value)
    {
        memento.State = new KeyValuePair<string, string?>(GetMementoKey(key), JsonConvert.SerializeObject(value));
        return memento;
    }

    public string GetMementoKey(string key)
    {
        return string.Format("{0}_{1}", key, typeof(T).Name);
    }
}