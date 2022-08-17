using System.Collections.Concurrent;

namespace CsharpMultimethod;

public class Multi<T, D, W>
{
    private readonly Func<T, D> dispatch;
    private readonly Func<D, D, bool> matches;
    private readonly ConcurrentBag<(D key, Func<T, W> method)> methods;
    private Func<D, T, W> defaultMethod;

    public Multi(
        Func<T, D> dispatch,
        Func<D, D, bool>? matches = null)
    {
        this.dispatch = dispatch;
        this.matches = matches ?? new Func<D, D, bool>((d1, d2) => Equals(d1, d2));
        methods = new ConcurrentBag<(D key, Func<T, W> method)>();
        defaultMethod = ThrowNotImplemented;
    }

    public Multi<T, D, W> DefMethod(D dispatchingVal, Func<T, W> impl)
    {
        methods.Add((key: dispatchingVal, method: impl));

        return this;
    }

    public Multi<T, D, W> DefDefault(Func<D, T, W> impl)
    {
        defaultMethod = impl;

        return this;
    }

    public W Invoke(T arg)
    {
        var dispatchingVal = dispatch(arg);

        return methods
            .Where(entry => matches(entry.key, dispatchingVal))
            .Select(entry => entry.method)
            .Append((arg) => defaultMethod(dispatchingVal, arg))
            .First()
            .Invoke(arg);
    }

    private static W ThrowNotImplemented(D dispatchingVal, T arg)
        => throw new NotImplementedException($"Implementation not found for: {dispatchingVal}");
}

public static class Multi
{
    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, D> dispatch,
        Func<D, D, bool>? matches = null)
        => new Multi<T, D, W>(dispatch, matches);

    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, W> contract,
        Func<T, D> dispatch,
        Func<D, D, bool>? matches = null)
        => DefMulti<T, D, W>(dispatch, matches);
}
