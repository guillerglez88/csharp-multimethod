namespace CsharpMultimethod.UnitTests;

public class InheritanceShape { }

public class InheritanceRect : InheritanceShape
{
    public double Wd { get; set; }
    public double Ht { get; set; }
}

public class InheritanceCircle : InheritanceShape
{
    public double Radius { get; set; }
}

public record TaggedShape(
    string Tag,
    Dictionary<string, object> Properties);

public record ObservationValue(
    int? Integer = null,
    string? String = null,
    bool? Boolean = null,
    DateTime? DateTime = null,
    Period? Period = null);

public record Period(
    DateTime? Start = null,
    DateTime? End = null);

public record Constant(int Value);
public record BinaryPlus(Constant Left, Constant Right);
public record UnariInc(Constant Val);
