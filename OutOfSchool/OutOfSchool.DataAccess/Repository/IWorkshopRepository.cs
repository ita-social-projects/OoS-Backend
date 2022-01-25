﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepositoryBase<Guid, Workshop>
    {
        /// <summary>
        /// Checks entity classId existence.
        /// </summary>
        /// <param name="id">Class id.</param>
        /// <returns>True if Class exists, otherwise false.</returns>
        bool ClassExists(long id);

        Task<Workshop> GetWithNavigations(Guid id);

        Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids);

        /// <summary>
        /// Update prodider's properies in all workshops with specified provider.
        /// </summary>
        /// <param name="provider">Provider to be searched by.</param>
        /// <returns>List of Workshops for the specified provider.</returns>
        Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider);
    }
}
