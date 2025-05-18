using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Logging;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models.ContactInfo;
using System.Collections.Generic;
using System;
using System.Linq;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class NestedObjectChangeLoggerTests
{
    private NestedObjectChangeLogger _logger;
    private IValueProjector _valueProjector;
    private Guid _entityId;
    private string _entityType;
    private string _userId;
    private List<string> _trackedProperties;

    [SetUp]
    public void Setup()
    {
        _logger = new NestedObjectChangeLogger();
        _valueProjector = new ValueProjector();
        _entityId = Guid.NewGuid();
        _entityType = "TestEntity";
        _userId = "user123";
        _trackedProperties = new List<string> { "Name", "Age", "Address", "IsActive" };
    }

    [Test]
    public void CompareAndLogChanges_WithNullObjects_ReturnsEmptyList()
    {
        // Arrange
        TestPerson oldObj = null;
        TestPerson newObj = null;

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void CompareAndLogChanges_WithOneNullObject_ReturnsEmptyList()
    {
        // Arrange
        TestPerson oldObj = new TestPerson { Name = "John" };
        TestPerson newObj = null;

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void CompareAndLogChanges_WithIdenticalObjects_ReturnsEmptyList()
    {
        // Arrange
        var address = new ContactsAddress { Street = "Main St", BuildingNumber = "123" };
        var oldObj = new TestPerson { Name = "John", Age = 30, Address = address, IsActive = true };
        var newObj = new TestPerson { Name = "John", Age = 30, Address = address, IsActive = true };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void CompareAndLogChanges_WithSimpleChanges_ReturnsProperLogs()
    {
        // Arrange
        var oldObj = new TestPerson { Name = "John", Age = 30, IsActive = true };
        var newObj = new TestPerson { Name = "John Doe", Age = 31, IsActive = false };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count); // Three properties changed

        var nameLog = result.FirstOrDefault(l => l.PropertyName == "Name");
        Assert.IsNotNull(nameLog);
        Assert.AreEqual("\"John\"", nameLog.OldValue);
        Assert.AreEqual("\"John Doe\"", nameLog.NewValue);
        Assert.AreEqual(_entityId, nameLog.EntityIdGuid);
        Assert.AreEqual(_entityType, nameLog.EntityType);
        Assert.AreEqual(_userId, nameLog.UserId);

        var ageLog = result.FirstOrDefault(l => l.PropertyName == "Age");
        Assert.IsNotNull(ageLog);
        Assert.AreEqual("30", ageLog.OldValue);
        Assert.AreEqual("31", ageLog.NewValue);

        var activeLog = result.FirstOrDefault(l => l.PropertyName == "IsActive");
        Assert.IsNotNull(activeLog);
        Assert.AreEqual("true", activeLog.OldValue);
        Assert.AreEqual("false", activeLog.NewValue);
    }

    [Test]
    public void CompareAndLogChanges_WithComplexObjectChanges_ReturnsJsonLogs()
    {
        // Arrange
        var oldObj = new TestPerson
        {
            Name = "John",
            Address = new ContactsAddress { Street = "Old Street", BuildingNumber = "123" }
        };

        var newObj = new TestPerson
        {
            Name = "John",
            Address = new ContactsAddress { Street = "New Street", BuildingNumber = "456" }
        };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count); // Only Address changed

        var addressLog = result[0];
        Assert.AreEqual("Address", addressLog.PropertyName);

        // Confirm that the old and new values contain the expected JSON serialized data
        Assert.IsTrue(addressLog.OldValue.Contains("Old Street"));
        Assert.IsTrue(addressLog.NewValue.Contains("New Street"));
        Assert.IsTrue(addressLog.OldValue.Contains("123"));
        Assert.IsTrue(addressLog.NewValue.Contains("456"));
    }

    [Test]
    public void CompareAndLogChanges_WithValueProjector_UsesProjectorForValues()
    {
        // Arrange
        var oldObj = new TestPerson
        {
            Name = "John",
            Address = new ContactsAddress { CATOTTGId = 123, Street = "Old Street", BuildingNumber = "10" }
        };

        var newObj = new TestPerson
        {
            Name = "John",
            Address = new ContactsAddress { CATOTTGId = 123, Street = "New Street", BuildingNumber = "20" }
        };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties,
            _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count); // Only Address changed

        var addressLog = result[0];
        Assert.AreEqual("Address", addressLog.PropertyName);
        Assert.AreEqual("123, Old Street, 10", addressLog.OldValue);
        Assert.AreEqual("123, New Street, 20", addressLog.NewValue);
    }

    [Test]
    public void CompareAndLogChanges_WithLongValues_TruncatesValues()
    {
        // Arrange
        var longString = new string('X', 1000); // String longer than max value length (500)
        var oldObj = new TestPerson { Name = "John" };
        var newObj = new TestPerson { Name = longString };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        var nameLog = result[0];
        Assert.IsTrue(nameLog.NewValue.Length <= 500); // Should be truncated to 500 chars or less
        Assert.AreEqual("\"John\"", nameLog.OldValue);
    }

    [Test]
    public void CompareAndLogChanges_WithUnchangedTrackedProperties_ReturnsNoLogs()
    {
        // Arrange
        var oldObj = new TestPerson { Name = "John", Age = 30, IsActive = true, Salary = 50000 };
        var newObj = new TestPerson { Name = "John", Age = 30, IsActive = true, Salary = 60000 };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties); // Salary is not in tracked properties

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count); // No logs because Salary is not tracked
    }

    [Test]
    public void CompareAndLogChanges_WithUntrackedProperties_OnlyTracksSpecifiedProperties()
    {
        // Arrange
        var oldObj = new TestPerson { Name = "John", Age = 30, IsActive = true, Salary = 50000 };
        var newObj = new TestPerson { Name = "John Doe", Age = 30, IsActive = true, Salary = 60000 };

        // Act - only track Name, not Age, IsActive, or Salary
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            new List<string> { "Name" });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count); // Only Name should be logged
        Assert.AreEqual("Name", result[0].PropertyName);
    }

    [Test]
    public void CompareAndLogChanges_WithCollectionType_ThrowsArgumentException()
    {
        // Arrange
        var oldList = new List<string> { "item1", "item2" };
        var newList = new List<string> { "item1", "item3" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _logger.CompareAndLogChanges(
            oldList,
            newList,
            _entityId,
            _entityType,
            _userId,
            new List<string>()));
    }

    [Test]
    public void CompareAndLogChanges_WithStringType_DoesNotThrow()
    {
        // Arrange
        string oldString = "Hello";
        string newString = "World";

        // Act
        var result = _logger.CompareAndLogChanges(
            oldString,
            newString,
            _entityId,
            _entityType,
            _userId,
            new List<string>()); // No properties to track for string

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count); // String has no properties
    }

    [Test]
    public void CompareAndLogChanges_WithNullPropertyValues_HandlesCorrectly()
    {
        // Arrange
        var oldObj = new TestPerson { Name = null, Age = 30 };
        var newObj = new TestPerson { Name = "John", Age = 30 };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        var nameLog = result[0];
        Assert.AreEqual("Name", nameLog.PropertyName);
        Assert.AreEqual("null", nameLog.OldValue);
        Assert.AreEqual("\"John\"", nameLog.NewValue);
    }

    [Test]
    public void CompareAndLogChanges_WithSameTimestamp_UsesConsistentTimestamp()
    {
        // Arrange
        var oldObj = new TestPerson { Name = "John", Age = 30 };
        var newObj = new TestPerson { Name = "John Doe", Age = 31 };

        // Act
        var result = _logger.CompareAndLogChanges(
            oldObj,
            newObj,
            _entityId,
            _entityType,
            _userId,
            _trackedProperties);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);

        var timestamp = result[0].UpdatedDate;
        foreach (var log in result)
        {
            Assert.AreEqual(timestamp, log.UpdatedDate);
        }
    }

    // Helper class for testing
    private class TestPerson
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public ContactsAddress Address { get; set; }
        public bool IsActive { get; set; }
        public decimal Salary { get; set; }
    }
}
