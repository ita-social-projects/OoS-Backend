using OutOfSchool.WebApi.Models;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services.Interfaces
{
    /// <summary>
    /// Interface of ChildService.
    /// </summary>
    public interface IChildService
    {
        /// <summary>
        /// Add new Child to the database.
        /// </summary>
        /// <param name="child">ChildDTO element.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ChildDTO> Create(ChildDTO child);
    }
}
