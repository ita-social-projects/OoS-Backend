using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class DecimalDotJsonConverterTests
{
    private static readonly DecimalDotJsonConverter _converter = new ();
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions ();
    private static readonly Type _typeToConvert = typeof(decimal?);

    #region Read

    [Test]
    public void Read_WhenDecimalWithDot_ReturnsDecimal()
    {
        var decimalString = "1234.56";
        var bytes = Encoding.UTF8.GetBytes(decimalString);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read();
        var expected = 1234.56m;

        // Act
        var result = _converter.Read(ref reader, _typeToConvert, _options);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Read_WhenDecimalWithComma_ReturnsDecimal()
    {
        var decimalString = "\"1234,56\"";
        var bytes = Encoding.UTF8.GetBytes(decimalString);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read();
        var expected = 1234.56m;

        // Act
        var result = _converter.Read(ref reader, _typeToConvert, _options);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Read_WhenNull_ReturnsNull()
    {
        var nullString = "null";
        var bytes = Encoding.UTF8.GetBytes(nullString);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read();

        // Act
        var result = _converter.Read(ref reader, _typeToConvert, _options);

        // Assert
        Assert.That(result, Is.Null);
    }

    [TestCase("\"1234\\\\56\"")]
    [TestCase("\"1234'56\"")]
    [TestCase("\"abc\"")]
    [TestCase("\" \"")]
    public void Read_WhenInvalidDecimal_ThrowsJsonException(string invalidDecimal)
    {
        var bytes = Encoding.UTF8.GetBytes(invalidDecimal);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read();

        // Act & Assert
        try
        {
            _converter.Read(ref reader, _typeToConvert, _options);
            Assert.Fail();
        }
        catch (JsonException ex)
        {
            Assert.That(ex.Message, Is.EqualTo($"Unable to convert \"{reader.GetString()}\" to decimal."));
        }
    }

    #endregion

    #region Write

    [Test]
    public void Write_WhenDecimalWithDot_WritesDecimal()
    {
        var decimalValue = 1234.56m;
        var expected = "1234.56";
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, decimalValue, _options);
        writer.Flush();
        var result = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Write_WhenNull_WritesNull()
    {
        decimal? decimalValue = null;
        var expected = "null";
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        _converter.Write(writer, decimalValue, _options);
        writer.Flush();
        var result = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    #endregion
}