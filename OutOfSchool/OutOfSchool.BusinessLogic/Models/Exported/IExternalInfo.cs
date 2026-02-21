namespace OutOfSchool.BusinessLogic.Models.Exported;

public interface IExternalInfo<TKey>
{
    public TKey Id { get; set; }
}