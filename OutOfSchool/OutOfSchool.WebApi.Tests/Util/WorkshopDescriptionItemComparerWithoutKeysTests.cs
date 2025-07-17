using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.CustomComparers;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Tests.Util;

public class WorkshopDescriptionItemComparerWithoutKeysTests
{
    Guid id1 = Guid.NewGuid();
    Guid id2 = Guid.NewGuid();
    Guid id3 = Guid.NewGuid();

    [Test]
    public void Distinct_WhenWorkshopDescriptionItemListHasDuplications_ReturnTwo()
    {
        // Arrange
        var list = new List<WorkshopDescriptionItem>()
        {
            new WorkshopDescriptionItem() { Id = id1, SectionName = "section", Description = "description" },
            new WorkshopDescriptionItem() { Id = id2, SectionName = "section", Description = "description" },
            new WorkshopDescriptionItem() { Id = id3, SectionName = "section3", Description = "description3" }
        };

        // Act
        var result = list.Distinct(new WorkshopDescriptionItemComparerWithoutKeys()).Count();

        // Assert
        Assert.AreEqual(2, result);
    }
}
