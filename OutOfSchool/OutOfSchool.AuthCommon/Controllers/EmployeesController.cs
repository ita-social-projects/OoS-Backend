using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class EmployeesController : Controller
{
    private readonly ILogger<EmployeesController> logger;
    private readonly IEmployeeService employeeService;

    private string userId;

    public EmployeesController(
        ILogger<EmployeesController> logger,
        IEmployeeService employeeService)
    {
        this.logger = logger;
        this.employeeService = employeeService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    [HttpPost]
    [HasPermission(Permissions.Employees)]
    public async Task<ResponseDto> Create(CreateEmployeeDto employeeDto)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .CreateEmployeeAsync(employeeDto, Url, userId);
    }

    [HttpPut("{employeeId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Update(string employeeId, UpdateEmployeeDto employeeUpdateDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            userId);

        return await employeeService
            .UpdateEmployeeAsync(employeeUpdateDto, userId);
    }

    [HttpDelete("{employeeId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Delete(string employeeId)
    {
        if (employeeId is null)
        {
            throw new ArgumentNullException(nameof(employeeId));
        }

        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .DeleteEmployeeAsync(employeeId, userId);
    }

    [HttpPut("{employeeId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Block(string employeeId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .BlockEmployeeAsync(employeeId, userId, isBlocked);
    }

    [HttpPut("{providerId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderBlock)]
    public async Task<ResponseDto> BlockByProvider(Guid providerId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .BlockEmployeesByProviderAsync(providerId, userId, isBlocked);
    }

    [HttpPut("{employeeId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Reinvite(string employeeId)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .ReinviteEmployeeAsync(employeeId, userId, Url);
    }
}