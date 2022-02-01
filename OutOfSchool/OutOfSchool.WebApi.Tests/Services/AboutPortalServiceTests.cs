using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class AboutPortalServiceTests
    {
        private IAboutPortalService service;
        private Mock<IAboutPortalRepository> repositoryMock;
        private Mock<ISensitiveEntityRepository<AboutPortal>> aboutPortalRepositoryMock;
        private Mock<ISensitiveEntityRepository<AboutPortalItem>> aboutPortalItemRepositoryMock;
        private Mock<ILogger<AboutPortalService>> logger;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<IMapper> mapper;

        [SetUp]
        public void SetUp()
        {
            repositoryMock = new Mock<IAboutPortalRepository>();
            aboutPortalRepositoryMock = new Mock<ISensitiveEntityRepository<AboutPortal>>();
            aboutPortalItemRepositoryMock = new Mock<ISensitiveEntityRepository<AboutPortalItem>>();
            logger = new Mock<ILogger<AboutPortalService>>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            mapper = new Mock<IMapper>();

            service = new AboutPortalService(
                repositoryMock.Object,
                aboutPortalRepositoryMock.Object,
                aboutPortalItemRepositoryMock.Object,
                logger.Object,
                localizer.Object,
                mapper.Object);
        }
    }
}
