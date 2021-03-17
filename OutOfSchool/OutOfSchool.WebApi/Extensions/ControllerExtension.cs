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
            return controller.User.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
        }
    }
}
