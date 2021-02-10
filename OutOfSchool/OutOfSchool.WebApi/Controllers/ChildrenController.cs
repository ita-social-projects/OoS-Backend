using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    /// <summary>
    /// Controller with CRUD operations for Child entity.
    /// </summary>
    public class ChildrenController : ControllerBase
    {
        private IChildService childService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildrenController"/> class.
        /// </summary>
        /// <param name="childService">Service for Child model.</param>
        public ChildrenController(IChildService childService)
        {
            this.childService = childService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Child>>> GetChildren()
        {
            return this.Ok();
        }

        /// <summary>
        /// Method for create new child.
        /// </summary>
        /// <param name="childDTO">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<Child>> CreateChild(ChildDTO childDTO)
        {
            ChildDTO child;
            try
            {
                child = await this.childService.Create(childDTO).ConfigureAwait(false);
            }
            catch (ArgumentNullException ex)
            {
                return this.BadRequest();
            }
            catch (ArgumentException ex)
            {
                return this.BadRequest();
            }

            return this.CreatedAtAction(
                nameof(this.GetChildren),
                new { id = child.Id },
                child);
        }
    }
}
