using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for parent entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ParentController : ControllerBase
    {
        private readonly IParentService serviceParent;
        private readonly IApplicationService serviceApplication;
        private readonly IChildService serviceChild;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParentController"/> class.
        /// Initialization of ParentController.
        /// </summary>
        /// <param name="serviceParent">Parent service for ParentController.</param>
        /// <param name="serviceApplication">Application service for ParentController.</param>
        /// <param name="serviceChild">Child service for ParentController.</param>
        /// <param name="localizer">Localizer.</param>
        public ParentController(IParentService serviceParent, IApplicationService serviceApplication, IChildService serviceChild, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.serviceParent = serviceParent;
            this.serviceApplication = serviceApplication;
            this.serviceChild = serviceChild;
        }

        /// <summary>
        /// To get all Parents from DB.
        /// </summary>
        /// <returns>List of Parents.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ParentDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var parents = await serviceParent.GetAll().ConfigureAwait(false);

            if (!parents.Any())
            {
                return NoContent();
            }

            return Ok(parents);
        }

        /// <summary>
        /// To get information about workshops that parent applied child for.
        /// </summary>
        /// <returns>List of ParentCardDto.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ParentCard>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChildrenWorkshops()
        {
            try
            {
                string userId = User.FindFirst("sub")?.Value;

                var parent = await serviceParent.GetByUserId(userId).ConfigureAwait(false);

                var children = await serviceChild.GetByUserId(userId, new OffsetFilter() { From = 0, Size = int.MaxValue}).ConfigureAwait(false);

                var cards = new List<ParentCard>();

                foreach (var child in children.Entities)
                {
                    var applications = await serviceApplication.GetAllByChild(child.Id).ConfigureAwait(false);

                    foreach (var application in applications)
                    {
                        cards.Add(application.ToCard());
                    }
                }

                return Ok(cards);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// To recieve parent with define id.
        /// </summary>
        /// <param name="id">Key in table.</param>
        /// <returns>Parent with define id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await serviceParent.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To update Parent entity that already exists.
        /// </summary>
        /// <param name="shortUserDto">ShortUserDto object with new properties.</param>
        /// <returns>Parent's key.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(ShortUserDto shortUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = User.FindFirst("sub")?.Value;

            if (userId != shortUserDto.Id)
            {
                return StatusCode(403, "Forbidden to update another user.");
            }

            return Ok(await serviceParent.Update(shortUserDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete Parent entity from DB.
        /// </summary>
        /// <param name="id">The key in table.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            var parent = await serviceParent.GetById(id).ConfigureAwait(false);

            if (parent is null)
            {
                return NoContent();
            }

            var userHasRights = await IsUserParentProfileOwner(parent.Id).ConfigureAwait(false);

            if (!userHasRights)
            {
                return StatusCode(403, "Forbidden to delete another parent account.");
            }

            await serviceParent.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// To Get the Profile of authorized Parent.
        /// </summary>
        /// <returns>Authorized parent's profile.</returns>
        [Authorize(Roles = "parent,admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ParentDTO>> GetProfile()
        {
            try
            {
                string userId = User.FindFirst("sub")?.Value;
                var parents = await serviceParent.GetAll().ConfigureAwait(false);
                var parentDTO = parents.FirstOrDefault(x => x.UserId == userId);
                return this.Ok(parentDTO);
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest(ex.Message);
            }
        }

        private async Task<bool> IsUserParentProfileOwner(long parentId)
        {
            // Parent can create/update/delete his/her account only if parent is the owner.
            // Admin can manipulate data without checks.
            if (User.IsInRole("parent"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var parent = await serviceParent.GetByUserId(userId).ConfigureAwait(false);

                if (parentId != parent.Id)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
