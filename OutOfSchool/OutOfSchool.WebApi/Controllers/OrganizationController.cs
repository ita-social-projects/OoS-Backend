
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(ILogger<OrganizationController> logger)
        {
            _logger = logger;
        }

        public IActionResult TestOk()
        {
            return this.Ok("Hello world");
        }

    }
}
