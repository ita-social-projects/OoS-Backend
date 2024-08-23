namespace OutOfSchool.BusinessLogic.Services.Memento.Interfaces;

public interface IMemento
{
    KeyValuePair<string, string?> State { get; set; }
}