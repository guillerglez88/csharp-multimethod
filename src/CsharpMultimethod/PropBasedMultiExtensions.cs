using System.Linq.Expressions;
using System.Reflection;
using static CsharpMultimethod.MultimethodsModule;

namespace CsharpMultimethod;

public static class PropBasedMultiExtensions
{
    public static Multi<T, PropertyInfo?, W> DefMulti<T, W>(
        Func<T, W> contract,
        Func<T, PropertyInfo?> dispatch)
        => DefMulti<T, PropertyInfo?, W>(contract, dispatch, (p1, p2) => p1?.Name == p2?.Name);

    public static Multi<T, PropertyInfo?, W> DefMethod<T, W>(
        this Multi<T, PropertyInfo?, W> multi,
        string dispatchingVal,
        Func<T, W> impl)
    {
        var prop = typeof(T).GetProperty(dispatchingVal, BindingFlags.Public | BindingFlags.Instance);

        return multi.DefMethod(prop, (arg) => impl(arg));
    }

    public static Func<T, PropertyInfo?> DispatchByProp<T>()
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return (arg) => props.FirstOrDefault(prop => prop.GetValue(arg) != null);
    }
}