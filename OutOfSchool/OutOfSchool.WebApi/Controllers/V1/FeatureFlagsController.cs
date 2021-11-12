using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.WebApi.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class FeatureFlagsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FeatureFlagsController(IConfiguration configuration, ILogger<FeatureFlagsController> logger)
        {
            _config = configuration;
        }

        /// <summary>
        /// Get enables feature flags depending on releases.
        /// </summary>
        /// <returns>List of releases.</returns>
        /// <response code="200">All entities were found.</response>
        /// <response code="204">No entity was found.</response>
        /// <response code="500">If any server error occures.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public IActionResult Get()
        {
            var releases = _config.GetSection("FeatureManagement").GetChildren().Where(x => x.Value.Equals("True")).ToList();
            return Ok(releases);
        }
    }
}