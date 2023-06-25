using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers;
public class GDPRController : Controller
{
    public IActionResult PersonalData()
    {
        return View();
    }

    public IActionResult ParentTerms()
    {
        return View();
    }

    public IActionResult ProviderTerms()
    {
        return View();
    }
}
