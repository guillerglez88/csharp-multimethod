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

        var rectArea = area.Invoke(new TaggedShape(
            Tag: "rect",
            Properties: new() { ["Wd"] = 2.0, ["Ht"] = 3.0 }));
        var circleArea = area.Invoke(new TaggedShape(
            Tag: "circle",
            Properties: new() { ["Radius"] = 2.0 }));

        Assert.Equal(6, rectArea);
        Assert.Equal(12.57, Math.Round(circleArea, 2));
    }

    [Fact]
    public void CanUseMultipleDispatching()
    {
        var eats = DefMulti(
            contract: ((Animal animal, Food food) pair) => default(bool),
            dispatch: (pair) => (pair.animal.Kind, pair.food.Type));
        eats = eats.DefMethod(("lion", FoodType.Meat), impl: (_) => true);
        eats = eats.DefMethod(("cow", FoodType.Vegetable), impl: (_) => true);
        eats = eats.DefMethod(("dog", FoodType.Meat), impl: (_) => true);
        eats = eats.DefMethod(("dog", FoodType.Eggs), impl: (_) => true);
        eats = eats.DefDefault((_,_) => false);

        var lionEatsVegetable = eats.Invoke((new Animal(Kind: "lion"), new Food(Type: FoodType.Vegetable)));
        var lionEatsMeat = eats.Invoke((new Animal(Kind: "lion"), new Food(Type: FoodType.Meat)));
        var dogEatsEggs = eats.Invoke((new Animal(Kind: "dog"), new Food(Type: FoodType.Eggs)));
        var cowEatsVegetable = eats.Invoke((new Animal(Kind: "cow"), new Food(Type: FoodType.Vegetable)));

        Assert.False(lionEatsVegetable);
        Assert.True(lionEatsMeat);
        Assert.True(dogEatsEggs);
        Assert.True(cowEatsVegetable);
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

        var strHt = stringifyObs.Invoke(new ( Integer: 172 ));
        var strSmokes = stringifyObs.Invoke(new ( Boolean: false ));
        var strCoB = stringifyObs.Invoke(new ( String: "Cuba" ));
        var strDoB = stringifyObs.Invoke(new ( DateTime: new DateTime(1988, 4, 18) ));
        var strValidity = stringifyObs.Invoke(new (
            Period: new ( 
                Start: new DateTime(2022, 2, 14),
                End: new DateTime(2023, 2, 14))));

        Assert.Equal("172", strHt);
        Assert.Equal("NO", strSmokes);
        Assert.Equal("Cuba", strCoB);
        Assert.Equal("4/18/1988 📅", strDoB);
        Assert.Equal("[2/14/2022 - 2/14/2023]", strValidity);
    }

    [Fact]
    public void CanSolveExpressionProblem()
    {
        var eval = DefMulti(
            contract: (dynamic exp) => default(Constant),
            dispatch: DispatchByType<dynamic>());
        eval = eval.DefMethod(
            impl: (Constant constant) => constant);
        eval = eval.DefMethod(
            impl: (BinaryPlus plus) => new Constant(eval.Invoke(plus.Left).Value + eval.Invoke(plus.Right).Value));
        eval = eval.DefMethod(
            impl: (UnariInc inc) => eval.Invoke(new BinaryPlus(
                Left: inc.Val,
                Right: new Constant(Value: 1))));

        var stringify = DefMulti(
            contract: (dynamic exp) => default(string),
            dispatch: DispatchByType<dynamic>());
        stringify = stringify.DefMethod(
            impl: (Constant constant) => $"{constant.Value}");
        stringify = stringify.DefMethod(
            impl: (BinaryPlus plus) => $"{stringify.Invoke(plus.Left)} + {stringify.Invoke(plus.Right)}");
        stringify = stringify.DefMethod(
            impl: (UnariInc inc) => $"{stringify.Invoke(inc.Val)}++");

        var addition = new BinaryPlus(
            Left: new Constant(2),
            Right: new Constant(3));
        var increment = new UnariInc(Val: new Constant(Value: 6));

        var addResult = eval.Invoke(addition);
        var incResult = eval.Invoke(increment);
        var strAddition = stringify.Invoke(addition);
        var strIncrement = stringify.Invoke(increment);

        Assert.Equal(5, addResult.Value);
        Assert.Equal(7, incResult.Value);
        Assert.Equal("2 + 3", strAddition);
        Assert.Equal("6++", strIncrement);
    }
}
