using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/users/personalinfo")]
[HasPermission(Permissions.PersonalInfo)]
public class PersonalInfoController : ControllerBase
{
    private readonly IUserService userService;
    private readonly IParentService parentService;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalInfoController"/> class.
    /// </summary>
    /// <param name="userService">Service for interacting with users.</param>
    /// <param name="parentService">Service for interacting with parents.</param>
    /// <param name="currentUserService">Service for interacting with current user.</param>
    public PersonalInfoController(
        IUserService userService,
        IParentService parentService,
        ICurrentUserService currentUserService)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.parentService = parentService ?? throw new ArgumentNullException(nameof(parentService));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Gets User's personal information.
    /// </summary>
    /// <returns>User's personal information.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPersonalInfo()
    {
        var info = currentUserService.IsInRole(Role.Parent)
            ? await parentService.GetPersonalInfoByUserId(currentUserService.UserId)
            : await userService.GetById(currentUserService.UserId);

        if (string.IsNullOrWhiteSpace(info.Role))
        {
            info.Role = currentUserService.UserRole;
        }
        return Ok(info);
    }


    /// <summary>
    /// Updates User's personal information.
    /// </summary>
    /// <param name="dto">New User's personal information.</param>
    /// <returns>Updated User's personal information.</returns>
    [HttpPut]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShortUserDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] ShortUserDto dto)
    {
        ShortUserDto result;
        if (currentUserService.IsInRole(Role.Parent))
        {
            result = await parentService.Update(dto);
        }
        else
        {
            if (currentUserService.UserId != dto.Id)
            {
                throw new UnauthorizedAccessException("Forbidden to update another user");
            }

            result = await userService.Update(dto);
        }

        return Ok(result);
    }
}