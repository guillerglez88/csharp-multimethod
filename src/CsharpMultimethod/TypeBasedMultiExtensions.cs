using static CsharpMultimethod.MultimethodsModule;

namespace CsharpMultimethod;

public static class TypeBasedMultiExtensions
{
    public static Multi<T, Type, W> DefMulti<T, W>(Func<T, W> contract)
        => DefMulti<T, Type, W>((arg) => (Type)arg.GetType(), (t1, t2) => t1.FullName == t2.FullName);

    public static Multi<T, Type, W> DefMethod<T, S, W>(this Multi<T, Type, W> multi, Func<S, W> impl)
        where S : T
        => multi.DefMethod(typeof(S), (obj) => impl((S)obj));
}