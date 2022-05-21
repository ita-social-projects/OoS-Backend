using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageDependentEntityImagesInteractionMediator<in TEntity> : IEntitySetOfImagesInteractionMediator<TEntity>, IEntityCoverImageInteractionMediator<TEntity>
        where TEntity : class, IKeyedEntity, IImageDependentEntity<TEntity>
    {
    }
}
