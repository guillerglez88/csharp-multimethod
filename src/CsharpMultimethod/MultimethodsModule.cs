namespace CsharpMultimethod;

public static class MultimethodsModule
{
    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, D> dispatch,
        Func<D, D, bool>? matches = null)
        => new Multi<T, D, W>(
            Dispatch: (args) => dispatch(args),
            Matches: matches ?? new Func<D, D, bool>((d1, d2) => Equals(d1, d2)),
            Methods: Enumerable.Empty<(D, Func<T, W>)>(),
            Default: ThrowNotImplemented<D, T, W>); 

    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, W> contract,
        Func<T, D> dispatch,
        Func<D, D, bool>? matches = null)
        => DefMulti<T, D, W>(dispatch, matches ?? new Func<D, D, bool>((d1, d2) => Equals(d1, d2)));

    public static Multi<T, D, W> DefMethod<T, D, W>(
        this Multi<T, D, W> multi,
        D dispatchingVal,
        Func<T, W> impl)
        => multi with { Methods = multi.Methods.Append((key: dispatchingVal, method: impl)) };

    public static Multi<T, D, W> DefDefault<T, D, W>(
        this Multi<T, D, W> multi,
        Func<D, T, W> impl)
        => multi with { Default = impl };

    public static W Invoke<T, D, W>(this Multi<T, D, W> multi, T arg)
    {
        var dispatchingVal = multi.Dispatch(arg);

        return multi.Methods
            .Where(entry => multi.Matches(entry.key, dispatchingVal))
            .Select(entry => entry.method)
            .Append((arg) => multi.Default(dispatchingVal, arg))
            .First()
            .Invoke(arg);
    }

    public static W ThrowNotImplemented<D, T, W>(D dispatchingVal, T arg)
        => throw new NotImplementedException($"Implementation not found for: {dispatchingVal}");
}
