using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Base Controller with CRUD operations for CompanyInformation entity.
/// </summary>
public abstract class CompanyInformationController : ControllerBase
{
    private readonly ICompanyInformationService companyInformationService;
    private readonly IStringLocalizer<SharedResource> localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyInformationController"/> class.
    /// </summary>
    /// <param name="companyInformationService">Service for CompanyInformation model.</param>
    /// <param name="localizer">Localizer.</param>
    protected CompanyInformationController(
        ICompanyInformationService companyInformationService,
        IStringLocalizer<SharedResource> localizer)
    {
        this.companyInformationService = companyInformationService ?? throw new ArgumentNullException(nameof(companyInformationService));
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    /// <summary>
    /// Gets CompanyInformation type property.
    /// </summary>
    protected abstract CompanyInformationType Type { get; }

    /// <summary>
    /// Get a part of a CompanyInformation from the database by Type.
    /// </summary>
    /// <returns>Information about CompanyInformation by Type.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyInformationDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var companyInformation = await this.companyInformationService.GetByType(this.Type).ConfigureAwait(false);

        if (companyInformation == null)
        {
            return this.NoContent();
        }

        return this.Ok(companyInformation);
    }

    /// <summary>
    /// Update a part of a CompanyInformation by Type.
    /// </summary>
    /// <param name="companyInformationModel">Entity to update.</param>
    /// <returns>Updated information about CompanyInformation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyInformationDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(CompanyInformationDto companyInformationModel)
    {
        if (!ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        CompanyInformationDto companyInformation;
        try
        {
            companyInformation = await this.companyInformationService.Update(companyInformationModel, this.Type).ConfigureAwait(false);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }

        if (companyInformation == null)
        {
            return this.BadRequest(this.localizer["Cannot change information about {0}.\n Please check information is valid.", this.Type]);
        }

        return this.Ok(companyInformation);
    }
}