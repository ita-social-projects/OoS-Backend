using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class ChildrenController : ControllerBase
    {
        private ChildService childService;

        public ChildrenController(OutOfSchoolDbContext outOfSchoolDbContext)
        {
            childService = new ChildService(outOfSchoolDbContext);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Child>>> GetChildren()
        {

            
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<Child>> CreateChildren(Child child)
        {
            var newChild = await childService.Create(child);

            return CreatedAtAction(
                nameof(GetChildren),
                new { id = child.ChildId },
                child);
        }
    }
}
