using System;
using System.Threading.Tasks;
using IdentityServer.Controllers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IIdentityServerInteractionService _interactionService;

        public AuthController(UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              IIdentityServerInteractionService interactionService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _interactionService = interactionService;
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();

            var logoutRequest = await _interactionService.GetLogoutContextAsync(logoutId);

            // if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            // {
            //     return RedirectToAction("Index", "Home");
            // }

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
            // check if the model is valid

            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);

            if (result.Succeeded)
            {
                return Redirect(vm.ReturnUrl);
            }
            else if (result.IsLockedOut)
            {
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register(string returnUrl = "Login")
        {
            return View(new RegisterViewModel {ReturnUrl = returnUrl});
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = new User()
            {
                UserName = vm.Username,
                PhoneNumber = vm.PhoneNumber,
                CreatingTime = DateTime.Now
            };
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);

                return Redirect(vm.ReturnUrl);
            }

            return View();
        }
    }
}