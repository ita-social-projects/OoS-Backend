using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionMediator<in TEntity> : ISingleImageInteractionMediator<TEntity>, IMultipleImageInteractionMediator<TEntity>
        where TEntity : class, IKeyedEntity
    {
    }
}
