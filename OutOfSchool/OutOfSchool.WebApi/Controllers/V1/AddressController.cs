﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService addressService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressController"/> class.
        /// </summary>
        /// <param name="addressService">Service for Address model.</param>
        /// <param name="localizer">Localizer.</param>
        public AddressController(IAddressService addressService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.addressService = addressService;
        }

        /// <summary>
        /// Get all addresses from the database.
        /// </summary>
        /// <returns>List of all addresses.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddressDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddresses()
        {
            return Ok(await addressService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get address by it's key.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>Address element with some id.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddressDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAddressById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await addressService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Create new address.
        /// </summary>
        /// <param name="addressDto">Element which must be added.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.AddressAddNew)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(AddressDto addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (addressDto == null)
            {
                return BadRequest("Address is null.");
            }

            try
            {
                AddressDto address = await addressService.Create(addressDto).ConfigureAwait(false);
                return CreatedAtAction(
                    nameof(GetAddressById),
                    new
                    {
                        id = address.Id,
                    }, address);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update info about some address in database.
        /// </summary>
        /// <param name="addressDto">Entity.</param>
        /// <returns>Address key.</returns>
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.AddressEdit)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddressDto))]
        public async Task<IActionResult> Update(AddressDto addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await addressService.Update(addressDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Address entity from the database.
        /// </summary>
        /// <param name="id">Address key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        // [Authorize(Roles = "provider,admin")]
        [HasPermission(Permissions.AddressRemove)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            await addressService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}
