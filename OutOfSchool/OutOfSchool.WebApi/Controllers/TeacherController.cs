using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models.ModelsDto;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer ")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        
        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }
        [HttpPost]
        public async Task<ActionResult<Teacher>> Create([FromBody] TeacherDto teacher)
        {
            if (ModelState.IsValid)
            {
                return await _teacherService.CreateAsync(teacher);
            }

            return BadRequest(ModelState);
        }
    }
}