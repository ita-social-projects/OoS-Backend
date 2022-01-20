using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Services.Models.Images
{
    public interface IImageDependentEntity<TEntity>
    {
        public List<Image<TEntity>> Images { get; set; }
    }
}
