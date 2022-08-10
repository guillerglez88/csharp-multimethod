using static CsharpMultimethod.MultimethodsModule;

namespace CsharpMultimethod.UnitTests;

public class MultimethodsTests
{
    [Fact]
    public void CanUsePropBasedMulti()
    {
        var area = DefMulti((TaggedShape shape) => default(double), shape => shape.Tag);
        area = area.DefMethod("rect", (TaggedShape rect) => (double)rect.Properties["Wd"] * (double)rect.Properties["Ht"]);
        area = area.DefMethod("circle", (TaggedShape circle) => Math.PI * ((double)circle.Properties["Radius"] * (double)circle.Properties["Radius"]));

        var rectArea = area.Invoke(new TaggedShape { 
            Tag = "rect", 
            Properties = new() { ["Wd"] = 2.0, ["Ht"] = 3.0 }});
        var circleArea = area.Invoke(new TaggedShape {
            Tag = "circle",
            Properties = new() { ["Radius"] = 2.0 } });

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }

    [Fact]
    public void CanUseTypeBasedMulti()
    {
        var area = DefMulti((InheritanceShape shape) => default(double));
        area = area.DefMethod((InheritanceRect rect) => rect.Wd * rect.Ht);
        area = area.DefMethod((InheritanceCircle circle) => Math.PI * (circle.Radius * circle.Radius));
        
        var rectArea = area.Invoke(new InheritanceRect { Wd = 2, Ht = 3 });
        var circleArea = area.Invoke(new InheritanceCircle { Radius = 2 });

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }
}
