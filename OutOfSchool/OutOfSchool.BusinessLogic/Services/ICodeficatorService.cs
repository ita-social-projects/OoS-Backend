using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

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
    /// <param name="id">Parent's id of entity which children we want to receive.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="IEnumerable{CodeficatorDto}"/> found elements.</returns>
    public Task<IEnumerable<CodeficatorDto>> GetChildrenByParentId(long? id = null);

    /// <summary>
    /// Get all address's parts from the database.
    /// </summary>
    /// <param name="id"> Codeficator's id.</param>
    /// <returns>The task result contains the <see cref="Task{AllAddressPartsDto}"/>.</returns>
    public Task<AllAddressPartsDto> GetAllAddressPartsById(long id);

    /// <summary>
    /// Get full addresses' names from the database.
    /// </summary>
    /// <param name="filter">Filter for the search.</param>
    /// <returns>The task result contains the list of <see cref="Task{CodeficatorAddressDto}"/>.</returns>
    public Task<List<CodeficatorAddressDto>> GetFullAddressesByPartOfName(CodeficatorFilter filter);

    /// <summary>
    /// Get the nearest codeficator entry by latitude & longitude.
    /// </summary>
    /// <param name="lat">Latitude (id degrees).</param>
    /// <param name="lon">Longitude (in degrees).</param>
    /// <param name="categories">Categories for search.</param>
    /// <returns>The task result contains a <see cref="CodeficatorAddressDto"/>.</returns>
    public Task<CodeficatorAddressDto> GetNearestByCoordinates(double lat, double lon, string categories = default);

    /// <summary>
    /// Get the subsettlements of current settlement by CATOTTG id.
    /// </summary>
    /// <param name="catottgId">CATOTTG id</param>
    /// <returns>The task result contains a <see cref="List{TResult}"/> that contains subsettlements ids.</returns>
    public Task<IEnumerable<long>> GetAllChildrenIdsByParentIdAsync(long catottgId);
}
