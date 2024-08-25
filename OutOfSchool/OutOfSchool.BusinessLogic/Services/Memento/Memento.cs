using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;

/// <summary>
/// Class for storing a memento in cache.
/// </summary>
public class Memento : IMemento
{
    /// <summary>Gets or sets the state of the memento.</summary>
    /// <value>The state of the memento.</value>
    public KeyValuePair<string, string?> State { get; set; }
}