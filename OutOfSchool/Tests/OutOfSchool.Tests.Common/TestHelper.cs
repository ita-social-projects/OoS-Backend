using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Tests.Common
{
    public static class TestHelper
    {
        public static void GetAssertedResponseOkAndValidValue<TExpectedValue>(this IActionResult response)
        {
            Assert.IsInstanceOf<OkObjectResult>(response);
            var okResult = response as OkObjectResult;
            Assert.IsInstanceOf<TExpectedValue>(okResult.Value);
            Assert.That(okResult.Value, Is.Not.Null);
        }

        public static void GetAssertedResponseValidateValueNotEmpty<TExpectedResponseType>(this IActionResult response)
        {
            Assert.IsInstanceOf<TExpectedResponseType>(response);
            var objectResult = response as ObjectResult;
            Assert.That(objectResult.Value, Is.Not.Null);
        }
    }
}
