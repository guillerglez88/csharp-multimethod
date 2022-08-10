namespace CsharpMultimethod;

public static class MultimethodsModule
{
    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, D> dispatch, 
        Func<D, D, bool>? matches = null)
            where D : notnull
            => new Multi<T, D, W>(
                Dispatch: (args) => dispatch(args),
                Matches: matches ?? new Func<D, D, bool>((d1, d2) => Equals(d1, d2)),
                Methods: Enumerable.Empty<(D, Func<T, W>)>());

    public static Multi<T, D, W> DefMulti<T, D, W>(
        Func<T, W> contract,
        Func<T, D> dispatch,
        Func<D, D, bool>? matcher = null)
        where D : notnull
        => new Multi<T, D, W>(
            Dispatch: (args) => dispatch(args),
            Matches: matcher ?? new Func<D, D, bool>((d1, d2) => Equals(d1, d2)),
            Methods: Enumerable.Empty<(D, Func<T, W>)>());
    
    public static Multi<T, D, W> DefMethod<T, D, W>(
        this Multi<T, D, W> multi,
        D dispatchingVal,
        Func<T, W> impl)
        where D : notnull
        => multi with { Methods = multi.Methods.Append((key: dispatchingVal, method: impl)) };

    public static W Invoke<T, D, W>(this Multi<T, D, W> multi, T arg)
        where D : notnull
    {
        var dispatchingVal = multi.Dispatch(arg);

        return multi.Methods
            .Where(entry => multi.Matches(entry.key, dispatchingVal))
            .Select(entry => entry.method)
            .Append((_) => { throw new NotImplementedException($"Implementation not found for: {dispatchingVal}"); })
            .First()
            .Invoke(arg);
    }

    public static Multi<T, Type, W> DefMulti<T, W>(Func<T, W> contract)
        => DefMulti<T, Type, W>((arg) => (Type)arg.GetType(), (t1, t2) => t1.FullName == t2.FullName);

    public static Multi<T, Type, W> DefMethod<T, S, W>(
        this Multi<T, Type, W> multi,
        Func<S, W> impl)
        where S : T
        => multi.DefMethod(typeof(S), (obj) => impl((S)obj));
}
