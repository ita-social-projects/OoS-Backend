using System;
using System.Threading.Tasks;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Models;

[TestFixture]
public class EitherTests
{
    private const string Left = "Left";
    private const int Right = 42;

    [Test]
    public void ImplicitConversion_ToLeft_SetsLeftValue()
    {
        Either<string, int> either = Left;
        either.AssertLeft(error => { Assert.AreEqual(Left, error); });
    }

    [Test]
    public void ImplicitConversion_ToRight_SetsRightValue()
    {
        Either<string, int> either = Right;
        either.AssertRight(value => { Assert.AreEqual(Right, value); });
    }

    [Test]
    public void TryGetLeft_WhenLeft_ReturnsTrueAndValue()
    {
        Either<string, int> either = Left;
        Assert.IsTrue(either.TryGetLeft(out var leftVal));
        Assert.AreEqual(Left, leftVal);
    }

    [Test]
    public void TryGetLeft_WhenRight_ReturnsFalse()
    {
        var either = (Either<string, int>) Right;
        Assert.IsFalse(either.TryGetLeft(out var leftVal));
        Assert.AreEqual(default(string), leftVal);
    }

    [Test]
    public void TryGetRight_WhenRight_ReturnsTrueAndValue()
    {
        var either = (Either<string, int>) Right;
        Assert.IsTrue(either.TryGetRight(out var rightVal));
        Assert.AreEqual(Right, rightVal);
    }

    [Test]
    public void TryGetRight_WhenLeft_ReturnsFalse()
    {
        var either = (Either<string, int>) Left;
        Assert.IsFalse(either.TryGetRight(out var rightVal));
        Assert.AreEqual(default(int), rightVal);
    }

    [Test]
    public void DoLeft_WhenLeft_ExecutesAction()
    {
        var either = (Either<string, int>) Left;
        string captured = null;
        either.DoLeft(val => captured = val);
        Assert.AreEqual(Left, captured);
    }

    [Test]
    public void DoLeft_WhenRight_DoesNotExecuteAction()
    {
        var either = (Either<string, int>) Right;
        string captured = null;
        either.DoLeft(val => captured = val);
        Assert.IsNull(captured);
    }

    [Test]
    public void DoLeft_NullAction_Throws()
    {
        var either = (Either<string, int>) Left;
        Assert.Throws<ArgumentNullException>(() => either.DoLeft(null));
    }

    [Test]
    public void DoRight_WhenRight_ExecutesAction()
    {
        var either = (Either<string, int>) Right;
        var captured = 0;
        either.DoRight(val => captured = val);
        Assert.AreEqual(Right, captured);
    }

    [Test]
    public void DoRight_WhenLeft_DoesNotExecuteAction()
    {
        var either = (Either<string, int>) Left;
        var captured = 0;
        either.DoRight(val => captured = val);
        Assert.AreEqual(0, captured);
    }

    [Test]
    public void DoRight_NullAction_Throws()
    {
        var either = (Either<string, int>) Right;
        Assert.Throws<ArgumentNullException>(() => either.DoRight(null));
    }

    [Test]
    public void Match_WhenLeft_InvokesOnLeft()
    {
        var either = (Either<string, int>) Left;
        var result = either.Match(l => l.Length, r => r / 2);
        Assert.AreEqual(Left.Length, result);
    }

    [Test]
    public void Match_WhenRight_InvokesOnRight()
    {
        var either = (Either<string, int>) Right;
        var result = either.Match(l => l.Length, r => r / 2);
        Assert.AreEqual(21, result);
    }

    [Test]
    public void FlatMap_WhenLeft_ReturnsLeft()
    {
        var either = (Either<string, int>) Left;
        var flattened = either.FlatMap(r => (Either<string, double>) (r / 2.0));
        flattened.AssertLeft(error =>
        {
            Assert.AreEqual(Left, error);
        });
    }

