using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class CustomUkrainianNameAttributeTests
{
    public static IEnumerable<string> ValidCommonSingleNamesStrings =>
    [
        "Анастасія", "Марія", "Софія", "Вероніка", "Вікторія",
        "Соломія", "Ангеліна", "Злата", "Ганна", "Єва", "Варвара",
        "Поліна", "Аліса", "Олександра", "Мілана", "Дарина", "Аріна",
        "Марина", "Діана", "Катерина", "Артем", "Олександр", "Максим",
        "Дмитро", "Матвій", "Назар", "Богдан", "Марк", "Владислав",
        "Михайло", "Володимир", "Тимофій", "Іван", "Давид", "Андрій",
        "Олексій", "Макс", "Денис", "Антон", "Вадим",
        "Ян", "Як", "Мар'яна", "Валер'ян", "Мар'ян",
        "О'Коннор", "Оставф'єнко", "Прокоп'єнко",
        "Васильович", "Петрович", "Владиславович", "Михайлович",
        "Валер'янович", "Мар'янович", "Прізвище'запострофом",
    ];

    public static IEnumerable<object[]> InvalidNamesContainingInvalidSymbols =>
    [
        ["-----"],
        ["'''''"],
        ["В--д---ав"],
        ["Влади-"],
        ["  -   "],
        ["  '   "],
        ["  О   "],
        ["О-Коннор"],
        ["'"],
        ["-"],
        ["'Мар'ян'"],
        ["Мар'я'н"],
    ];

    public static IEnumerable<object[]> ValidComminSingleNamesTestParams => ValidCommonSingleNamesStrings.Select(n => new object[] { n });

    public static IEnumerable<object[]> ValidCommonDoubleNamesTestParams => ValidCommonSingleNamesStrings.Select(n => new object[] { $"{n}-{n}" });

    [Test]
    public void IsValid_WhenNameIsNull_ShouldReturnTrue()
    {
        // Arrange
        var value = null as string;

        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenNameIsEmpty_ShouldReturnTrue()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_WhenNameIsWhiteSpace_ShouldReturnTrue()
    {
        // Arrange
        var value = " ";

        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestCaseSource(nameof(ValidComminSingleNamesTestParams))]
    public void IsValid_WhenNameIsValidCommonSingleName_ShouldReturnTrue(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestCaseSource(nameof(ValidCommonDoubleNamesTestParams))]
    public void IsValid_WhenNameIsValidCommonDoubleName_ShouldReturnTrue(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsTrue(isValid);
    }

    // TODO:
    // Invalid cases:
    // Done: 1. "-----"
    // Done: 2. "В--д---ав"
    // Done: 3. "Влади-"
    // Done: 4. "  -   "
    // Done: 5. "  '   "
    // Done: 6. "  О   "
    // Done: 7. "123"
    // 8. "О"
    // 9. "О-Коннор"
    // Done: 10. "'"
    // Done: 11. "-"
    // 12. "Vladyslav"
    // 13. "Влаdислав"
    // 14. "Влаdис лав"
    // 15. " Влаdислав"
    // 16. "Влаdислав "
    // 17. "Влаdислав - Владислав"
    // 18. "Влаdислав - - Владислав"
    // 19. "Влаdислав -  влад  - Владислав"
    // 20. "Влаdислав-               Владислав"
    // 21. "Влаdислав-"
    // 22. "Vladyslav-Vladyslav"
    // 23. "'Мар'ян'"
    // 24. "владислав"
    // 25. "мар'Ян"
    // 26. "вЛаДиСлАв"
    // 27. "Мар'я'н"

    [Test]
    public void IsValid_WhenNameIsNotString_ShouldReturnFalse()
    {
        // Arrange
        var value = DateTime.UtcNow;

        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestCaseSource(nameof(InvalidNamesContainingInvalidSymbols))]
    public void IsValid_WhenNameIsContainingInvalidSymbols_ShouldReturnFalse(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }
}
