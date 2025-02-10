using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ContactsServiceTests
{
    private IContactsService<TestEntity, TestDto> contactsService;

    [SetUp]
    public void SetUp()
    {
        var mapper = TestHelper.CreateMapperInstanceOfProfileType<ContactsProfile>();
        contactsService = new ContactsService<TestEntity, TestDto>(mapper);
    }

    [Test]
    public void PrepareNewContacts_WhenDtoContactsIsNullOrEmpty_ShouldNotThrowOrAssign()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto {Contacts = []};

        // Act
        contactsService.PrepareNewContacts(entity, dto);

        // Assert
        Assert.AreEqual(0, entity.Contacts.Count,
            "Expected no contacts to be assigned if DTO contacts are empty.");
    }

    [Test]
    public void PrepareNewContacts_WhenDtoHasMultipleDefaults_ShouldThrowInvalidOperation()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = true},
                new ContactsDto {Title = "Contact2", IsDefault = true}
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
                contactsService.PrepareNewContacts(entity, dto),
            "Should throw if more than one IsDefault.");
    }

    [Test]
    public void PrepareNewContacts_WhenDtoHasZeroDefaults_ShouldSetFirstToDefault()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "Contact2", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act
        contactsService.PrepareNewContacts(entity, dto);

        // Assert
        Assert.AreEqual(2, entity.Contacts.Count);
        Assert.IsTrue(entity.Contacts[0].IsDefault, "First contact should be set to default.");
        Assert.IsFalse(entity.Contacts[1].IsDefault);
    }

    [Test]
    public void PrepareNewContacts_WhenDtoHasExactlyOneDefault_ShouldMapSuccessfully()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "Contact2", IsDefault = true, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act
        contactsService.PrepareNewContacts(entity, dto);

        // Assert
        Assert.AreEqual(2, entity.Contacts.Count, "Expected 2 contacts to be mapped.");
        Assert.IsTrue(entity.Contacts.Any(c => c.Title == "Contact2" && c.IsDefault));
        Assert.IsFalse(entity.Contacts[0].IsDefault && entity.Contacts[1].IsDefault,
            "Only one contact should remain default.");
    }
    
    [Test]
    public void PrepareNewContacts_WhenDtoHasDuplicates_ShouldFilterUnique()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "Contact1", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act
        contactsService.PrepareNewContacts(entity, dto);

        // Assert
        Assert.AreEqual(1, entity.Contacts.Count);
        Assert.IsTrue(entity.Contacts[0].IsDefault, "First contact should be set to default.");
    }

    [Test]
    public void PrepareNewContacts_WhenContactHasNoAddress_ShouldThrowInvalidOperation()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = true, Address = null, Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => contactsService.PrepareNewContacts(entity, dto));
        Assert.That(ex.Message, Is.EqualTo("Address must be specified for each contact."));
    }

    [Test]
    public void PrepareNewContacts_WhenContactHasNoPhones_ShouldThrowInvalidOperation()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = true, Address = new ContactsAddressDto(), Phones = []}
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => contactsService.PrepareNewContacts(entity, dto));
        Assert.That(ex.Message, Is.EqualTo("At least one phone number must be specified for each contact."));
    }

    [Test]
    public void PrepareUpdatedContacts_WhenEntityHasNoContacts_ShouldMapDto()
    {
        // Arrange
        var entity = new TestEntity();
        var dto = new TestDto
        {
            Contacts = [new ContactsDto {Title = "NewContact", IsDefault = true, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}]
        };

        // Act
        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        Assert.AreEqual(1, entity.Contacts.Count);
        Assert.AreEqual("NewContact", entity.Contacts[0].Title);
        Assert.IsTrue(entity.Contacts[0].IsDefault);
    }

    [Test]
    public void PrepareUpdatedContacts_WhenDtoIsEmpty_ShouldDoNothing()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts = [new Contacts {Title = "Existing", IsDefault = false}]
        };
        var dto = new TestDto {Contacts = new List<ContactsDto>()};

        // Act
        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        Assert.AreEqual(1, entity.Contacts.Count);
        Assert.AreEqual("Existing", entity.Contacts[0].Title);
    }

    [Test]
    public void PrepareUpdatedContacts_WhenDtoHasMultipleDefaults_ShouldThrow()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts =
            [
                new Contacts {Title = "Existing1", IsDefault = false},
                new Contacts {Title = "Existing2", IsDefault = true}
            ]
        };

        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Existing1", IsDefault = true},
                new ContactsDto {Title = "Existing2", IsDefault = true}
            ]
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            contactsService.PrepareUpdatedContacts(entity, dto));
    }

    [Test]
    public void PrepareUpdatedContacts_WhenDtoHasZeroDefaults_ShouldSetFirstToDefault()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts =
            [
                new Contacts {Title = "Existing1", IsDefault = true, Address = new ContactsAddress()},
                new Contacts {Title = "Existing2", IsDefault = false, Address = new ContactsAddress()}
            ]
        };
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Existing1", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "Existing2", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "NewOne", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act
        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        Assert.AreEqual(3, entity.Contacts.Count, "We expect 3 contacts after merging.");

        // The first contact in the DTO was "Existing1", so that is set default
        var cExisting1 = entity.Contacts.First(c => c.Title == "Existing1");
        Assert.IsTrue(cExisting1.IsDefault, "First contact in the DTO list must become default if none were default.");

        Assert.IsFalse(entity.Contacts.FirstOrDefault(c => c.Title == "Existing2")?.IsDefault);
        Assert.IsFalse(entity.Contacts.FirstOrDefault(c => c.Title == "NewOne")?.IsDefault);
    }

    [Test]
    public void PrepareUpdatedContacts_WhenContactsOverlap_ShouldUpdateFields()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts =
            [
                new Contacts
                {
                    Title = "Overlap", IsDefault = false, Address = new ContactsAddress()
                },
                new Contacts {Title = "OldStuff", IsDefault = true, Address = new ContactsAddress()}
            ]
        };
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Overlap", IsDefault = true, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "NewDto", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act
        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        // Overlap item should be updated to IsDefault = true
        var overlap = entity.Contacts.FirstOrDefault(c => c.Title == "Overlap");
        Assert.IsNotNull(overlap);
        Assert.IsTrue(overlap.IsDefault, "Overlap contact should now be default.");

        Assert.IsNull(entity.Contacts.FirstOrDefault(c => c.Title == "OldStuff"),
            "Contacts not in new DTO should be removed.");

        // A new contact was added
        var newlyAdded = entity.Contacts.FirstOrDefault(c => c.Title == "NewDto");
        Assert.IsNotNull(newlyAdded);
        Assert.AreEqual("NewDto", newlyAdded.Title);
        Assert.IsFalse(newlyAdded.IsDefault);
    }

    [Test]
    public void PrepareUpdatedContacts_ShouldUpdateSubListsCorrectly()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts =
            [
                new Contacts
                {
                    Title = "Contact1",
                    IsDefault = true,
                    Address = new ContactsAddress
                    {
                        BuildingNumber = "1/A",
                        Street = "A",
                        CATOTTGId = 1,
                    },
                    Phones =
                    [
                        new PhoneNumber {Type = "Mobile", Number = "123-456"},
                        new PhoneNumber {Type = "Home", Number = "123-789"}
                    ]
                }
            ]
        };

        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto
                {
                    Title = "Contact1",
                    IsDefault = true,
                    Address = new ContactsAddressDto
                    {
                        BuildingNumber = "1/A",
                        Street = "A",
                        CATOTTGId = 1,
                    },
                    Phones =
                    [
                        new PhoneNumberDto {Type = "Mobile", Number = "123-456"},
                        new PhoneNumberDto {Type = "Work", Number = "789-123"}
                    ]
                }
            ]
        };

        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        var updatedContact = entity.Contacts.First();
        Assert.AreEqual(2, updatedContact.Phones.Count,
            "Expected exactly 2 phones after update");

        Assert.IsNotNull(updatedContact.Phones
                .FirstOrDefault(p => p.Type == "Mobile" && p.Number == "123-456"),
            "Mobile 123-456 should remain");

        Assert.IsNull(updatedContact.Phones
                .FirstOrDefault(p => p.Type == "Home" && p.Number == "123-789"),
            "Home 123-789 should be removed since it's not in the new DTO");

        Assert.IsNotNull(updatedContact.Phones
                .FirstOrDefault(p => p.Type == "Work" && p.Number == "789-123"),
            "Work 789-123 should be added since it's new in the DTO");
    }
    
    [Test]
    public void PrepareUpdatedContacts_WhenDtoHasDuplicates_ShouldFilterUnique()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts =
            [
                new Contacts {Title = "Existing1", IsDefault = true, Address = new ContactsAddress()}
            ]
        };

        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Existing1", IsDefault = true, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "New2", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]},
                new ContactsDto {Title = "New2", IsDefault = false, Address = new ContactsAddressDto(), Phones = [new PhoneNumberDto()]}
            ]
        };
        // Act
        contactsService.PrepareUpdatedContacts(entity, dto);

        // Assert
        Assert.AreEqual(2, entity.Contacts.Count);
        var newlyAdded = entity.Contacts.SingleOrDefault(c => c.Title == "New2");
        Assert.IsNotNull(newlyAdded, "Newly added contact should not be null.");
    }

    [Test]
    public void PrepareUpdatedContacts_WhenContactHasNoAddress_ShouldThrowInvalidOperation()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts = [new Contacts {Title = "Existing", IsDefault = true, Address = new ContactsAddress()}]
        };
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = true, Address = null, Phones = [new PhoneNumberDto()]}
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => contactsService.PrepareUpdatedContacts(entity, dto));
        Assert.That(ex.Message, Is.EqualTo("Address must be specified for each contact."));
    }

    [Test]
    public void PrepareUpdatedContacts_WhenContactHasNoPhones_ShouldThrowInvalidOperation()
    {
        // Arrange
        var entity = new TestEntity
        {
            Contacts = [new Contacts {Title = "Existing", IsDefault = true, Address = new ContactsAddress()}]
        };
        var dto = new TestDto
        {
            Contacts =
            [
                new ContactsDto {Title = "Contact1", IsDefault = true, Address = new ContactsAddressDto(), Phones = []}
            ]
        };

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => contactsService.PrepareUpdatedContacts(entity, dto));
        Assert.That(ex.Message, Is.EqualTo("At least one phone number must be specified for each contact."));
    }

    private class TestEntity : BusinessEntity, IHasContacts
    {
        public List<Contacts> Contacts { get; set; } = [];
    }

    private class TestDto : IHasContactsDto<TestEntity>
    {
        public List<ContactsDto> Contacts { get; set; } = [];
        public Guid Id { get; set; }
    }
}