using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.AuthCommon.Controllers;
public class PrivacyController : Controller
{
    private readonly ILogger<PrivacyController> logger;

    public PrivacyController(ILogger<PrivacyController> logger)
    {
        this.logger = logger;
    }

    public IActionResult PersonalData()
    {
        logger.LogInformation("The consent to processing of the personal data page was shown.");

        return View();
    }

    public IActionResult ParentTerms()
    {
        logger.LogInformation("The parent's terms of use page was shown.");

        return View();
    }

    public IActionResult ProviderTerms()
    {
        logger.LogInformation("The provider's terms of use page was shown.");

        return View();
    }
}
