using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OutOfSchool.WebApi.Models;
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

        public static ProviderDto GetDeepCopyProviderDto(this ProviderDto originalDto)
        {
            var copiedEntity = new ProviderDto();
            copiedEntity.Id = originalDto.Id;
            copiedEntity.FullTitle = originalDto.FullTitle;
            copiedEntity.ShortTitle = originalDto.ShortTitle;
            copiedEntity.Website = originalDto.Website;
            copiedEntity.Email = originalDto.Email;
            copiedEntity.PhoneNumber = originalDto.PhoneNumber;
            copiedEntity.Facebook = originalDto.Facebook;
            copiedEntity.Instagram = originalDto.Instagram;
            copiedEntity.Description = originalDto.Description;
            copiedEntity.Director = originalDto.Director;
            copiedEntity.DirectorDateOfBirth = originalDto.DirectorDateOfBirth;
            copiedEntity.EdrpouIpn = originalDto.EdrpouIpn;
            copiedEntity.UserId = originalDto.UserId;
            copiedEntity.Ownership = originalDto.Ownership;
            copiedEntity.Founder = originalDto.Founder;
            copiedEntity.Status = originalDto.Status;
            copiedEntity.Type = originalDto.Type;
            copiedEntity.Rating = originalDto.Rating;
            copiedEntity.NumberOfRatings = originalDto.NumberOfRatings;


            copiedEntity.ActualAddress = new AddressDto
            {
                Region = originalDto.ActualAddress.Region,
                District = originalDto.ActualAddress.District,
                Street = originalDto.ActualAddress.Street,
                City = originalDto.ActualAddress.City,
                BuildingNumber = originalDto.ActualAddress.BuildingNumber,
            };

            copiedEntity.LegalAddress = new AddressDto
            {
                Region = originalDto.LegalAddress.Region,
                District = originalDto.LegalAddress.District,
                Street = originalDto.LegalAddress.Street,
                City = originalDto.LegalAddress.City,
                BuildingNumber = originalDto.LegalAddress.BuildingNumber,
            };

            return copiedEntity;
        }
    }
}
