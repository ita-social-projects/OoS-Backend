using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.Services.CombinedProviders
{
    public class ImageDependentRepositoriesProvider : IImageDependentRepositoriesProvider
    {
        private readonly OutOfSchoolDbContext db;
        private IWorkshopRepository workshopRepository;

        public ImageDependentRepositoriesProvider(OutOfSchoolDbContext db)
        {
            this.db = db;
        }

        public IWorkshopRepository WorkshopRepository => workshopRepository ??= new WorkshopRepository(db);
    }
}
