using System.Collections.Generic;

namespace OutOfSchool.Services.Models.Images;

public interface IImageDependentEntity<TEntity>
{
    public string CoverImageId { get; set; }

    public List<Image<TEntity>> Images { get; set; }
}