using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SkiaSharp;

namespace OutOfSchool.WebApi.IntegrationTests.SkiaSharp;

[TestFixture]
public class SkiaSharpTests
{
    private static readonly string SkiaSharpDirectoryAbsolutePathValue = GetSkiaSharpDirectoryAbsolutePath();

    [Test]
    public void CreateSkData_WhenStreamIsValidImage_ShouldBeNotNullSKDataInstance()
    {
        // Arrange
        using var stream = File.OpenRead(GetAbsoluteFilePathFromSkiaSharpDirectory(FileNames.Image1920X1080Jpg));

        // Act
        using var skData = SKData.Create(stream);

        // Assert
        skData.Should().NotBeNull();
    }

    [Test]
    public void CreateSkCodec_WhenStreamIsValidImage_ShouldBeNotNullSkCodecInstance()
    {
        // Arrange
        using var stream = File.OpenRead(GetAbsoluteFilePathFromSkiaSharpDirectory(FileNames.Image1920X1080Jpg));
        using var skData = SKData.Create(stream);

        // Act
        using var skCodec = SKCodec.Create(skData);

        // Assert
        skCodec.Should().NotBeNull();
    }

    [Test]
    public void GetSkCodecEncodedFormat_WhenStreamIsValidImage_ShouldNotBeNull()
    {
        // Arrange
        using var stream = File.OpenRead(GetAbsoluteFilePathFromSkiaSharpDirectory(FileNames.Image1920X1080Jpg));
        using var skData = SKData.Create(stream);
        using var skCodec = SKCodec.Create(skData);

        // Act
        var encodedFormat = skCodec.EncodedFormat.ToString();

        // Assert
        encodedFormat.Should().NotBeNull();
    }

    [Test]
    public void GetSkCodecInfo_WhenStreamIsValidImage_ShouldNotBeNull()
    {
        // Arrange
        using var stream = File.OpenRead(GetAbsoluteFilePathFromSkiaSharpDirectory(FileNames.Image1920X1080Jpg));
        using var skData = SKData.Create(stream);
        using var skCodec = SKCodec.Create(skData);

        // Act
        var info = skCodec.Info;

        // Assert
        info.Should().NotBeNull();
    }

    private static string GetAbsoluteFilePathFromSkiaSharpDirectory(string filename)
        => Path.Combine(SkiaSharpDirectoryAbsolutePathValue, filename);

    private static string GetSkiaSharpDirectoryAbsolutePath()
    {
        var parentDirectory = Path.GetDirectoryName(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName)
                ?? throw new DirectoryNotFoundException("Unreal to find directory with test data");
        return Path.Combine(parentDirectory, "SkiaSharp");
    }
}