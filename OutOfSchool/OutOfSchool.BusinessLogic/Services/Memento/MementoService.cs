using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;
public class MementoService<T> : IMementoService<T>
{
    private readonly IMemento memento;
    private readonly ILogger<MementoService<T>> logger;

    public MementoService(IMemento memento, ILogger<MementoService<T>> logger)
    {
        this.memento = memento ?? throw new ArgumentNullException(nameof(memento));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public T? State { get; set; }

    public void RestoreMemento(KeyValuePair<string, string?> memento)
    {
        logger.LogInformation("Restoring memento started");
        State = JsonConvert.DeserializeObject<T>(memento.Value ?? string.Empty);
        logger.LogInformation("Restoring memento finished");
    }

    public IMemento CreateMemento(string key, T value)
    {
        logger.LogInformation("Creating memento started");
        memento.State = new KeyValuePair<string, string?>(GetMementoKey(key), JsonConvert.SerializeObject(value));
        logger.LogInformation("Creating memento finished");
        return memento;
    }

    public string GetMementoKey(string key)
    {
        return string.Format("{0}_{1}", key, typeof(T).Name);
    }
}