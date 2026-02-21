using System.Collections.Generic;

namespace OutOfSchool.Services.Models.Images;

public interface IHasEntityImages<TEntity>
{
    List<Image<TEntity>> Images { get; }
}