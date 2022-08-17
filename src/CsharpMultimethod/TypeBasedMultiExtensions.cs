using static CsharpMultimethod.Multi;

namespace CsharpMultimethod;

public static class TypeBasedMultiExtensions
{
    public static Multi<T, Type?, W> DefMulti<T, W>(
        Func<T, W> contract,
        Func<T, Type?> dispatch)
        => DefMulti<T, Type?, W>(contract, dispatch, (t1, t2) => t1?.FullName == t2?.FullName);

    public static Multi<T, Type?, W> DefMethod<T, S, W>(
        this Multi<T, Type?, W> multi, 
        Func<S, W> impl)
        where S : T
        => multi.DefMethod(typeof(S), (arg) => impl((S)arg));
    
    public static Func<T, Type?> DispatchByType<T>()
        => (arg) => arg == null ? default : arg.GetType();
}