using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public class PersonalInfoController : ControllerBase
{
    private const string GetPersonalInfoActionName = "GetPersonalInfo";
    private const string UpdatePersonalInfoActionName = "UpdatePersonalInfo";
    private readonly IUserService userService;
    private readonly IParentService parentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalInfoController"/> class.
    /// </summary>
    /// <param name="userService">Service for interacting with users.</param>
    /// <param name="parentService">Service for interacting with parents.</param>
    public PersonalInfoController(IUserService userService, IParentService parentService)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
    }

    /// <summary>
    /// Get user by it's key.
    /// </summary>
    /// <returns>User element with some id.</returns>
    [Route($"Parent/{GetPersonalInfoActionName}")]
    [HasPermission(Permissions.ParentRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentPersonalInfo))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetParentPersonalInfo()
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);

        var info = await parentService.GetPersonalInfoByUserId(userId);

        return info is null ? NoContent() : Ok(info);
    }

    /// <summary>
    /// Get user by it's key.
    /// </summary>
    /// <returns>User element with some id.</returns>
    [Route($"Provider/{GetPersonalInfoActionName}")]
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderPersonalInfo() => await GetUserPersonalInfo();

    /// <summary>
    /// Get user by it's key.
    /// </summary>
    /// <returns>User element with some id.</returns>
    [Route($"ProviderAdmin/{GetPersonalInfoActionName}")]
    [HasPermission(Permissions.ProviderAdmins)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviderAdminPersonalInfo() => await GetUserPersonalInfo();

    /// <summary>
    /// Get user by it's key.
    /// </summary>
    /// <returns>User element with some id.</returns>
    [Route($"MinistryAdmin/{UpdatePersonalInfoActionName}")]
    [HasPermission(Permissions.MinistryAdminRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMinistryAdminPersonalInfo() => await GetUserPersonalInfo();

    /// <summary>
    /// Update info about the User.
    /// </summary>
    /// <param name="dto">Entity to update.</param>
    /// <returns>Updated Provider.</returns>
    [Route($"Parent/{UpdatePersonalInfoActionName}")]
    [HasPermission(Permissions.ParentEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentPersonalInfo))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateParentPersonalInfo(ParentPersonalInfo dto) => Ok(await parentService.Update(dto));

    /// <summary>
    /// Update info about the User.
    /// </summary>
    /// <param name="dto">Entity to update.</param>
    /// <returns>Updated Provider.</returns>
    [Route($"Provider/{UpdatePersonalInfoActionName}")]
    [HasPermission(Permissions.ProviderEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProviderPersonalInfo(ShortUserDto dto) => Ok(await userService.Update(dto));

    /// <summary>
    /// Update info about the User.
    /// </summary>
    /// <param name="dto">Entity to update.</param>
    /// <returns>Updated Provider.</returns>
    [Route($"ProviderAdmin/{UpdatePersonalInfoActionName}")]
    [HasPermission(Permissions.ProviderAdmins)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProviderAdminPersonalInfo(ShortUserDto dto) => Ok(await userService.Update(dto));

    /// <summary>
    /// Update info about the User.
    /// </summary>
    /// <param name="dto">Entity to update.</param>
    /// <returns>Updated Provider.</returns>
    [Route($"MinistryAdmin/{GetPersonalInfoActionName}")]
    [HasPermission(Permissions.MinistryAdminEdit)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMinistryAdminPersonalInfo(ShortUserDto dto) => Ok(await userService.Update(dto));

    private async Task<IActionResult> GetUserPersonalInfo()
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);

        var info = await userService.GetById(userId);

        return info is null ? NoContent() : Ok(info);
    }
}