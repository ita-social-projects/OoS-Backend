using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.WebApi.Services.Images;

public interface IImageDependentEntityImagesInteractionService<in TEntity> : IEntitySetOfImagesInteractionService<TEntity>, IEntityCoverImageInteractionService<TEntity>
    where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>
{
}