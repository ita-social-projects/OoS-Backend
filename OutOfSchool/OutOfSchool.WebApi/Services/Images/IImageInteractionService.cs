using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Images;

namespace OutOfSchool.WebApi.Services.Images
{
    public interface IImageInteractionService<in TKey> : ISingleImageInteractionService<TKey>, IMultipleImageInteractionService<TKey>
    {
    }
}
