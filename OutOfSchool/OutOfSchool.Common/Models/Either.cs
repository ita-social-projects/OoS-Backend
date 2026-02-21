using System;
using System.Threading.Tasks;

namespace OutOfSchool.Common.Models;

/// <summary>
/// Functional data type to represent a disjoint
/// union of two possible types.
/// Instances of Either are either an instance of Left or Right.
/// FP Convention dictates that Left is used for "failure".
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
    /// If left value is assigned, execute an action on it.
    /// </summary>
    /// <param name="leftAction">Action to execute.</param>
    public void DoLeft(Action<TL> leftAction)
    {
        ArgumentNullException.ThrowIfNull(leftAction);

        if (imp is Left left)
        {
            leftAction(left.Value);
        }
    }

    /// <summary>
    /// Tries to get the Left value.
    /// </summary>
    /// <param name="value">The Left value if present.</param>
    /// <returns>true if the Either is Left; otherwise, false.</returns>
    public bool TryGetLeft(out TL value)
    {
        if (imp is Left left)
        {
            value = left.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// If right value is assigned, execute an action on it.
    /// </summary>
    /// <param name="rightAction">Action to execute.</param>
    public void DoRight(Action<TR> rightAction)
    {
        ArgumentNullException.ThrowIfNull(rightAction);

        if (imp is Right right)
        {
            rightAction(right.Value);
        }
    }

    /// <summary>
    /// Tries to get the Right value.
    /// </summary>
    /// <param name="value">The Right value if present.</param>
    /// <returns>true if the Either is Right; otherwise, false.</returns>
    public bool TryGetRight(out TR value)
    {
        if (imp is Right right)
        {
            value = right.Value;
            return true;
        }

        value = default;
        return false;
    }

    public T Match<T>(Func<TL, T> onLeft, Func<TR, T> onRight)
    {
        return imp.Match(onLeft, onRight);
    }

    public Either<TL, T> FlatMap<T>(Func<TR, Either<TL, T>> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        return this switch
        {
            _ when imp is Left left => Either<TL, T>.CreateLeft(left.Value),
            _ when imp is Right right => fn(right.Value),
            _ => throw new NotImplementedException("This should not happen.")
        };
    }

    public Task<Either<TL, T>> FlatMapAsync<T>(Func<TR, Task<Either<TL, T>>> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        return InternalFlatMapAsync(fn);
    }

    public Either<TL, T> Map<T>(Func<TR, T> fn)
    {
        return FlatMap(fn.C(Either<TL, T>.CreateRight));
    }

    public Task<Either<TL, T>> MapAsync<T>(Func<TR, Task<T>> fn)
    {
        ArgumentNullException.ThrowIfNull(fn);

        return InternalMapAsync(fn);
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

    private async Task<Either<TL, T>> InternalFlatMapAsync<T>(Func<TR, Task<Either<TL, T>>> fn)
    {
        return this switch
        {
            _ when imp is Left left => Either<TL, T>.CreateLeft(left.Value),
            _ when imp is Right right => await fn(right.Value),
            _ => throw new NotImplementedException("This should not happen.")
        };
    }

    private Task<Either<TL, T>> InternalMapAsync<T>(Func<TR, Task<T>> fn)
    {
        return FlatMapAsync(async c =>
        {
            var f = fn.C(async a => Either<TL, T>.CreateRight(await a));
            return await f(c);
        });
    }

    private sealed class Left : IEither
    {
        private readonly TL left;

        public Left(TL left)
        {
            this.left = left;
        }

        public TL Value => left;

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
        private readonly TR right;

        public Right(TR right)
        {
            this.right = right;
        }

        public TR Value => right;

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