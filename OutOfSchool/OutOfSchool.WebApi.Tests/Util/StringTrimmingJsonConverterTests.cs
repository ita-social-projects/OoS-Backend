using NUnit.Framework;
using OutOfSchool.WebApi.Util.JsonTools;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace OutOfSchool.WebApi.Tests.Util;

[TestFixture]
public class StringTrimmingJsonConverterTests
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
    private static readonly StringTrimmingJsonConverter _converter = new();

    [TestCase("\"  Hello World  \"", "Hello World")]
    [TestCase("\"NoSpaces\"", "NoSpaces")]
    [TestCase("null", null)]
    public void Read_WhenReaderIsNotNull_ShouldReturnTrimmedString(string jsonInput, string expectedOutput)
    {
        // Arrange
        byte[] bytes = Encoding.UTF8.GetBytes(jsonInput);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read();

        // Act
        var result = _converter.Read(ref reader, typeof(string), _jsonSerializerOptions);

        // Assert
        Assert.AreEqual(expectedOutput, result);
    }

    [Test]
    public void Write_WhenWriterIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var jsonTextWriter = null as Utf8JsonWriter;
        string value = "Test String";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _converter.Write(jsonTextWriter, value, _jsonSerializerOptions));
    }

    [Test]
    public void Write_WhenWriterIsNotNull_ShouldWriteValidJsonString()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var jsonTextWriter = new Utf8JsonWriter(stream);
        string value = "  Test String  ";
        var expectedValue = "\"  Test String  \"";

        // Act
        _converter.Write(jsonTextWriter, value, _jsonSerializerOptions);
        jsonTextWriter.Flush();
        var result = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        Assert.AreEqual(expectedValue, result);
    }
}