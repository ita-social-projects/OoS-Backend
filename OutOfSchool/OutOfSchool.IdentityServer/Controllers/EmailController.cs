using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.EmailService;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    public class EmailController : Controller
    {
        private readonly ILogger<EmailController> logger;
        private readonly UserManager<User> userManager;
        private readonly IEmailSender emailSender;        

        public EmailController(
            ILogger<EmailController> logger,
            IEmailSender emailSender,
            UserManager<User> userManager)
        {
            this.logger = logger;
            this.emailSender = emailSender;
            this.userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Change()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Change(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new ChangeEmailViewModel());
            }

            var user = await userManager.FindByEmailAsync(User.Identity.Name);
            var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var callBackUrl = Url.Action(nameof(ConfirmChange), "Email", new { token, email = model.CurrentEmail, newEmail = model.Email }, Request.Scheme);

            var emailMessage = new EmailMessage
            {
                FromAddresses = new List<EmailAddress>()
                {
                    new EmailAddress()
                    {
                        Name = "Oos-Backend",
                        Address = "OoS.Backend.Test.Server@gmail.com",
                    },
                },
                ToAddresses = new List<EmailAddress>()
                {
                    new EmailAddress()
                    {
                        Name = model.CurrentEmail,
                        Address = model.CurrentEmail,
                    },
                },
                Content = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.",
                Subject = "Confirm email.",
            };
            await emailSender.SendAsync(emailMessage);
        
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmChange(string token, string email, string newEmail)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return View("Error");
            }

            var result = await userManager.ChangeEmailAsync(user, newEmail, token);
            if (result.Succeeded)
            {
                return View();
            }
            else
            {
                return View("Error");
            }            
        }
    }
}
