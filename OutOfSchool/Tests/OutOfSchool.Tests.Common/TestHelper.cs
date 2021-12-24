using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OutOfSchool.Tests.Common
{
    public static class TestHelper
    {
        public static void AssertResponseOkResultAndValidateValue<TExpectedValue>(this IActionResult response, TExpectedValue expected)
        {
            var actual = (response as ObjectResult).Value;
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<OkObjectResult>(response);
                Assert.IsInstanceOf<TExpectedValue>(actual);
                AssertDtosAreEqual(expected, (TExpectedValue)actual);
            });
        }

        public static void AssertResponseOkResultAndValidateValue<TExpectedValue>(this IActionResult response, IEnumerable<TExpectedValue> expected)
        {
            var actual = (response as ObjectResult).Value;
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<OkObjectResult>(response);
                Assert.IsInstanceOf<IEnumerable<TExpectedValue>>(actual);
                AssertTwoCollectionsEqualByValues(expected, (IEnumerable<TExpectedValue>)actual);
            });

        }

        public static void AssertExpectedResponseTypeAndCheckDataInside<TExpectedResponseType>(this IActionResult response, ObjectResult expected)
        {
            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<TExpectedResponseType>(response);
                var objectResult = response as ObjectResult;
                var type = expected.Value.GetType();
                Assert.That(objectResult.Value.GetType(), Is.EqualTo(type));
                Assert.That(objectResult.Value, Is.Not.Null);
            });
        }

        public static void AssertTwoCollectionsEqualByValues<TValue>(IEnumerable<TValue> expected, IEnumerable<TValue> actual)
        {
            Assert.Multiple(() =>
            {
                foreach (var collection in expected.Zip(actual))
                {
                    AssertDtosAreEqual(collection.First, collection.Second);
                }
            }
            );
        }

        public static void AssertDtosAreEqual<TValue>(TValue expected, TValue actual)
        {
            var tuppledProperties = GetTuppledProperties<TValue>(expected, actual);
            tuppledProperties.AssertPropertiesAreEqual();
        }

        private static IEnumerable<(object , object)> GetTuppledProperties<TValue>(TValue expected, TValue actual)
        {
            return expected.GetType().GetProperties()
                .Select(p => p.GetValue(expected))
                .Zip(actual.GetType().GetProperties()
                .Select(r => r.GetValue(actual)));
        }

        private static void AssertPropertiesAreEqual(this IEnumerable<(object, object)> tuppledProperties)
        {
            Assert.Multiple(() =>
            {
                foreach (var property in tuppledProperties)
                {
                    Assert.AreEqual(property.Item1, property.Item2);
                }
            });
        }
    }
}
