using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.WebApi.Tests.Common;

public class TimespanConverterTest
{
    private static readonly JsonSerializerOptions JsonSerializerOptionsWeb = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private static readonly TimespanConverter Converter = new();

    [TestCase("2023-10-16T00:00:00", "2023-12-31T12:34:56")]
    [TestCase("2023-11-17T00:00:00", "2023-12-31T12:34:56")]
    [TestCase("2023-08-06T00:00:00", "2023-12-31T12:34:56")]
    public void Write_WhenWriterIsNotNull_ShouldWriteValidJsonString(DateTime date1, DateTime date2)
    {
        // Arrange
        TimeSpan interval = date2 - date1;
        var expectedValue = $"\"{interval.Hours}:{interval.Minutes}\"";
        const int BufferSize = 1024;
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));

        // Act
        Converter.Write(jsonTextWriter, interval, JsonSerializerOptionsWeb);
        jsonTextWriter.Flush();
        var result = Encoding.UTF8.GetString(TrimEnd(bytes));

        // Assert
        Assert.AreEqual(expectedValue, result);
    }

    [Test]
    public void Write_WhenWriterIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var jsonTextWriter = null as Utf8JsonWriter;
        TimeSpan interval = default;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Converter.Write(jsonTextWriter, interval, JsonSerializerOptionsWeb));
    }

    [TestCase("10:30")]
    [TestCase("12:49")]
    [TestCase("15:00")]
    public void Read_WhenReaderIsNotNull_ShouldReturnValidObject(string time)
    {
        // Arrange
        var timeJson = $"\"{time}\"";
        var expectedTimeJson = $"{time}:00";
        byte[] bytes = Encoding.UTF8.GetBytes(timeJson);
        var reader = new Utf8JsonReader(bytes.AsSpan());
        reader.Read(); // Read the quote
        Type typeToConvert = typeof(TimeSpan);
        JsonSerializerOptions options = new JsonSerializerOptions();

        // Act
        var result = Converter.Read(ref reader, typeToConvert, options);

        // Assert
        Assert.AreEqual(expectedTimeJson, result.ToString());
    }

    private static byte[] TrimEnd(byte[] array)
    {
        var lastIndex = Array.FindLastIndex(array, b => b != 0);
        Array.Resize(ref array, lastIndex + 1);

        return array;
    }

    private sealed record TestObject(TimeSpan Property);
}
