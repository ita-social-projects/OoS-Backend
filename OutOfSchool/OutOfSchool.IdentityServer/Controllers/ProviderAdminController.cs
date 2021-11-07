using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private ResponseDto response;

        public ProviderAdminController(UserManager<User> userManager)
        {
            this.userManager = userManager;
            this.response = new ResponseDto();
        }

        [HttpPost]
        [Authorize(Roles = "provider, provideradmin")]
        public async Task<ResponseDto> Create(CreateUserDto createUserDto)
        {
            // TODO:
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

            var password = PasswordGenerator
                .GenerateRandomPassword(userManager.Options.Password);

            IdentityResult result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                createUserDto.UserId = user.Id;

                response.IsSuccess = true;
                response.Result = createUserDto;

                // send  activating mail to user here

                return response;
            }

            response.IsSuccess = false;
            response.ErrorMessages =
                    result.Errors.Select(error => error.Description);

            return response;
        }
    }
}