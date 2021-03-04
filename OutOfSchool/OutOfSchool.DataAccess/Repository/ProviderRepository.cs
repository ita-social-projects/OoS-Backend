﻿using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Repository
{
    public class ProviderRepository : EntityRepository<Provider>,IProviderRepository
    {
        public ProviderRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
        {

        }

        /// <summary>
        /// Checks entity elements for uniqueness
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>Bool</returns>
        public bool IsUnique(Provider entity) => GetAll().Result.Any(x => x.EDRPOU != entity.EDRPOU || x.INPP != entity.INPP);
    }
}
