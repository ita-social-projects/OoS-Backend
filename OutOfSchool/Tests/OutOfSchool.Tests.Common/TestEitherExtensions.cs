using System;
using NUnit.Framework;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Tests.Common;

public static class TestEitherExtensions
{
    public static void AssertRight<TL, TR>(this Either<TL, TR> either, Action<TR> assertion) => either.Match<object>(
        _ =>
        {
            Assert.Fail();
            return null;
        },
        right =>
        {
            assertion(right);
            return null;
        });

    public static void AssertLeft<TL, TR>(this Either<TL, TR> either, Action<TL> assertion) => either.Match<object>(
        left =>
        {
            assertion(left);
            return null;
        },
        _ =>
        {
            Assert.Fail();
            return null;
        });
}