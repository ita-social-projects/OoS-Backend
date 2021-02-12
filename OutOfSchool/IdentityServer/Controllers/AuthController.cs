using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IIdentityServerInteractionService interactionService)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _interactionService = interactionService;
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();

            var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);

            if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                throw new NotImplementedException();
            }

            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "Login")
        {
            var externalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = externalProviders
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(new LoginViewModel
                {
                    ExternalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync()
                });
            }

            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);

            if (result.Succeeded)
            {
                return Redirect(vm.ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
                return BadRequest();
            }
            else
            {
                ModelState.AddModelError("", "Login or password is wrong");
                
                return View(new LoginViewModel
                {
                    ExternalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync()
                });
            }
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = "Login")
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl, AllRoles = _roleManager.Roles.ToList() });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllRoles = _roleManager.Roles.ToList();
                return View(vm);
            }

            var user = new User()
            {
                UserName = vm.Username,
                PhoneNumber = vm.PhoneNumber,
                CreatingTime = DateTime.Now
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            var selectedRole = _roleManager.Roles.First(role => role.Id == vm.UserRoleId).Name;
            if (result.Succeeded)
            {
                var resultRoleAssign = await _userManager.AddToRoleAsync(user, selectedRole);
                if (resultRoleAssign.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);

                    return Redirect(vm.ReturnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(vm);
        }
        
        public Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            throw new NotImplementedException();
        }
    }
}
