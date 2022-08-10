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

public class TaggedShape
{
    public string Tag { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Observation
{
    public string Code  { get; set; }
    public ObservationValue Value { get; set; }
}

public class ObservationValue
{
    public int? Integer { get; set; }
    public string? String { get; set; }
    public bool? Boolean { get; set; }
    public DateTime? DateTime { get; set; }
    public Period? Period { get; set; }
}

public class Period
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}