using System;
using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Tests.Models;

[TestFixture]
public class ContactsDtoEqualityTests
{
    [Test]
    [TestCaseSource(nameof(MultiTypeTestCases))]
    public void EqualityTestForContactsTypes(Type typeToTest, object left, object right, bool shouldBeEqual)
    {
        bool equalsResult;
        if (left == null && right == null)
        {
            equalsResult = true;
        }
        else if (left == null)
        {
            var typedRight = Convert.ChangeType(right, typeToTest);
            equalsResult = typedRight.Equals(left);
        }
        else
        {
            // Needed to test the object equals method, not equitable
            var typedLeft = Convert.ChangeType(left, typeToTest);
            equalsResult = typedLeft.Equals(right);
        }

        Assert.AreEqual(shouldBeEqual, equalsResult,
            $"Failed equality for type: {typeToTest.Name} with left={left}, right={right}.");
    }

    [Test]
    [TestCaseSource(nameof(MultiTypeTestCases))]
    public void HashcodeTestForContactsTypes(Type typeToTest, object left, object right, bool shouldBeEqual)
    {
        // If both are null - skip
        if (left == null && right == null)
        {
            return;
        }

        var hashEqual = (left?.GetHashCode() ?? 0) == (right?.GetHashCode() ?? 0);
        
        Assert.AreEqual(shouldBeEqual, hashEqual, $"HashCode mismatch for type {typeToTest.Name} with left={left} and right={right}.");
    }

    [Test] [TestCaseSource(nameof(MultiTypeTestCases))]
    public void EquitableTestForContactsTypes(Type typeToTest, object left, object right, bool shouldBeEqual)
    {
        {
            bool equalsResult;
            if (left == null && right == null)
            {
                equalsResult = true;
            }
            else if (left == null || right == null)
            {
                equalsResult = false;
            }
            else
            {
                var equatableInterface = typeof(IEquatable<>).MakeGenericType(typeToTest);
                Assert.True(equatableInterface.IsAssignableFrom(typeToTest), $"{typeToTest.Name} does not implement {equatableInterface}");
                
                // This conversion magically calls IEquitable method instead object's :)
                dynamic dLeft = left;
                equalsResult = dLeft.Equals((dynamic) right);
            }

            Assert.AreEqual(shouldBeEqual, equalsResult,
                $"Failed equality for type: {typeToTest.Name} with left={left}, right={right}.");
        }
    }

    public static IEnumerable<TestCaseData> MultiTypeTestCases
    {
        get
        {
            yield return new TestCaseData(typeof(ContactsDto),
                new ContactsDto {Title = "A", IsDefault = false, Address = new ContactsAddressDto()},
                new ContactsDto {Title = "A", IsDefault = false, Address = new ContactsAddressDto()},
                true
            ).SetName("ContactsDto_SameObjects");

            yield return new TestCaseData(typeof(ContactsDto),
                new ContactsDto {Title = "A", IsDefault = false, Address = new ContactsAddressDto()},
                new ContactsDto {Title = "B", IsDefault = false, Address = new ContactsAddressDto()},
                false
            ).SetName("ContactsDto_DifferentObjects");
            
            yield return new TestCaseData(typeof(ContactsDto),
                new ContactsDto {Title = "A", IsDefault = false, Address = new ContactsAddressDto()},
                null,
                false
            ).SetName("ContactsDto_OtherNull");

            yield return new TestCaseData(typeof(ContactsAddressDto),
                new ContactsAddressDto {BuildingNumber = "1/A", CATOTTGId = 1, Street = "A"},
                new ContactsAddressDto {BuildingNumber = "1/A", CATOTTGId = 1, Street = "A"},
                true
            ).SetName("ContactsAddressDto_SameObjects");

            yield return new TestCaseData(typeof(ContactsAddressDto),
                new ContactsAddressDto {BuildingNumber = "1/A", CATOTTGId = 1, Street = "A"},
                new ContactsAddressDto {BuildingNumber = "1/A", CATOTTGId = 2, Street = "A"},
                false
            ).SetName("ContactsAddressDto_DifferentObjects");
            
            yield return new TestCaseData(typeof(ContactsAddressDto),
                new ContactsAddressDto {BuildingNumber = "1/A", CATOTTGId = 1, Street = "A"},
                null,
                false
            ).SetName("ContactsAddressDto_OtherNull");
            
            yield return new TestCaseData(typeof(PhoneNumberDto),
                new PhoneNumberDto {Type = "A", Number = "123456"},
                new PhoneNumberDto {Type = "A", Number = "123456"},
                true
            ).SetName("PhoneNumberDto_SameObjects");
            
            yield return new TestCaseData(typeof(PhoneNumberDto),
                new PhoneNumberDto {Type = "A", Number = "123456"},
                new PhoneNumberDto {Type = "A", Number = "123457"},
                false
            ).SetName("PhoneNumberDto_DifferentObjects");
            
            yield return new TestCaseData(typeof(PhoneNumberDto),
                new PhoneNumberDto {Type = "A", Number = "123456"},
                null,
                false
            ).SetName("PhoneNumberDto_OtherNull");
            
            yield return new TestCaseData(typeof(EmailDto),
                new EmailDto {Type = "A", Address = "a@b.com"},
                new EmailDto {Type = "A", Address = "a@b.com"},
                true
            ).SetName("EmailDto_SameObjects");
            
            yield return new TestCaseData(typeof(EmailDto),
                new EmailDto {Type = "A", Address = "a@b.com"},
                new EmailDto {Type = "A", Address = "b@b.com"},
                false
            ).SetName("EmailDto_DifferentObjects");
            
            yield return new TestCaseData(typeof(EmailDto),
                new EmailDto {Type = "A", Address = "a@b.com"},
                null,
                false
            ).SetName("EmailDto_OtherNull");
            
            yield return new TestCaseData(typeof(SocialNetworkDto),
                new SocialNetworkDto {Type = SocialNetworkContactType.Instagram, Url = "www.instagram.com"},
                new SocialNetworkDto {Type = SocialNetworkContactType.Instagram, Url = "www.instagram.com"},
                true
            ).SetName("SocialNetworkDto_SameObjects");
            
            yield return new TestCaseData(typeof(SocialNetworkDto),
                new SocialNetworkDto {Type = SocialNetworkContactType.Instagram, Url = "www.instagram.com"},
                new SocialNetworkDto {Type = SocialNetworkContactType.Facebook, Url = "www.facebook.com"},
                false
            ).SetName("SocialNetworkDto_DifferentObjects");
            
            yield return new TestCaseData(typeof(SocialNetworkDto),
                new SocialNetworkDto {Type = SocialNetworkContactType.Instagram, Url = "www.instagram.com"},
                null,
                false
            ).SetName("SocialNetworkDto_OtherNull");
        }
    }
}