﻿using System;
using System.IO;
using System.Text;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Extensions;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class StreamExtensionsTest
{
    [Test]
    public void ReadAndDeserializeFromJson_WhenStreamIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var stream = null as Stream;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => stream.ReadAndDeserializeFromJson<object>());
    }

    [Test]
    public void ReadAndDeserializeFromJson_WhenStreamIsNotReadable_ThrowNotSupportedException()
    {
        // Arrange
        var streamMock = new Mock<Stream>();

        streamMock.SetupGet(x => x.CanRead).Returns(false);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => streamMock.Object.ReadAndDeserializeFromJson<object>());
    }

    [Test]
    public void ReadAndDeserializeFromJson_WhenWhenJsonIsValid_ReturnsDeserializedObject()
    {
        // Arrange
        const int BufferSize = 1024;

        var objectToWrite = new TestObject("test");

        var bytes = new byte[BufferSize];

        using var streamWriter = new StreamWriter(new MemoryStream(bytes), new UTF8Encoding(), BufferSize, true);
        using var jsonTextWriter = new JsonTextWriter(streamWriter);

        var jsonSerializer = new JsonSerializer();

        jsonSerializer.Serialize(jsonTextWriter, objectToWrite);
        jsonTextWriter.Flush();

        // Act
        var deserializedObject = new MemoryStream(bytes).ReadAndDeserializeFromJson<TestObject>();

        // Assert
        Assert.AreEqual(objectToWrite, deserializedObject);
    }

    [Test]
    public void SerializeToJsonAndWrite_WhenStreamIsNull_ThrowArgumentNullException()
    {
        // Arrange
        var stream = null as Stream;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => stream.SerializeToJsonAndWrite(It.IsAny<object>()));
    }

    [Test]
    public void SerializeToJsonAndWrite_WhenStreamIsNotWritable_ThrowNotSupportedException()
    {
        // Arrange
        var streamMock = new Mock<Stream>();

        streamMock.SetupGet(x => x.CanWrite).Returns(false);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => streamMock.Object.SerializeToJsonAndWrite(It.IsAny<object>()));
    }

    [Test]
    public void SerializeToJsonAndWrite_WhenWhenObjectIsValid_ReturnsValidJson()
    {
        // Arrange
        const string ExpectedJsonString = "{\"Property\":\"test\"}";
        const int BufferSize = 1024;

        var bytes = new byte[BufferSize];

        // Act
        new MemoryStream(bytes).SerializeToJsonAndWrite(new TestObject("test"));

        using var streamReader = new StreamReader(new MemoryStream(bytes));
        var jsonString = streamReader.ReadToEnd()[..ExpectedJsonString.Length];

        // Assert
        Assert.AreEqual(ExpectedJsonString, jsonString);
    }

    private sealed record TestObject(string Property);
}
