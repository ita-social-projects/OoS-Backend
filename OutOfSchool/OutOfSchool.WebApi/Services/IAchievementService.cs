using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Achievement entity.
    /// </summary>
    public interface IAchievementService
    {
        /// <summary>
        /// To recieve the Achievement object with define id.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="AchievementDto"/> that was found.</returns>
        Task<AchievementDto> GetById(Guid id);

        /// <summary>
        /// To recieve all Achievement objects by Workshop id.
        /// </summary>
        /// <param name="id">Workshop Key in the table.</param>
        /// <returns>List of Achievement objects.</returns>
        Task<IEnumerable<AchievementDto>> GetByWorkshopId(Guid id);

        /// <summary>
        /// Add new Achievement to the DB.
        /// </summary>
        /// <param name="dto">AchievementCreateDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="AchievementCreateDTO"/> that was created.</returns>
        Task<AchievementDto> Create(AchievementCreateDTO dto);

        /// <summary>
        /// To Update our object in DB.
        /// </summary>
        /// <param name="dto">Achievement with new properties.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="AchievementDto"/> that was updated.</returns>
        Task<AchievementDto> Update(AchievementCreateDTO dto);

        /// <summary>
        /// To delete the object from DB.
        /// </summary>
        /// <param name="id">Key of the Achievement in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);
    }
}
