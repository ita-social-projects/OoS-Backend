using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class EitherTest
{
    [Test]
    public async Task MapAsync_WithRightValue_MapsValue()
    {
        // Arrange
        Either<string, int> input = 5;
        var expected = 10;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.MapAsync(x => x * 2);

        // Assert
        Assert.IsTrue(result.Match(
            _ => false,
            right => right == expected));
    }

    [Test]
    public async Task MapAsync_WithLeftValue_PreservesLeft()
    {
        // Arrange
        var expected = "Error";
        Either<string, int> input = expected;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.MapAsync(x => x * 2);

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            right => false));
    }

    [Test]
    public async Task MapAsyncAsync_WithRightValue_MapsValueAsync()
    {
        // Arrange
        Either<string, int> input = 5;
        var expected = 10;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.MapAsync(
            async x =>
            {
                await Task.Delay(10);
                return x * 2;
            },
            null);

        // Assert
        Assert.IsTrue(result.Match(
            _ => false,
            right => right == expected));
    }

    [Test]
    public async Task MapAsyncAsync_WithLeftValue_PreservesLeft()
    {
        // Arrange
        var expected = "Error";
        Either<string, int> input = expected;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.MapAsync(
            async x =>
            {
                await Task.Delay(10);
                return x * 2;
            },
            null);

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            _ => false));
    }

    [Test]
    public async Task MapAsyncAsync_WithException_UsesExceptionHandler()
    {
        // Arrange
        Either<string, int> input = 5;
        var expected = "Exception: Test exception";
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.MapAsync<string, int, int>(
            async x =>
            {
                await Task.Delay(10); // Simulate async work
                throw new InvalidOperationException("Test exception");
            },
            ex => "Exception: " + ex.Message);

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            _ => false));
    }

    [Test]
    public async Task FlatMapAsync_WithRightValue_FlattensResult()
    {
        // Arrange
        Either<string, int> input = 5;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.FlatMapAsync(x =>
        {
            Either<string, double> res = x;
            return res;
        });

        // Assert
        Assert.IsTrue(result.Match(
            _ => false,
            right => Math.Abs(right - 5) < 0.01));
    }

    [Test]
    public async Task FlatMapAsync_WithLeftValue_PreservesLeft()
    {
        // Arrange
        var expected = "Error";
        Either<string, int> input = expected;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.FlatMapAsync(x =>
        {
            Either<string, double> res = x;
            return res;
        });

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            _ => false));
    }

    [Test]
    public async Task FlatMapAsyncAsync_WithRightValue_FlattensResultAsync()
    {
        // Arrange
        Either<string, int> input = 5;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.FlatMapAsync(
            async x =>
            {
                await Task.Delay(10);
                Either<string, double> res = x;
                return res;
            });

        // Assert
        Assert.IsTrue(result.Match(
            _ => false,
            right => Math.Abs(right - 5) < 0.01));
    }

    [Test]
    public async Task FlatMapAsyncAsync_WithLeftValue_PreservesLeft()
    {
        // Arrange
        var expected = "Error";
        Either<string, int> input = expected;
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.FlatMapAsync(
            async x =>
            {
                await Task.Delay(10);
                Either<string, double> res = x;
                return res;
            });

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            _ => false));
    }

    [Test]
    public async Task FlatMapAsyncAsync_WithException_UsesExceptionHandler()
    {
        // Arrange
        Either<string, int> input = 5;
        var expected = "Exception: Test exception";
        var eitherTask = Task.FromResult(input);

        // Act
        var result = await eitherTask.FlatMapAsync<string, int, double>(
            async x =>
            {
                await Task.Delay(10); // Simulate async work
                throw new InvalidOperationException("Test exception");
            },
            ex => "Exception: " + ex.Message);

        // Assert
        Assert.IsTrue(result.Match(
            left => left == expected,
            _ => false));
    }
}