    [Test]
    public void FlatMap_WhenRight_TransformsValue()
    {
        var either = (Either<string, int>) Right;
        var flattened = either.FlatMap(r => (Either<string, double>) (r / 2.0));
        flattened.AssertRight(value =>
        {
            Assert.AreEqual(21.0, value);
        });
    }

    [Test]
    public void Map_WhenLeft_ReturnsLeft()
    {
        var either = (Either<string, int>) Left;
        var mapped = either.Map(r => r.ToString());
        mapped.AssertLeft(error =>
        {
            Assert.AreEqual(Left, error);
        });
    }

    [Test]
    public void Map_WhenRight_AppliesFunction()
    {
        var either = (Either<string, int>) Right;
        var mapped = either.Map(r => r / 2);
        mapped.AssertRight(value =>
        {
            Assert.AreEqual(21, value);
        });
    }

    [Test]
    public async Task FlatMapAsync_WhenLeft_ReturnsLeftImmediately()
    {
        var either = (Either<string, int>) Left;
        var result = await either.FlatMapAsync(async r =>
        {
            await Task.Delay(10);
            return (Either<string, double>) (r / 2.0);
        });
        result.AssertLeft(error =>
        {
            Assert.AreEqual(Left, error);
        });
    }

    [Test]
    public async Task FlatMapAsync_WhenRight_UsesAsyncFunction()
    {
        var either = (Either<string, int>) Right;
        var result = await either.FlatMapAsync(async r =>
        {
            await Task.Delay(10);
            return (Either<string, double>) (r / 2.0);
        });
        result.AssertRight(value =>
        {
            Assert.AreEqual(21.0, value);
        });
    }

    [Test]
    public async Task MapAsync_WhenLeft_ReturnsLeft()
    {
        var either = (Either<string, int>) Left;
        var result = await either.MapAsync(async r =>
        {
            await Task.Delay(10);
            return r.ToString();
        });
        result.AssertLeft(error =>
        {
            Assert.AreEqual(Left, error);
        });
    }

    [Test]
    public async Task MapAsync_WhenRight_AppliesAsyncFunction()
    {
        var either = (Either<string, int>) Right;
        var result = await either.MapAsync(async r =>
        {
            await Task.Delay(10);
            return r / 2;
        });
        result.AssertRight(value =>
        {
            Assert.AreEqual(21, value);
        });
    }

    [Test]
    public void Equals_WhenBothLeftAndEqual_ReturnsTrue()
    {
        Either<string, int> e1 = Left;
        Either<string, int> e2 = Left;

        Assert.IsTrue(e1.Equals(e2));
    }

    [Test]
    public void Equals_WhenBothRightAndEqual_ReturnsTrue()
    {
        Either<string, int> e1 = Right;
        Either<string, int> e2 = Right;

        Assert.IsTrue(e1.Equals(e2));
    }

    [Test]
    public void Equals_WhenBothLeftButDifferent_ReturnsFalse()
    {
        Either<string, int> e1 = "One";
        Either<string, int> e2 = "Two";

        Assert.IsFalse(e1.Equals(e2));
    }

    [Test]
    public void Equals_WhenBothRightButDifferent_ReturnsFalse()
    {
        Either<string, int> e1 = 42;
        Either<string, int> e2 = 43;

        Assert.IsFalse(e1.Equals(e2));
    }

    [Test]
    public void Equals_WhenDifferentSides_ReturnsFalse()
    {
        Either<string, int> e1 = Left;
        Either<string, int> e2 = Right;

        Assert.IsFalse(e1.Equals(e2));
    }

    [Test]
    public void GetHashCode_WhenEqualEithers_AreEqual()
    {
        Either<string, int> e1 = Left;
        Either<string, int> e2 = Left;

        Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());
    }

    [Test]
    public void GetHashCode_WhenDifferentEithers_MayDiffer()
    {
        Either<string, int> e1 = "Value";
        Either<string, int> e2 = "AnotherValue";

        Assert.AreNotEqual(e1.GetHashCode(), e2.GetHashCode());
    }
}