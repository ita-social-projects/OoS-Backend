using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.Images;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.BusinessLogic.Services.SearchString;
using System.Threading.Tasks;
using System;
using System.Linq;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models.ContactInfo;
using System.Collections.Generic;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models;
using OutOfSchool.Common.Enums;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SensitiveCompetitiveEventDraftServiceTest
{
    private CompetitiveEventDraftService service;
    private Mock<ILogger<CompetitiveEventDraftService>> loggerMock;
    private Mock<ICompetitiveEventDraftRepository> draftRepoMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<ICompetitiveEventServiceV2> competitiveEventServiceMock;
    private Mock<IImageDependentEntityImagesInteractionService<CompetitiveEventDraft>> imageServiceMock;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<ICodeficatorRepository> codeficatorRepoMock;

    Guid draftId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<CompetitiveEventDraftService>>();
        draftRepoMock = new Mock<ICompetitiveEventDraftRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        competitiveEventServiceMock = new Mock<ICompetitiveEventServiceV2>();
        imageServiceMock = new Mock<IImageDependentEntityImagesInteractionService<CompetitiveEventDraft>>();
        changesLogServiceMock = new Mock<IChangesLogService>();
        codeficatorRepoMock = new Mock<ICodeficatorRepository>();


        service = new CompetitiveEventDraftService(
            loggerMock.Object,
            currentUserServiceMock.Object,
            competitiveEventServiceMock.Object,
            draftRepoMock.Object,
            imageServiceMock.Object,
            changesLogServiceMock.Object,
            codeficatorRepoMock.Object,
            new Mock<IRegionAdminService>().Object,
            new Mock<IMinistryAdminService>().Object,
            new Mock<ICodeficatorService>().Object,
            new Mock<ISearchStringService>().Object);
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_ReturnsFailed_WhenDtoIsNull()
    {        
        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, null);

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual("400", result.OperationResult.Errors.First().Code);
        Assert.That(result.OperationResult.Errors.First().Description, Does.Contain("Dto must not be null"));
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_ReturnsSuccess_WhenDtoIsValid()
    {
        // Arrange        
        var dto = GetModeratorCompetitiveEventDraftEditDto();

        var competitiveEventDraft = GetCompetitiveEventDraft(draftId);

        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .Returns(Task.CompletedTask);

        draftRepoMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(competitiveEventDraft);

        draftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(competitiveEventDraft);

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, dto);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(CompetitiveEventDraftStatus.EditedByModerator, competitiveEventDraft.DraftStatus);
        Assert.NotNull(result.Value);
        draftRepoMock.Verify(x => x.Update(It.IsAny<CompetitiveEventDraft>()), Times.Once);
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_UpdatingError_ReturnsMessage()
    {
        // Arrange
        var dto = GetModeratorCompetitiveEventDraftEditDto();
        var competitiveEventDraft = GetCompetitiveEventDraft(draftId);

        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .Returns(Task.CompletedTask);

        draftRepoMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(competitiveEventDraft);

        draftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(competitiveEventDraft);
        
        draftRepoMock
            .Setup(x => x.Update(It.IsAny<CompetitiveEventDraft>()))
            .ThrowsAsync(new Exception("Database update failed"));

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, dto);

        // Assert
        Assert.IsFalse(result.Succeeded);

        loggerMock.Verify(
           x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred while updating CompetitiveEventDraft")),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception, string>>()),
           Times.Once);
    }
    
    [Test]
    public async Task UpdateDraftAsModeratorAsync_UpdatesContactsCorrectly_WhenContactsProvided()
    {
        // Arrange
        var existingAddress = new Address { Street = "123 Main St" };
        var existingContact = new Contacts{};

        var competitiveEventDraft = GetCompetitiveEventDraft(draftId);
        competitiveEventDraft.CompetitiveEventDraftContent.Contacts.Add(existingContact);

        var addressDto = new AddressDto { Street = "123 Main St" };
        var contactDto = new ContactsDto{};

        var dto = GetModeratorCompetitiveEventDraftEditDto();
        dto.Contacts = new List<ContactsDto> { contactDto };

        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .Returns(Task.CompletedTask);

        draftRepoMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(competitiveEventDraft);

        draftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(competitiveEventDraft);

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, dto);

        // Assert
        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(CompetitiveEventDraftStatus.EditedByModerator, competitiveEventDraft.DraftStatus);
        
        var updatedContact = competitiveEventDraft.CompetitiveEventDraftContent.Contacts
            .FirstOrDefault(c => c.Title == "Main Office");

        Assert.NotNull(updatedContact, "Contact should be found");
        Assert.AreEqual("Main Office", updatedContact.Title);
        
        draftRepoMock.Verify(x => x.Update(It.IsAny<CompetitiveEventDraft>()), Times.Once);
    }

    [Test]
    public async Task UpdateDraftAsModeratorAsync_DoesNotUpdateNonMatchingContacts_WhenContactsProvided()
    {
        // Arrange
        var existingContact = new Contacts
        {
            Title = "Main Office",            
        };

        var competitiveEventDraft = GetCompetitiveEventDraft(draftId);
        competitiveEventDraft.CompetitiveEventDraftContent.Contacts.Add(existingContact);
        
        var nonMatchingContactDto = new ContactsDto
        {
            Title = "Branch Office", 
        };

        var dto = GetModeratorCompetitiveEventDraftEditDto();
        dto.Contacts = new List<ContactsDto> { nonMatchingContactDto };

        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ModeratorRights>(), It.IsAny<TechAdminRights>()))
            .Returns(Task.CompletedTask);

        draftRepoMock
            .Setup(x => x.GetById(It.IsAny<Guid>()))
            .ReturnsAsync(competitiveEventDraft);

        draftRepoMock.Setup(repo => repo.GetByIdWithDetails(draftId, It.IsAny<string>(),
            It.IsAny<Func<IQueryable<CompetitiveEventDraft>, IQueryable<CompetitiveEventDraft>>>()))
            .ReturnsAsync(competitiveEventDraft);

        var originalPhone = existingContact.Phones.First().Number;        

        // Act
        var result = await service.UpdateDraftAsModeratorAsync(draftId, dto);

        // Assert
        Assert.IsTrue(result.Succeeded);

        // Verify that the original contact was NOT updated (since title didn't match)
        var unchangedContact = competitiveEventDraft.CompetitiveEventDraftContent.Contacts
            .FirstOrDefault(c => c.Title == "Main Office");

        Assert.NotNull(unchangedContact, "Original contact should still exist");
        Assert.AreEqual(originalPhone, unchangedContact.Phones.First().Number, "Phone should remain unchanged");        

        draftRepoMock.Verify(x => x.Update(It.IsAny<CompetitiveEventDraft>()), Times.Once);
    }

    private CompetitiveEventDraft GetCompetitiveEventDraft(Guid draftId)
    {
        return new CompetitiveEventDraft
        {
            Id = draftId,
            DraftStatus = CompetitiveEventDraftStatus.PendingModeration,
            CompetitiveEventDraftContent = new CompetitiveEventDraftContent
            {
                Title = "Some title",
                ShortTitle = "Short title",
                DescriptionOfTheEnrollmentProcedure = "Procedure",
                Contacts = new List<Contacts>
                {
                    new Contacts
                    {

                    }
                }
            },
            Provider = new Provider
            {
                Edrpou = "12345678",
                Positions = new List<Position>
                {
                    new Position
                    {
                        FullName = "ddd",
                        Officials = new List<Official>
                        {
                            new Official
                            {
                                Position = new Position { PositionType = PositionType.Director},
                                Individual = new Individual
                                {
                                    FirstName = "aaa",
                                    LastName = "aaa",
                                    MiddleName = "aaa"
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private ModeratorCompetitiveEventDraftEditDto GetModeratorCompetitiveEventDraftEditDto()
    {
        return new ModeratorCompetitiveEventDraftEditDto
        {
            Title = "Title",
            ShortTitle = "Title",
            DescriptionOfTheEnrollmentProcedure = "Description",
            Contacts = new List<ContactsDto>
            {
                new ContactsDto
                {
                    Title = "pppp"
                }
            }
        };
    }
}
