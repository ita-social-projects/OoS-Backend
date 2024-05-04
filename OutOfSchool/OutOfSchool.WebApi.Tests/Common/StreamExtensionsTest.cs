using System;
using System.IO;
using Moq;
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

        var stream = streamMock.Object;

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => stream.ReadAndDeserializeFromJson<object>());
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
    public void SerializeToJsonAndWrite_WhenStreamIsNotReadable_ThrowNotSupportedException()
    {
        // Arrange
        var streamMock = new Mock<Stream>();

        streamMock.SetupGet(x => x.CanWrite).Returns(false);

        var stream = streamMock.Object;

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => stream.SerializeToJsonAndWrite(It.IsAny<object>()));
    }
}
