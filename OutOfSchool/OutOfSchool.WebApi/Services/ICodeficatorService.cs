using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Codeficator;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Codeficator entity.
/// </summary>
public interface ICodeficatorService
{
    /// <summary>
    /// Get only the Id and Name of the child entities as a list from the database.
    /// </summary>
    /// <param name="id"> Parent's id of entity which children we want to receive. </param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="IEnumerable{KeyValuePair}"/> found elements.</returns>
    public Task<IEnumerable<KeyValuePair<long, string>>> GetChildrenNamesByParentId(long? id = null);

    /// <summary>
    /// Get list of chaild entities from the database.
    /// </summary>
    /// <param name="id">Parent's id of entity which children we want to receive. </param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="IEnumerable{CodeficatorDto}"/> found elements.</returns>
    public Task<IEnumerable<CodeficatorDto>> GetChildrenByParentId(long? id = null);

    /// <summary>
    /// Get all address's parts from the database.
    /// </summary>
    /// <param name="id"> Codeficator's id. </param>
    /// <returns>The task result contains the <see cref="Task{AllAddressPartsDto}"/>.</returns>
    public Task<AllAddressPartsDto> GetAllAddressPartsById(long id);

    /// <summary>
    /// Get full addresses' names from the database.
    /// </summary>
    /// <param name="filter">Filter for the search.</param>
    /// <returns>The task result contains the <see cref="Task{Dictionary}"/>.</returns>
    public Task<Dictionary<long, string>> GetFullAddressesByPartOfName(CodeficatorFilter filter);
}