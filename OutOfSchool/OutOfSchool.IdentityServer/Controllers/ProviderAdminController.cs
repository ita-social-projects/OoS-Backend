using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public ProviderAdminController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<ResponseDto> Create(CreateUserDto createUserDto)
        {
            //TODO:
            // Let's think about expediency of using mapper here.

            var user = new User()
            {
                UserName = createUserDto.Email,
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.PhoneNumber,
                Role = createUserDto.Role,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                MiddleName = createUserDto.MiddleName,
                CreatingTime = DateTimeOffset.UtcNow,
                IsRegistered = false,
            };

            IdentityResult result = await userManager.CreateAsync(user);

            ResponseDto responseDto = new ResponseDto();

            if (result.Succeeded)
            {
                createUserDto.UserId = user.Id;

                return new ResponseDto()
                {
                    IsSuccess = true,
                    Result = createUserDto,
                };
            }

            return new ResponseDto()
            {
                IsSuccess = false,
                ErrorMessages =
                    result.Errors.Select(error => error.Description),
            };
        }
    }
}