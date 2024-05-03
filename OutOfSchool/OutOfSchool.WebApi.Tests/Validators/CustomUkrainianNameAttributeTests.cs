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
        "Дар'я", "Ємельян", "Ґар'єнко", "Ібраґімович", "Ївашко", "Приїжко",
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

    public static IEnumerable<string> NonUkrainianSingleNameStrings =>
    [
        "Валер'ian", "Veler'ян",
        "Anastasia", "Mariia", "Vladyslav", "Dmytro",
        "明美", "明里", "愛子", "金锦津錦", "金澤", "김연아",
        "Рыбаков", "Рыбак", "Объектов", "Объект", "Ёженов", "Проёжов", "Эвклид", "Проэкт",
    ];

    public static IEnumerable<string> InvalidMultipleNamesContainingWhitespaces =>
    [
        "Ім'я - - Ім'я",
        "Ім'я -  ім'я  - Ім'я",
        "Ім'я-               Ім'я",
        "Ім'я-",
        "-Ім'я",
        "-Ім'я-",
    ];

    public static IEnumerable<string> InvalidNamesContainingInvalidApostrophes =>
    [
        "'Мар'ян'",
        "'Мар'ян",
        "Мар'ян'",
        "Мар'я'н",
        "мар'ян",
        "маr'ян",
    ];

    public static IEnumerable<object[]> ValidComminSingleNamesTestParams =>
        ValidCommonSingleNamesStrings.Select(n => new object[] { n });

    public static IEnumerable<object[]> ValidCommonDoubleNamesTestParams =>
        ValidCommonSingleNamesStrings.Select(n => new object[] { $"{n}-{n}" });

    public static IEnumerable<object[]> InvalidNamesContainingInvalidSymbolsTestParams =>
        InvalidNamesContainingInvalidSymbols.Select(n => new object[] { n });

    public static IEnumerable<object[]> InvalidNonUkrainianSingleNamesTestParams =>
        NonUkrainianSingleNameStrings.Select(n => new object[] { n });

    public static IEnumerable<object[]> InvalidMultipleNamesContainingWhitespacesTestParams =>
        InvalidMultipleNamesContainingWhitespaces.Select(n => new object[] { n });

    public static IEnumerable<object[]> InvalidNamesContainingInvalidApostrophesTestParams =>
        InvalidNamesContainingInvalidApostrophes.Select(n => new object[] { n });

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

    [TestCaseSource(nameof(InvalidNamesContainingInvalidSymbolsTestParams))]
    public void IsValid_WhenNameIsContainingInvalidSymbols_ShouldReturnFalse(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestCaseSource(nameof(InvalidNonUkrainianSingleNamesTestParams))]
    public void IsValid_WhenNameIsNonUkrainianName_ShouldReturnFalse(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }

    [Test]
    public void IsValid_WhenNameIsContainsDigits_ShouldReturnFalse()
    {
        // Assert
        var value = "Ім'я123";

        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestCaseSource(nameof(InvalidMultipleNamesContainingWhitespacesTestParams))]
    public void IsValid_WhenNameIsContainsWhitespaces_ShouldReturnFalse(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestCaseSource(nameof(InvalidNamesContainingInvalidApostrophesTestParams))]
    public void IsValid_WhenNameIsContainsInvalidApostrophes(object value)
    {
        // Act
        var isValid = new CustomUkrainianNameAttribute().IsValid(value);

        // Assert
        Assert.IsFalse(isValid);
    }
}
