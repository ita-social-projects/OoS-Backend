using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;

/// <summary>
/// Implements the IMementoService{T} interface with CRUD functionality to store an entity of type T in a cache.
/// </summary>
/// <typeparam name="T">T is the entity type that should be stored in the cache.</typeparam>
public class MementoService<T> : IMementoService<T>
{
    private readonly IMemento memento;

    /// <summary>Initializes a new instance of the <see cref="MementoService{T}" /> class.</summary>
    /// <param name="memento">The memento.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">Memento.</exception>
    public MementoService(IMemento memento, ILogger<MementoService<T>> logger)
    {
        this.memento = memento ?? throw new ArgumentNullException(nameof(memento));
    }

    /// <summary>Gets or sets the State of the memento.</summary>
    /// <value>The state of the memento.</value>
    public T? State { get; set; }

    /// <summary>Restores the memento.</summary>
    /// <param name="memento">The memento.</param>
    public void RestoreMemento(KeyValuePair<string, string?> memento)
    {
        State = JsonConvert.DeserializeObject<T>(memento.Value ?? string.Empty);
    }

    /// <summary>Creates the memento.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// A memento of the type IMemento.
    /// </returns>
    public IMemento CreateMemento(string key, T value)
    {
        memento.State = new KeyValuePair<string, string?>(GetMementoKey(key), JsonConvert.SerializeObject(value));
        return memento;
    }

    /// <summary>Gets the memento key.</summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// Memento key of string type.
    /// </returns>
    public string GetMementoKey(string key)
    {
        return string.Format("{0}_{1}", key, typeof(T).Name);
    }
}