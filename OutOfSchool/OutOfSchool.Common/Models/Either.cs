using System;
using System.Threading.Tasks;

namespace OutOfSchool.Common.Models;

/// <summary>
/// Functional data type to represent a disjoint
/// union of two possible types.
/// Instances of Either are either an instance of Left or Right.
/// FP Convention dictates that Left is used for "failure"
/// </summary>
/// <typeparam name="TL">Type of "Left" item.</typeparam>
/// <typeparam name="TR">Type of "Right" item.</typeparam>
public sealed class Either<TL, TR>
{
    private readonly IEither imp;

    private Either(IEither imp)
    {
        this.imp = imp;
    }

    private interface IEither
    {
        T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight);
    }

    public static implicit operator Either<TL, TR>(TL left) => CreateLeft(left);

    public static implicit operator Either<TL, TR>(TR right) => CreateRight(right);

    /// <summary>
    /// If right value is assigned, execute an action on it.
    /// </summary>
    /// <param name="rightAction">Action to execute.</param>
    public void DoRight(Action<TR> rightAction)
    {
        if (rightAction == null)
        {
            throw new ArgumentNullException(nameof(rightAction));
        }

        if (imp is Right right)
        {
            rightAction(right.right);
        }
    }

    public T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight)
    {
        return imp.Match(onLeft, onRight);
    }

    public Either<TL, T> FlatMap<T>(Func<TR, Either<TL, T>> fn)
    {
        if (fn is null)
        {
            throw new ArgumentNullException(nameof(fn));
        }

        return this switch
        {
            _ when imp is Left left => Either<TL, T>.CreateLeft(left.left),
            _ when imp is Right right => fn(right.right)
        };
    }

    public async Task<Either<TL, T>> FlatMapAsync<T>(Func<TR, Task<Either<TL, T>>> fn)
    {
        if (fn is null)
        {
            throw new ArgumentNullException(nameof(fn));
        }

        return this switch
        {
            _ when imp is Left left => Either<TL, T>.CreateLeft(left.left),
            _ when imp is Right right => await fn(right.right)
        };
    }

    public Either<TL, T> Map<T>(Func<TR, T> fn)
    {
        return FlatMap(fn.C(Either<TL, T>.CreateRight));
    }

    public async Task<Either<TL, T>> MapAsync<T>(Func<TR, Task<T>> fn)
    {
        return await FlatMapAsync(async c =>
        {
            var f = fn.C(async a => Either<TL, T>.CreateRight(await a));
            return await f(c);
        });
    }

    public override bool Equals(object obj)
    {
        return obj is Either<TL, TR> other && Equals(imp, other.imp);
    }

    public override int GetHashCode()
    {
        return imp.GetHashCode();
    }

    private static Either<TL, TR> CreateLeft(TL value)
    {
        return new Either<TL, TR>(new Left(value));
    }

    private static Either<TL, TR> CreateRight(TR value)
    {
        return new Either<TL, TR>(new Right(value));
    }

    private sealed class Left : IEither
    {
        public readonly TL left;

        public Left(TL left)
        {
            this.left = left;
        }

        public T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight)
        {
            return onLeft(left);
        }

        public override bool Equals(object obj)
        {
            return obj is Left other && Equals(left, other.left);
        }

        public override int GetHashCode()
        {
            return left.GetHashCode();
        }
    }

    private sealed class Right : IEither
    {
        public readonly TR right;

        public Right(TR right)
        {
            this.right = right;
        }

        public T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight)
        {
            return onRight(right);
        }

        public override bool Equals(object obj)
        {
            return obj is Right other && Equals(right, other.right);
        }

        public override int GetHashCode()
        {
            return right.GetHashCode();
        }
    }
}

public static class EitherExtensions
{
    public static Func<TA, TC> C<TA, TB, TC>(this Func<TA, TB> fun, Func<TB, TC> f)
    {
        return a => f(fun(a));
    }
}