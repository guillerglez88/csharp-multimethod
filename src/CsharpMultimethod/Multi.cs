namespace CsharpMultimethod;

public record Multi<T, D, W>(
    Func<T, D> Dispatch, 
    Func<D, D, bool> Matches,
    IEnumerable<(D key, Func<T, W> method)> Methods) where D : notnull;