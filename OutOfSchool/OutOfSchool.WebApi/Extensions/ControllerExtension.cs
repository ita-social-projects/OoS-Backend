using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ControllerExtension
    {
        public static string GetJwtClaimByName(this ControllerBase controller, string claimName)
        {
            ExtensionValidation(controller);
            return controller.User.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
        }

        private static void ExtensionValidation(ControllerBase controller)
        {
            if (controller == null)
            {
                throw new ArgumentException("Controller cannot be null", nameof(controller));
            }
        }
    }
}
