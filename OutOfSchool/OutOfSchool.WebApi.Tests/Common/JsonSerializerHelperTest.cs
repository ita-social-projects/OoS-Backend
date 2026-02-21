using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NUnit.Framework;
using OutOfSchool.Common;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class JsonSerializerHelperTest
{
    private const string JSONSTRING = "{\"property\":\"test\"}";
    private static readonly JsonSerializerOptions JsonSerializerOptionsGeneral = new(JsonSerializerDefaults.General);
    private static readonly JsonSerializerOptions JsonSerializerOptionsWeb = new(JsonSerializerDefaults.Web);

    #region
    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithJsonSerializerOptionsIsGeneral_ReturnsUnvalidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(JSONSTRING, JsonSerializerOptionsGeneral);

        // Assert
        Assert.AreNotEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithJsonSerializerOptionsIsWeb_ReturnsValidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(JSONSTRING, JsonSerializerOptionsWeb);

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithJsonSerializerOptionsIsNull_ReturnsValidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(JSONSTRING);

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStreamIsValid_WithJsonSerializerOptionsIsGeneral_ReturnsValidDeserializedObject()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));
        JsonSerializer.Serialize(jsonTextWriter, objectToWrite);

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(new MemoryStream(TrimEnd(bytes)), JsonSerializerOptionsGeneral);

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStreamIsValid_WithJsonSerializerOptionsIsWeb_ReturnsValidDeserializedObject()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));
        JsonSerializer.Serialize(jsonTextWriter, objectToWrite);

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(new MemoryStream(TrimEnd(bytes)), JsonSerializerOptionsWeb);

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStreamIsValid_WithJsonSerializerOptionsIsNull_ReturnsValidDeserializedObject()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));
        JsonSerializer.Serialize(jsonTextWriter, objectToWrite);

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize<TestObject>(new MemoryStream(TrimEnd(bytes)));

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStreamIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var stream = null as Stream;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonSerializerHelper.Deserialize<TestObject>(stream));
    }

    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithTypeAndJsonSerializerOptionsIsGeneral_ReturnsUnvalidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize(JSONSTRING, typeof(TestObject), JsonSerializerOptionsGeneral);

        // Assert
        Assert.AreNotEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithTypeAndJsonSerializerOptionsIsWeb_ReturnsValidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize(JSONSTRING, typeof(TestObject), JsonSerializerOptionsWeb);

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void Deserialize_WhenJsonFromStringIsValid_WithTypeAndJsonSerializerOptionsIsNull_ReturnsValidDeserializedObject()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var deserializedObject = JsonSerializerHelper.Deserialize(JSONSTRING, typeof(TestObject));

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }
    #endregion

    #region
    [Test]
    public void SerializeToJson_WhenObjectIsValid_WithJsonSerializerOptionsIsGeneral_ReturnsUnvalidJson()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var jsonResult = JsonSerializerHelper.Serialize(objectToWrite, JsonSerializerOptionsGeneral);

        // Assert
        Assert.AreNotEqual(JSONSTRING, jsonResult);
    }

    [Test]
    public void SerializeToJson_WhenObjectIsValid_WithJsonSerializerOptionsIsWeb_ReturnsValidJson()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var jsonResult = JsonSerializerHelper.Serialize(objectToWrite, JsonSerializerOptionsWeb);

        // Assert
        Assert.AreEqual(JSONSTRING, jsonResult);
    }

    [Test]
    public void SerializeToJson_WhenObjectIsValid_WithJsonSerializerOptionsIsNull_ReturnsValidJson()
    {
        // Arrange
        var objectToWrite = new TestObject("test");

        // Act
        var jsonResult = JsonSerializerHelper.Serialize(objectToWrite);

        // Assert
        Assert.AreEqual(JSONSTRING, jsonResult);
    }

    [Test]
    public void SerializeToJsonWithUtf8JsonWriter_WhenObjectIsValid_WithJsonSerializerOptionsIsGeneral_ReturnsUnvalidJson()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));

        // Act
        JsonSerializerHelper.Serialize(jsonTextWriter, objectToWrite, JsonSerializerOptionsGeneral);
        jsonTextWriter.Flush();
        var result = Encoding.UTF8.GetString(TrimEnd(bytes));

        // Assert
        Assert.AreNotEqual(JSONSTRING, result);
    }

    [Test]
    public void SerializeToJsonWithUtf8JsonWriter_WhenObjectIsValid_WithJsonSerializerOptionsIsWeb_ReturnsValidJson()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));

        // Act
        JsonSerializerHelper.Serialize(jsonTextWriter, objectToWrite, JsonSerializerOptionsWeb);
        jsonTextWriter.Flush();
        var result = Encoding.UTF8.GetString(TrimEnd(bytes));

        // Assert
        Assert.AreEqual(JSONSTRING, result);
    }

    [Test]
    public void SerializeToJsonWithUtf8JsonWriter_WhenObjectIsValid_WithJsonSerializerOptionsIsNull_ReturnsValidJson()
    {
        // Arrange
        const int BufferSize = 1024;
        var objectToWrite = new TestObject("test");
        var bytes = new byte[BufferSize];
        using var jsonTextWriter = new Utf8JsonWriter(new MemoryStream(bytes));

        // Act
        JsonSerializerHelper.Serialize(jsonTextWriter, objectToWrite);
        jsonTextWriter.Flush();
        var result = Encoding.UTF8.GetString(TrimEnd(bytes));

        // Assert
        Assert.AreEqual(JSONSTRING, result);
    }

    [Test]
    public void SerializeToJsonWithUtf8JsonWriter_WhenUtf8JsonWriterIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var objectToWrite = new TestObject("test");
        var jsonTextWriter = null as Utf8JsonWriter;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonSerializerHelper.Serialize(jsonTextWriter, objectToWrite));
    }
    #endregion

    private static byte[] TrimEnd(byte[] array)
    {
        var lastIndex = Array.FindLastIndex(array, b => b != 0);
        Array.Resize(ref array, lastIndex + 1);

        return array;
    }

    private sealed record TestObject(string Property);
}
