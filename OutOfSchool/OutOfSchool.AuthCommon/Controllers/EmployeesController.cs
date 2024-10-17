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

    [HttpPut("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Update(string providerAdminId, UpdateEmployeeDto employeeUpdateDto)
    {
        logger.LogDebug(
            "Operation initiated by User(id): {UserId}",
            userId);

        return await employeeService
            .UpdateEmployeeAsync(employeeUpdateDto, userId);
    }

    [HttpDelete("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Delete(string providerAdminId)
    {
        if (providerAdminId is null)
        {
            throw new ArgumentNullException(nameof(providerAdminId));
        }

        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .DeleteEmployeeAsync(providerAdminId, userId);
    }

    [HttpPut("{providerAdminId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Block(string providerAdminId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .BlockEmployeeAsync(providerAdminId, userId, isBlocked);
    }

    [HttpPut("{providerId}/{isBlocked}")]
    [HasPermission(Permissions.ProviderBlock)]
    public async Task<ResponseDto> BlockByProvider(Guid providerId, bool isBlocked)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .BlockEmployeesByProviderAsync(providerId, userId, isBlocked);
    }

    [HttpPut("{providerAdminId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> Reinvite(string providerAdminId)
    {
        logger.LogDebug("Operation initiated by User(id): {UserId}", userId);

        return await employeeService
            .ReinviteEmployeeAsync(providerAdminId, userId, Url);
    }
}