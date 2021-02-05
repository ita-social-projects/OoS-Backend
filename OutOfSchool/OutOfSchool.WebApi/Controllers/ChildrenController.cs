﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class ChildrenController : ControllerBase
    {
        private IChildService childService;

        public ChildrenController(IChildService childService)
        {
            this.childService = childService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Child>>> GetChildren()
        {

            
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult<Child>> CreateChildren(ChildDTO childDTO)
        {
            ChildDTO child;
            try
            {
                child = await childService.Create(childDTO);
            }
            catch
            {
                return BadRequest();
            }

            return CreatedAtAction(
                nameof(GetChildren),
                new { id = child.ChildId },
                child);
        }
    }
}
