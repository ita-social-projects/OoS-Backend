using System.Linq.Expressions;
using H3Lib;
using H3Lib.Extensions;
using OutOfSchool.BusinessLogic.Models.Codeficator;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Implements the interface with CRUD functionality for Codeficator entity.
/// </summary>
/// <param name="codeficatorRepository">СodeficatorRepository repository.</param>
public class CodeficatorService(ICodeficatorRepository codeficatorRepository) : ICodeficatorService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<CodeficatorDto>> GetChildrenByParentId(long? id = null)
    {
        var filter = GetFilter(id, CodeficatorCategory.Level1);

        var codeficators = await codeficatorRepository.GetByFilter(filter).ConfigureAwait(false);

        return codeficators.ToCodeficatorDto();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<KeyValuePair<long, string>>> GetChildrenNamesByParentId(long? id = null)
    {
        var filter = GetFilter(id, CodeficatorCategory.Level1);

        return await codeficatorRepository.GetNamesByFilter(filter).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<AllAddressPartsDto> GetAllAddressPartsById(long id)
    {
        var codeficator = await codeficatorRepository.GetById(id).ConfigureAwait(false);

        return codeficator?.ToAllAddressPartsDto();
    }

    /// <inheritdoc/>
    public async Task<List<CodeficatorAddressDto>> GetFullAddressesByPartOfName(CodeficatorFilter filter)
    {
        filter ??= new CodeficatorFilter();

        return await codeficatorRepository.GetFullAddressesByPartOfName(filter.Name, filter.Categories, filter.ParentId).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<CodeficatorAddressDto> GetNearestByCoordinates(double lat, double lon, string categories = default)
    {
        var searchableEntries = string.IsNullOrEmpty(categories) ? CodeficatorCategory.Level4.Name : categories;

        var hash = default(GeoCoord).SetDegrees(Convert.ToDecimal(lat), Convert.ToDecimal(lon));
        var h3Location = Api.GeoToH3(hash, GeoMathHelper.ResolutionForCity);
        Api.KRing(h3Location, GeoMathHelper.KRingForResolution, out var neighbours);

        var closestCities = await codeficatorRepository.GetByFilter(c => neighbours
            .Select(n => n.Value)
            .Any(geo => geo == c.GeoHash) && searchableEntries.Contains(c.Category, StringComparison.Ordinal));

        return closestCities
            .Select(city => new
            {
                city,
                Distance = GeoMathHelper
                    .GetDistanceFromLatLonInKm(
                        city.Latitude,
                        city.Longitude,
                        lat,
                        lon),
            })
            .OrderBy(p => p.Distance)
            .Select(c => c.city.ToCodeficatorAddressDto())
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<long>> GetAllChildrenIdsByParentIdAsync(long catottgId)
    {
        var parent = await codeficatorRepository.GetById(catottgId).ConfigureAwait(false);
        if (parent == null)
        {
            return Enumerable.Empty<long>();
        }

        var result = new List<long> { catottgId };

        var childrenIds = await codeficatorRepository.GetIdsByParentIds(result).ConfigureAwait(false);

        while (childrenIds.Any())
        {
            result.AddRange(childrenIds);
            childrenIds = await codeficatorRepository.GetIdsByParentIds(childrenIds).ConfigureAwait(false);
        }

        return result;
    }

    #region privateMethods

    private static Expression<Func<CATOTTG, bool>> GetFilter(long? parentId, CodeficatorCategory level)
    {
        if (parentId.HasValue)
        {
            return c => c.ParentId == parentId.Value;
        }

        return c => level.Name.Contains(c.Category, StringComparison.Ordinal);
    }

    #endregion
}