using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services.Logging;
using System.Collections.Generic;
using System;
using OutOfSchool.BusinessLogic.Services;
using System.Linq;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class CollectionChangeLoggerTests
{
    private CollectionChangeLogger _logger;
    private readonly Guid _entityId = Guid.NewGuid();
    private const string _entityType = "TestEntity";
    private const string _userId = "user123";
    private TestValueProjector _valueProjector;

    [SetUp]
    public void Setup()
    {
        _logger = new CollectionChangeLogger();
        _valueProjector = new TestValueProjector();
    }

    [Test]
    public void CompareCollections_WhenOldItemsIsNull_ShouldHandleGracefully()
    {
        // Arrange
        List<TestItem> oldItems = null;
        var newItems = new List<TestItem> { new() { Id = "1", Name = "Test" } };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        var nameLogs = result.Where(r => r.PropertyName.Contains("Name")).ToList();
        Assert.IsTrue(nameLogs.Count > 0);
        var nameLog = nameLogs[0];
        Assert.IsNull(nameLog.OldValue);
        Assert.AreEqual("Test", nameLog.NewValue);
        Assert.IsTrue(nameLog.PropertyName.Contains("Added"));
    }

    [Test]
    public void CompareCollections_WhenNewItemsIsNull_ShouldHandleGracefully()
    {
        // Arrange
        var oldItems = new List<TestItem> { new TestItem { Id = "1", Name = "Test" } };
        List<TestItem> newItems = null;

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
        var nameLogs = result.Where(r => r.PropertyName.Contains("Name")).ToList();
        Assert.IsTrue(nameLogs.Count > 0);
        var nameLog = nameLogs[0];
        Assert.AreEqual("Test", nameLog.OldValue);
        Assert.IsNull(nameLog.NewValue);
        Assert.IsTrue(nameLog.PropertyName.Contains("Removed"));
    }

    [Test]
    public void CompareCollections_WhenBothCollectionsEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var oldItems = new List<TestItem>();
        var newItems = new List<TestItem>();

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void CompareCollections_WithNullItem_ShouldSkipNullPropertyValues()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Test", Value = 10 }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = null, Value = 20 }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        var nameLog = result.FirstOrDefault(r => r.PropertyName.Contains("Name"));
        Assert.IsNotNull(nameLog);
        Assert.AreEqual("Test", nameLog.OldValue);
        Assert.IsNull(nameLog.NewValue);

        var valueLog = result.FirstOrDefault(r => r.PropertyName.Contains("Value"));
        Assert.IsNotNull(valueLog);
        Assert.AreEqual("10", valueLog.OldValue);
        Assert.AreEqual("20", valueLog.NewValue);
    }

    [Test]
    public void CompareCollections_WithDifferentCollectionSizes_WhenUsingIndexComparison_ShouldHandleCorrectly()
    {
        // Arrange
        var oldItems = new List<TestItem>
        {
            new TestItem { Id = "1", Name = "First" },
            new TestItem { Id = "2", Name = "Second" },
            new TestItem { Id = "3", Name = "Third" }
        };
        var newItems = new List<TestItem>
        {
            new TestItem { Id = "1", Name = "First Updated" },
            new TestItem { Id = "4", Name = "Fourth" }
        };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            null,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector,
            useIndexOnly: true);

        // Assert
        Assert.IsNotNull(result);

        // Filter logs by property name pattern to focus on specific items and properties
        var item0NameLogs = result.Where(r => r.PropertyName.EndsWith(".Item[0].Name")).ToList();
        var item1NameLogs = result.Where(r => r.PropertyName.EndsWith(".Item[1].Name")).ToList();
        var removed2NameLogs = result.Where(r => r.PropertyName.EndsWith(".Removed[2].Name")).ToList();

        // First item should be updated
        Assert.AreEqual(1, item0NameLogs.Count);
        Assert.AreEqual("First", item0NameLogs[0].OldValue);
        Assert.AreEqual("First Updated", item0NameLogs[0].NewValue);

        // Second item should be changed completely
        Assert.AreEqual(1, item1NameLogs.Count);
        Assert.AreEqual("Second", item1NameLogs[0].OldValue);
        Assert.AreEqual("Fourth", item1NameLogs[0].NewValue);

        // Third item should be removed
        Assert.AreEqual(1, removed2NameLogs.Count);
        Assert.AreEqual("Third", removed2NameLogs[0].OldValue);
        Assert.IsNull(removed2NameLogs[0].NewValue);
    }

    [Test]
    public void CompareCollections_WithValuesThatExceedMaxLength_ShouldTruncateValues()
    {
        // Arrange
        string longValue = new string('X', 1000);
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Short" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = longValue }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);

        var nameLog = result[0];
        Assert.AreEqual("Short", nameLog.OldValue);
        Assert.AreEqual(500, nameLog.NewValue.Length);
        Assert.AreEqual(longValue.Substring(0, 500), nameLog.NewValue);
    }

    [Test]
    public void CompareCollections_WithSamePropertyValues_ShouldNotGenerateLogs()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Same", Value = 10 }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Same", Value = 10 }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void CompareCollections_WithMultipleChangesInSameItem_ShouldGenerateMultipleLogs()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "OldName", Value = 10, Description = "OldDesc" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "NewName", Value = 20, Description = "NewDesc" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);

        var nameLog = result.FirstOrDefault(r => r.PropertyName.Contains("Name"));
        Assert.IsNotNull(nameLog);
        Assert.AreEqual("OldName", nameLog.OldValue);
        Assert.AreEqual("NewName", nameLog.NewValue);

        var valueLog = result.FirstOrDefault(r => r.PropertyName.Contains("Value"));
        Assert.IsNotNull(valueLog);
        Assert.AreEqual("10", valueLog.OldValue);
        Assert.AreEqual("20", valueLog.NewValue);

        var descLog = result.FirstOrDefault(r => r.PropertyName.Contains("Description"));
        Assert.IsNotNull(descLog);
        Assert.AreEqual("OldDesc", descLog.OldValue);
        Assert.AreEqual("NewDesc", descLog.NewValue);
    }

    [Test]
    public void CompareCollections_WithChangedItems_ShouldMaintainConsistentTimestamp()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Old", Value = 10 }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "New", Value = 20 }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count >= 2);

        // All logs should have the same timestamp
        var firstTimestamp = result[0].UpdatedDate;
        Assert.IsTrue(result.All(r => r.UpdatedDate == firstTimestamp));

        // Timestamp should be close to current time
        var timeSpan = DateTime.UtcNow - firstTimestamp;
        Assert.IsTrue(timeSpan.TotalSeconds < 5);
    }

    [Test]
    public void CompareCollections_WithComplexNestedObjects_ShouldUseToStringWhenNoProjector()
    {
        // Arrange
        var oldItems = new List<ComplexItem>
            {
                new ComplexItem
                {
                    Id = "1",
                    ComplexValue = new NestedObject { Data = "OldData" }
                }
            };
        var newItems = new List<ComplexItem>
            {
                new ComplexItem
                {
                    Id = "1",
                    ComplexValue = new NestedObject { Data = "NewData" }
                }
            };

        // Act
        var valueProjector = new TestValueProjector();
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Projected:OldData", result[0].OldValue);
        Assert.AreEqual("Projected:NewData", result[0].NewValue);
    }

    [Test]
    public void CompareCollections_WithNestedNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var oldItems = new List<ComplexItem>
            {
                new ComplexItem { Id = "1", ComplexValue = null }
            };
        var newItems = new List<ComplexItem>
            {
                new ComplexItem
                {
                    Id = "1",
                    ComplexValue = new NestedObject { Data = "NewData" }
                }
            };

        // Act
        var valueProjector = new TestValueProjector();
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsNull(result[0].OldValue);
        Assert.AreEqual("Projected:NewData", result[0].NewValue);
    }

    [Test]
    public void CompareCollections_WithNullProjectorForComplexTypes_ShouldUseToString()
    {
        // Arrange
        var customObject = new CustomToStringObject { Id = 1, Name = "Test" };
        var oldItems = new List<TestItemWithCustomObject>
            {
                new TestItemWithCustomObject { Id = "1", CustomObject = customObject }
            };
        var newItems = new List<TestItemWithCustomObject>
            {
                new TestItemWithCustomObject
                {
                    Id = "1",
                    CustomObject = new CustomToStringObject { Id = 2, Name = "Changed" }
                }
            };

        // Act
        var valueProjector = new TestValueProjector();
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Custom(1, Test)", result[0].OldValue);
        Assert.AreEqual("Custom(2, Changed)", result[0].NewValue);
    }

    [Test]
    public void CompareCollections_WithUnixPathAsId_ShouldExtractFileName()
    {
        // Arrange
        string unixPath = "/usr/local/files/test-data.json";
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = unixPath, Name = "OldName" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = unixPath, Name = "NewName" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].PropertyName.Contains("test-data.json"));
        Assert.IsFalse(result[0].PropertyName.Contains("/usr/local/files/"));
    }

    [Test]
    public void CompareCollections_WithEmptyId_ShouldUseIndexOnly()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "", Name = "OldName" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "", Name = "NewName" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        // Should use index instead of empty ID
        Assert.IsTrue(result[0].PropertyName.Contains("[0]"));
    }

    [Test]
    public void CompareCollections_WithMixedAddedAndChangedItems_ShouldGenerateCorrectLogs()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Item1" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Item1Updated" },
                new TestItem { Id = "2", Name = "Item2" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);

        // Changed item
        var changedLogs = result.Where(r => r.PropertyName.Contains("Item")).ToList();
        Assert.IsTrue(changedLogs.Count > 0);
        var changedLog = changedLogs[0];
        Assert.AreEqual("Item1", changedLog.OldValue);
        Assert.AreEqual("Item1Updated", changedLog.NewValue);

        // Added item
        var addedLogs = result.Where(r => r.PropertyName.Contains("Added")).ToList();
        Assert.IsTrue(addedLogs.Count > 0);
        var addedName = addedLogs.First(r => r.PropertyName.Contains("Name"));
        Assert.IsNotNull(addedName);
        Assert.IsNull(addedName.OldValue);
        Assert.AreEqual("Item2", addedName.NewValue);
    }

    [Test]
    public void CompareCollections_WithMixedRemovedAndChangedItems_ShouldGenerateCorrectLogs()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Item1" },
                new TestItem { Id = "2", Name = "Item2" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Item1Updated" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);

        // Changed item
        var changedLogs = result.Where(r => r.PropertyName.Contains("Item")).ToList();
        Assert.IsTrue(changedLogs.Count > 0);
        var changedLog = changedLogs[0];
        Assert.AreEqual("Item1", changedLog.OldValue);
        Assert.AreEqual("Item1Updated", changedLog.NewValue);

        // Removed item
        var removedLogs = result.Where(r => r.PropertyName.Contains("Removed")).ToList();
        Assert.IsTrue(removedLogs.Count > 0);
        var removedName = removedLogs.First(r => r.PropertyName.Contains("Name"));
        Assert.IsNotNull(removedName);
        Assert.AreEqual("Item2", removedName.OldValue);
        Assert.IsNull(removedName.NewValue);
    }

    [Test]
    public void CompareCollections_WithAllMetadataFields_ShouldSetAllMetadataFields()
    {
        // Arrange
        var oldItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "OldName" }
            };
        var newItems = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "NewName" }
            };

        // Act
        var result = _logger.CompareCollections(
            oldItems,
            newItems,
            item => item.Id,
            _entityId,
            _entityType,
            _userId,
            valueProjector: _valueProjector);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        var log = result[0];
        Assert.AreEqual(_entityId, log.EntityIdGuid);
        Assert.AreEqual(_entityType, log.EntityType);
        Assert.AreEqual(_userId, log.UserId);
        Assert.IsTrue(log.PropertyName.Contains("Name"));
        Assert.AreEqual("OldName", log.OldValue);
        Assert.AreEqual("NewName", log.NewValue);
    }

    #region Test Classes

    private class TestItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Name ?? Value.ToString();
        }
    }

    private class NestedObject
    {
        public string Data { get; set; }

        public override string ToString()
        {
            return Data;
        }
    }

    private class ComplexItem
    {
        public string Id { get; set; }
        public NestedObject ComplexValue { get; set; }
    }

    private class CustomToStringObject
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Custom({Id}, {Name})";
        }
    }

    private class TestItemWithCustomObject
    {
        public string Id { get; set; }
        public CustomToStringObject CustomObject { get; set; }
    }

    private class TestValueProjector : IValueProjector
    {
        public string ProjectValue(Type type, object value)
        {
            if (value is NestedObject nested)
            {
                return $"Projected:{nested.Data}";
            }

            if (value is string strValue)
            {
                return strValue;
            }

            if (value is int intValue)
            {
                return intValue.ToString();
            }

            return value?.ToString();
        }
    }

    #endregion
}