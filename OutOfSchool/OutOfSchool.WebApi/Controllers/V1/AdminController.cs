using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public AdminController(IUserService userService, IMapper mapper)
        {
            this.userService = userService;
            this.mapper = mapper;
        }

        /// <summary>
        /// To Get the Profile of authorized Admin.
        /// </summary>
        /// <returns>Authorized admin's profile.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminDto>> GetProfile()
        {
            #warning This controller method is here to test UI Admin features. Will be removed or refactored
            try
            {
                string userId = User.FindFirst("sub")?.Value;
                var shortUser = await userService.GetById(userId).ConfigureAwait(false);
                return Ok(mapper.Map<AdminDto>(shortUser));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}