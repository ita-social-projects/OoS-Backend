namespace OutOfSchool.Services.Models;

public interface IDto<TEntity, TKey>
    where TEntity : IKeyedEntity<TKey>
{
    TKey Id { get; set; }
}
