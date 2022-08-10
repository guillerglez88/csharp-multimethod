using System.Globalization;
using static CsharpMultimethod.MultimethodsModule;
using static CsharpMultimethod.TypeBasedMultiExtensions;

namespace CsharpMultimethod.UnitTests;

public class MultimethodsTests
{
    [Fact]
    public void CanUsePropBasedMulti()
    {
        var area = DefMulti((TaggedShape shape) => default(double), shape => shape.Tag);
        area = area.DefMethod("rect", (TaggedShape rect) => (double)rect.Properties["Wd"] * (double)rect.Properties["Ht"]);
        area = area.DefMethod("circle", (TaggedShape circle) => Math.PI * ((double)circle.Properties["Radius"] * (double)circle.Properties["Radius"]));

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
        var area = DefMulti((InheritanceShape shape) => default(double));
        area = area.DefMethod((InheritanceRect rect) => rect.Wd * rect.Ht);
        area = area.DefMethod((InheritanceCircle circle) => Math.PI * (circle.Radius * circle.Radius));

        var rectArea = area.Invoke(new InheritanceRect { Wd = 2, Ht = 3 });
        var circleArea = area.Invoke(new InheritanceCircle { Radius = 2 });

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }

    [Fact]
    public void CanDispatchOnValueExistance()
    {
        var obsValProps = typeof(ObservationValue).GetProperties();
        var stringifyObs = DefMulti(
            contract: (Observation obs) => default(string), 
            dispatch: obs => obsValProps.FirstOrDefault(prop => prop.GetValue(obs.Value) != null)?.Name);
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Integer), 
            impl: (Observation intObs) => $"{intObs.Code}: {intObs.Value.Integer}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.String), 
            impl: (Observation strObs) => $"{strObs.Code}: {strObs.Value.String}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Boolean), 
            impl: (Observation boolObs) => $"{boolObs.Code}: {(boolObs.Value.Boolean == true ? "YES" : "NO")}");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.DateTime), 
            impl: (Observation dateObs) => $"{dateObs.Code}: {dateObs.Value.DateTime?.ToString("d")} 📅");
        stringifyObs = stringifyObs.DefMethod(nameof(ObservationValue.Period), 
            impl: (Observation periodObs) => $"{periodObs.Code}: [{periodObs.Value.Period.Start?.ToString("d")} - {periodObs.Value.Period.End?.ToString("d")}]");
        
        var strHt = stringifyObs.Invoke(new() { 
            Code = "height",
            Value = new () { Integer = 172 } });
        var strSmokes = stringifyObs.Invoke(new() { 
            Code = "smokes",
            Value = new () { Boolean = false } });
        var strCoB = stringifyObs.Invoke(new() { 
            Code = "birth-country",
            Value = new () { String = "Cuba" } });
        var strDoB = stringifyObs.Invoke(new() { 
            Code = "birth-date",
            Value = new () { DateTime = new DateTime(1988, 4, 18) } });
        var strValidity = stringifyObs.Invoke(new() { 
            Code = "validity-period",
            Value = new () { Period = new() {
                Start = new DateTime(2022, 2, 14),
                End = new DateTime(2023, 2, 14) } } });
        
        Assert.Equal("height: 172", strHt);
        Assert.Equal("smokes: NO", strSmokes);
        Assert.Equal("birth-country: Cuba", strCoB);
        Assert.Equal("birth-date: 04/18/1988 📅", strDoB);
        Assert.Equal("validity-period: [02/14/2022 - 02/14/2023]", strValidity);
    }
}
