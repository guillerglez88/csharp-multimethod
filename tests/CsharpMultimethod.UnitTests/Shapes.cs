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