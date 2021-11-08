using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : ControllerBase
    {
        private readonly IProviderAdminService providerAdminService;

        public ProviderAdminController(IProviderAdminService providerAdminService)
        {
            this.providerAdminService = providerAdminService;
        }

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "provider,provideradmin")]
        [HttpPost]
        public async Task<ActionResult> Create(ProviderAdminDto providerAdmin)
        {
            if (ModelState.IsValid)
            {
                var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

                var response = await providerAdminService.CreateProviderAdminAsync(
                    userId,
                    providerAdmin,
                    await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
                        .ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    return Ok((ProviderAdminDto)response.Result);
                }
            }

            return BadRequest();
        }
    }
}
