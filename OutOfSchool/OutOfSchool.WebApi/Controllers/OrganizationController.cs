using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> _logger;
        private readonly OutOfSchoolDbContext _context;

        public OrganizationController(ILogger<OrganizationController> logger, OutOfSchoolDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult TestOk()
        {
            var userId = User.FindFirst("sub")?.Value;
            var result =  _context.Users.FirstOrDefaultAsync(user => user.Id == userId).Result;
            var username = result.UserName;
            return this.Ok("Hello world "+username);
        }
    }
}
