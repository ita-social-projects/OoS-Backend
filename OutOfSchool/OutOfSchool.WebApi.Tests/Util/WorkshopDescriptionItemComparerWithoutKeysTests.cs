using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Tests.Util;

public class WorkshopDescriptionItemComparerWithoutKeysTests
{
    readonly Guid id1 = Guid.NewGuid();
    readonly Guid id2 = Guid.NewGuid();
    readonly Guid id3 = Guid.NewGuid();
    readonly Guid workshopId = Guid.NewGuid();

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasDuplications_ReturnTwo()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            new() { Id = id1, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id2, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id3, WorkshopId = workshopId, SectionName = "section3", Description = "description3" }
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(2, result);
    }

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasOneNull_ReturnThree()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            new() { Id = id1, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id2, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id3, WorkshopId = workshopId, SectionName = "section3", Description = "description3" },
            null
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(3, result);
    }

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasNulls_ReturnThree()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            new() { Id = id1, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id2, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            new() { Id = id3, WorkshopId = workshopId, SectionName = "section3", Description = "description3" },
            null,
            null
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(3, result);
    }

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasOnlyTwoNulls_ReturnThree()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            null,
            null
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(1, result);
    }

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasOneNullAndOneEntity_ReturnThree()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            new() { Id = id1, WorkshopId = workshopId, SectionName = "section", Description = "description" },
            null
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(2, result);
    }
}
