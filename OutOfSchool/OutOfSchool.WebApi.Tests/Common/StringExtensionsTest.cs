using NUnit.Framework;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.WebApi.Tests.Common
{
    [TestFixture]
    public class StringExtensionsTest
    {
        private readonly string enumNull = null;
        private readonly string enumEmpty = string.Empty;
        private readonly string enumString = "Example";
        private readonly string notEnumString = "Hello";

        [Test]
        public void IfNull_ReturnDefault()
        {
            var result = enumNull.ToEnum(Test.Default);
            Assert.AreEqual(Test.Default, result);
        }

        [Test]
        public void IfEmpty_ReturnDefault()
        {
            var result = enumEmpty.ToEnum(Test.Default);
            Assert.AreEqual(Test.Default, result);
        }

        [Test]
        public void IfNotExistValue_ReturnDefault()
        {
            var result = notEnumString.ToEnum(Test.Default);
            Assert.AreEqual(Test.Default, result);
        }

        [Test]
        public void IfCorrectValue_ReturnParsedEnum()
        {
            var result = enumString.ToEnum(Test.Default);
            Assert.AreEqual(Test.Example, result);
        }

        internal enum Test
        {
            Default,
            Example,
        }
    }
}