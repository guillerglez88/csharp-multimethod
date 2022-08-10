using System.Globalization;
using static CsharpMultimethod.MultimethodsModule;
using static CsharpMultimethod.TypeBasedMultiExtensions;
using static CsharpMultimethod.PropBasedMultiExtensions;

namespace CsharpMultimethod.UnitTests;

public class MultimethodsTests
{
    [Fact]
    public void CanUseValueBasedMulti()
    {
        var area = DefMulti(
            contract: (TaggedShape shape) => default(double), 
            dispatch: (shape) => shape.Tag);
        area = area.DefMethod("rect", (rect) => (double)rect.Properties["Wd"] * (double)rect.Properties["Ht"]);
        area = area.DefMethod("circle", (circle) => Math.PI * ((double)circle.Properties["Radius"] * (double)circle.Properties["Radius"]));

        var rectArea = area.Invoke(new TaggedShape
        {
            Tag = "rect",
            Properties = new() { ["Wd"] = 2.0, ["Ht"] = 3.0 }
        });
        var circleArea = area.Invoke(new TaggedShape
        {
            Tag = "circle",
            Properties = new() { ["Radius"] = 2.0 }
        });

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }

    [Fact]
    public void CanUseTypeBasedMulti()
    {
        var area = DefMulti(
            contract: (InheritanceShape shape) => default(double),
            dispatch: DispatchByType<InheritanceShape>());
        area = area.DefMethod((InheritanceRect rect) => rect.Wd * rect.Ht);
        area = area.DefMethod((InheritanceCircle circle) => Math.PI * (circle.Radius * circle.Radius));

        var rectArea = area.Invoke(new InheritanceRect { Wd = 2, Ht = 3 });
        var circleArea = area.Invoke(new InheritanceCircle { Radius = 2 });

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }

    [Fact]
    public void CanUsePropBasedMulti()
    {
        var stringifyObs = DefMulti(
            contract: (ObservationValue obsVal) => default(string),
            dispatch: DispatchByProp<ObservationValue>());
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Integer), 
            impl: (intVal) => $"{intVal.Integer}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.String), 
            impl: (strVal) => $"{strVal.String}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Boolean), 
            impl: (boolVal) => $"{(boolVal.Boolean == true ? "YES" : "NO")}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.DateTime), 
            impl: (dateVal) => $"{dateVal.DateTime?.ToString("d")} 📅");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Period), 
            impl: (periodVal) => $"[{periodVal.Period?.Start?.ToString("d")} - {periodVal.Period?.End?.ToString("d")}]");
        
        var strHt = stringifyObs.Invoke(new () { Integer = 172 });
        var strSmokes = stringifyObs.Invoke(new () { Boolean = false });
        var strCoB = stringifyObs.Invoke(new () { String = "Cuba" });
        var strDoB = stringifyObs.Invoke(new () { DateTime = new DateTime(1988, 4, 18) });
        var strValidity = stringifyObs.Invoke(new () { 
            Period = new() {
                Start = new DateTime(2022, 2, 14),
                End = new DateTime(2023, 2, 14) } });
        
        Assert.Equal("172", strHt);
        Assert.Equal("NO", strSmokes);
        Assert.Equal("Cuba", strCoB);
        Assert.Equal("4/18/1988 📅", strDoB);
        Assert.Equal("[2/14/2022 - 2/14/2023]", strValidity);
    }
}
