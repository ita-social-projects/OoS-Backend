using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Util
{
    public static class ControllerExtension
    {
        public static string GetJwtClaimByName(this ControllerBase controller, string claimName)
        {
            return controller.User.Claims.FirstOrDefault(c => c.Type == claimName)?.Value;
        }
    }
}
