using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.AverageRatings;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class RatingController : ControllerBase
{
    private readonly IRatingService ratingService;
    private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> esWorkshopService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ILogger<RatingController> logger;
    private readonly IAverageRatingService averageRatingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatingController"/> class.
    /// </summary>
    /// <param name="service">Service for Rating model.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="esWorkshopService">Service for operations with workshop documents of Elasticsearch data.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="averageRatingService">Service for AverageRating model.</param>
    public RatingController(
        IRatingService service,
        IStringLocalizer<SharedResource> localizer,
        IElasticsearchService<WorkshopES,
        WorkshopFilterES> esWorkshopService,
        ILogger<RatingController> logger,
        IAverageRatingService averageRatingService)
    {
        this.ratingService = service;
        this.localizer = localizer;
        this.esWorkshopService = esWorkshopService;
        this.logger = logger;
        this.averageRatingService = averageRatingService;
    }

    /// <summary>
    /// Check if exists an any rewiewed application in workshop for parent.
    /// </summary>
    /// <param name="parentId">Parent's id.</param>
    /// <param name="workshopId">Workshop's id.</param>
    /// <returns>Result of checking.</returns>
    [HasPermission(Permissions.RatingRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> IsReviewed(Guid parentId, Guid workshopId)
    {
        return Ok(await ratingService.IsReviewed(parentId, workshopId).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all ratings from the database.
    /// </summary>
    /// <param name="filter">Skip & Take number.</param>
    /// <returns>List of all ratings.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RatingDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] OffsetFilter filter)
    {
        var ratings = await ratingService.GetAsync(filter).ConfigureAwait(false);

        if (!ratings.Any())
        {
            return NoContent();
        }

        return Ok(ratings);
    }

    /// <summary>
    /// Get rating by it's id.
    /// </summary>
    /// <param name="id">Rating's id.</param>
    /// <returns>Rating.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        return Ok(await ratingService.GetById(id).ConfigureAwait(false));
    }

    /// <summary>
    /// Get all ratings from the database.
    /// </summary>
    /// <param name="entityId">Id of Entity.</param>
    /// <param name="filter">Skip & Take number.</param>
    /// <returns>The result is a <see cref="SearchResult{RatingDto}"/> that contains the count of all found ratings and a list of ratings that were received.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<RatingDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{entityType:regex(^provider$|^workshop$)}/{entityId}")]
    public async Task<IActionResult> GetByEntityId(Guid entityId, [FromQuery] OffsetFilter filter)
    {
        var ratings = await ratingService.GetAllByEntityId(entityId, filter).ConfigureAwait(false);

        if (ratings.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(ratings);
    }

    /// <summary>
    /// Get all ratings from the database.
    /// </summary>
    /// <param name="id">Provider Id.</param>
    /// <returns>List of all workshop ratings by provider.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RatingDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("byprovider/{id}")]
    public async Task<IActionResult> GetAllWorshopsByProvider(Guid id)
    {
        var ratings = await ratingService.GetAllWorshopsRatingByProvider(id).ConfigureAwait(false);

        if (!ratings.Any())
        {
            return NoContent();
        }

        return Ok(ratings);
    }

    /// <summary>
    /// Get parent rating for the specified entity.
    /// </summary>
    /// <param name="parentId">Id of Parent.</param>
    /// <param name="entityId">Id of Entity.</param>
    /// <returns>Parent rating for the specified entity.</returns>
    [HasPermission(Permissions.RatingRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RatingDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{entityType:regex(^provider$|^workshop$)}/{entityId}/parent/{parentId}")]
    public async Task<IActionResult> GetParentRating(Guid parentId, Guid entityId)
    {
        var rating = await ratingService.GetParentRating(parentId, entityId).ConfigureAwait(false);

        if (rating == null)
        {
            return NoContent();
        }

        return Ok(rating);
    }

    /// <summary>
    /// Add a new rating to the database.
    /// </summary>
    /// <param name="dto">Rating entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.RatingAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RatingDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(RatingDto dto)
    {
        var rating = await ratingService.Create(dto).ConfigureAwait(false);

        if (rating == null)
        {
            return BadRequest("Can't create a Rating with such parameters!\n" +
                              "Please check that entity, parent, type information are valid and don't exist in the system yet.");
        }

        await this.UpdateWorkshopInElasticSearch(dto.EntityId).ConfigureAwait(false);

        return CreatedAtAction(
            nameof(GetById),
            new { id = rating.Id, },
            rating);
    }

    /// <summary>
    /// Update info about a specific rating in the database.
    /// </summary>
    /// <param name="dto">Rating to update.</param>
    /// <returns>Rating.</returns>
    [HasPermission(Permissions.RatingEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(RatingDto dto)
    {
        var rating = await ratingService.Update(dto).ConfigureAwait(false);

        if (rating == null)
        {
            return BadRequest("Can't change Rating with such parameters\n" +
                              "Please check that id, entity, parent, type information are valid and exist in the system.");
        }

        await this.UpdateWorkshopInElasticSearch(dto.EntityId).ConfigureAwait(false);

        return Ok(rating);
    }

    /// <summary>
    /// Delete a specific Rating entity from the database.
    /// </summary>
    /// <param name="id">Rating's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var ratingDto = await ratingService.GetById(id).ConfigureAwait(false);
        if (ratingDto is null)
        {
            return NoContent();
        }

        await ratingService.Delete(id).ConfigureAwait(false);

        await UpdateWorkshopInElasticSearch(ratingDto.EntityId).ConfigureAwait(false);

        return NoContent();
    }

    private async Task<bool> UpdateWorkshopInElasticSearch(Guid id)
    {
        try
        {
            var ratingDto = await averageRatingService.GetByEntityIdAsync(id)
                .ConfigureAwait(false);

            var rating = ratingDto?.Rate ?? default;

            return await esWorkshopService.PartialUpdate(id, new WorkshopRatingES { Rating = rating })
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while updating rating in ElasticSearch: {ex.Message}");
            return false;
        }
    }
}