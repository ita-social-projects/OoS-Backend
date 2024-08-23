using OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

namespace OutOfSchool.BusinessLogic.Services.Memento;

public class Memento : IMemento
{
    public KeyValuePair<string, string?> State { get; set; }
}