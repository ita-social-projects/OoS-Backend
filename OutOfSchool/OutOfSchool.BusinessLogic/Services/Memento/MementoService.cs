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
    /// <exception cref="ArgumentNullException">Memento.</exception>
    public MementoService(IMemento memento)
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
        if (memento.Value is null)
        {
            State = default;
        }
        else
        {
            State = JsonConvert.DeserializeObject<T>(memento.Value);
        }
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