using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.Services.CombinedProviders
{
    public interface IImageDependentRepositoriesProvider
    {
        public IWorkshopRepository WorkshopRepository { get; }
    }
}
