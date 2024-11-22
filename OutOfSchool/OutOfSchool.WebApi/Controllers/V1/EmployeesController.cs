using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[HasPermission(Permissions.Employees)]
public class EmployeesController : Controller
{
    private readonly IEmployeeService employeeService;
    private readonly IUserService userService;
    private readonly IProviderService providerService;
    private readonly ILogger<EmployeesController> logger;
    private string path;    
    private string userId;

    public EmployeesController(
        IEmployeeService employeeService,
        IUserService userService,
        IProviderService providerService,
        ILogger<EmployeesController> logger)
    {
        this.employeeService = employeeService;
        this.userService = userService;
        this.providerService = providerService;
        this.logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        userId = GettingUserProperties.GetUserId(User);
    }

    /// <summary>
    /// Method for creating new Employee.
    /// </summary>
    /// <param name="employee">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateEmployeeDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEmployeeDto employee)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (employee == null)
        {
            return BadRequest("Employee is null.");
        }

        if (await IsProviderBlocked(employee.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to create the employee at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to create the employee by the blocked provider.");
        }

        if (!ModelState.IsValid)
        {
            logger.LogError($"Input data was not valid for User(id): {userId}");

            return StatusCode(StatusCodes.Status422UnprocessableEntity);
        }

        var response = await employeeService.CreateEmployeeAsync(
                userId,
                employee,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, new { error.Message, error.ApiErrorResponse }),
            result =>
            {
                logger.LogInformation("Successfully created Employee(id): {result_UserId} by User(id): {UserId}", result.UserId, userId);
                return Created(string.Empty, result);
            });
    }

    /// <summary>
    /// Update info about the Employee.
    /// </summary>
    /// <param name="providerId">Employee's id for which operation perform.</param>
    /// <param name="employeeModel">Entity to update.</param>
    /// <returns>Updated Employee.</returns>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(Guid providerId, [FromBody] UpdateEmployeeDto employeeModel)
    {
        if (employeeModel == null)
        {
            return BadRequest("Employee is null.");
        }

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to update the Employee at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to update the Employee by the blocked provider.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await employeeService.UpdateEmployeeAsync(
                employeeModel,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

            return response.Match(
                error => StatusCode((int)error.HttpStatusCode),
                _ =>
                {
                    logger.LogInformation($"Can't change Employee with such parameters.\n" +
                        "Please check that information are valid.");

                    return Ok();
                });
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Method for deleting Employee.
    /// </summary>
    /// <param name="employeeId">Entity's id to delete.</param>
    /// <param name="providerId">Provider's id for which operation perform.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete]
    public async Task<ActionResult> Delete(string employeeId, Guid providerId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to delete the Employee at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to delete the Employee by the blocked provider.");
        }

        var response = await employeeService.DeleteEmployeeAsync(
                employeeId,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Succesfully deleted Employee(id): {employeeId} by User(id): {userId}.");

                return Ok();
            });
    }

    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut]
    public async Task<ActionResult> Block(string employeeId, Guid providerId, bool? isBlocked)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (isBlocked is null)
        {
            logger.LogDebug("IsBlocked parameter is not specified");
            return BadRequest("IsBlocked parameter is required");
        }

        if (await IsProviderBlocked(providerId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to block the Employee at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to block the Employee by the blocked provider.");
        }

        var response = await employeeService.BlockEmployeeAsync(
                employeeId,
                userId,
                providerId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false),
                (bool)isBlocked)
            .ConfigureAwait(false);

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Successfully blocked Employee(id): {employeeId} by User(id): {userId}.");

                return Ok();
            });
    }

    /// <summary>
    /// Method to Get filtered data about related Employees.
    /// </summary>
    /// <param name="filter">Filter to get a part of all employees that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<EmployeeDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    public async Task<IActionResult> GetFilteredProviderAdminsAsync([FromQuery] EmployeeSearchFilter filter)
    {
        var relatedAdmins = await employeeService.GetFilteredRelatedProviderAdmins(userId, filter).ConfigureAwait(false);
    
        return this.SearchResultToOkOrNoContent(relatedAdmins);
    }

    /// <summary>
    /// Method to Get data about related Employees.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EmployeeDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> GetRelatedEmployees()
    {
        var relatedEmployees = await employeeService.GetRelatedEmployees(userId).ConfigureAwait(false);

        if (!relatedEmployees.Any())
        {
            return NoContent();
        }

        return Ok(relatedEmployees);
    }

    /// <summary>
    /// Method to Get data about managed Workshops.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopProviderViewCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> ManagedWorkshops()
    {
        var userRole = GettingUserProperties.GetUserRole(HttpContext);

        if (userRole is not Role.Employee)
        {
            return BadRequest();
        }

        var relatedWorkshops = await employeeService.GetWorkshopsThatEmployeeCanManage(userId).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(relatedWorkshops);
    }

    /// <summary>
    /// Get Employee by its id.
    /// </summary>
    /// <param name="employeeId">Employee's id.</param>
    /// <returns>Info about Employee.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FullEmployeeDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetEmployeeById(string employeeId)
    {
        var employee = await employeeService.GetFullEmployee(employeeId)
            .ConfigureAwait(false);
        if (employee == null)
        {
            return NoContent();
        }

        return Ok(employee);
    }

    /// <summary>
    /// Send new invitation to Employee.
    /// </summary>
    /// <param name="employeeId">Employee's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{employeeId}")]
    public async Task<IActionResult> Reinvite(string employeeId)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to reinvite the employee by the blocked provider.");
        }

        var response = await employeeService.ReinviteEmployeeAsync(
                employeeId,
                userId,
                await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        if (response == null)
        {
            return NoContent();
        }

        return response.Match(
            error => StatusCode((int)error.HttpStatusCode),
            _ =>
            {
                logger.LogInformation($"Succesfully reinvited employee(id): {employeeId} by User(id): {userId}.");

                return Ok();
            });
    }

    private async Task<bool> IsCurrentUserBlocked() =>
        await userService.IsBlocked(userId);

    private async Task<bool> IsProviderBlocked(Guid providerId) =>
        await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false;

}
