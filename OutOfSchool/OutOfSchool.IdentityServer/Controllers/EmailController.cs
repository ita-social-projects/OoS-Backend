using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.EmailService;
using OutOfSchool.IdentityServer.ViewModels;

namespace OutOfSchool.IdentityServer.Controllers
{
    public class EmailController : Controller
    {
        private readonly ILogger<EmailController> logger;
        private readonly IEmailSender emailSender;

        public EmailController(
            ILogger<EmailController> logger,
            IEmailSender emailSender)
        {
            this.logger = logger;
            this.emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Change()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Change(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new ChangeEmailViewModel());
            }

            var callBackUrl = Url.Action(nameof(ConfirmChange), "Email", new { code = "777" }, Request.Scheme);

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
                        Name = model.Email,
                        Address = model.Email,
                    },
                },
                Content = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.",
                Subject = "Test",
            };
            await emailSender.SendAsync(emailMessage);
        
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmChange(string code)
        {
            return View();
        }
    }
}
