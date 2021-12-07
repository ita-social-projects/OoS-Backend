using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class FeatureManagementController : ControllerBase
    {
        private readonly FeatureManagementConfig featureManagementConfig;

        public FeatureManagementController(IOptions<FeatureManagementConfig> featureManagementConfig)
        {
            this.featureManagementConfig = featureManagementConfig.Value;

        }

        /// <summary>
        /// Get enables feature flags depending on releases.
        /// </summary>
        /// <returns>List of releases.</returns>
        /// <response code="200">All entities were found.</response>
        /// <response code="204">No entity was found.</response>
        /// <response code="500">If any server error occures.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FeatureManagementConfig))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public IActionResult Get()
        {
            var isEnabledRelease = featureManagementConfig;

            return Ok(isEnabledRelease);
        }
    }
}
