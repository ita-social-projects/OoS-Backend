using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static void IdValidation(this ControllerBase controller, long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "The Id cannot be less than 1.");
            }
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
