# csharp-multimethod
Exploring clojure multimethod from c#

## Methods

```
DefMulti: (contract: (T) -> W,
           dispatch: (T) -> D) -> Multi
```

Define a multi-method, should provide contract and dispatch. Contract should be a sample function, useful for defining the signature of the multi-method, it is not necessary to implement anything for this function since it won't ever be called, so providing default values is enough. Dispatch function computes a value used by the multi-method to choose the implementation to invoke.

```
DefMethod: (dispatchingVal: D,
            impl: (T) -> W) -> Multi
```

Extends multi-method with specific implementation for dispatchingVal.

```
DefDefault: (impl: (dispatchingVal: D, arg: T) -> W) -> Multi
```

Provide a default implementation for handling argument when no accurate implementation is found for the specific `dispatchingVal`.

```
Invoke: (arg: T) -> W
```

Invoke the multi-method.

## Value based dispatching

```csharp
public record TaggedShape(
    string Tag,
    Dictionary<string, object> Props);
```

```csharp
var area = DefMulti(
	contract: (TaggedShape shape) => default(double),
	dispatch: (shape) => shape.Tag);

area.DefMethod("rect", (rect) => (double)rect.Props["Wd"] * (double)rect.Props["Ht"]);
area.DefMethod("circle", (circle) => Math.PI * ((double)circle.Props["Radius"] * (double)circle.Props["Radius"]));

var rect = new TaggedShape("rect", new() { ["Wd"] = 2.0, ["Ht"] = 3.0 });
var circle = new TaggedShape("circle", new() { ["Radius"] = 2.0 });

var rectArea = area.Invoke(rect); // => 6
var circleArea = area.Invoke(circle); // => ~12.57
```

## Type based dispatching

Dispatching on class type, same as inheritance polymorphism. Multi-method will choose the implementation based on runtime type of argument.

```csharp
var area = DefMulti(
	contract: (InheritanceShape shape) => default(double),
	dispatch: DispatchByType<InheritanceShape>());

area.DefMethod((InheritanceRect rect) => rect.Wd * rect.Ht);
area.DefMethod((InheritanceCircle circle) => Math.PI * (circle.Radius * circle.Radius));

var rect = new InheritanceRect { Wd = 2, Ht = 3 };
var circle = new InheritanceCircle { Radius = 2 };

var rectArea = area.Invoke(rect); // => 6
var circleArea = area.Invoke(circle); // => ~12.57
```

## Type based without a base type (dynamic)

```csharp
public record Constant(int Value);
public record BinaryPlus(Constant Left, Constant Right);
public record UnaryInc(Constant Val);
```

```csharp
var eval = DefMulti(
	contract: (dynamic exp) => default(Constant),
	dispatch: DispatchByType<dynamic>());

var stringify = DefMulti(
	contract: (dynamic exp) => default(string),
	dispatch: DispatchByType<dynamic>());

eval.DefMethod((Constant constant) => constant);
eval.DefMethod((BinaryPlus plus) => new Constant(
	eval.Invoke(plus.Left).Value + 
	eval.Invoke(plus.Right).Value));
eval.DefMethod((UnaryInc inc) => eval.Invoke(new BinaryPlus(inc.Val, new Constant(1))));

stringify.DefMethod((Constant constant) => $"{constant.Value}");
stringify.DefMethod((BinaryPlus plus) => $"{stringify.Invoke(plus.Left)} + {stringify.Invoke(plus.Right)}");
stringify.DefMethod((UnaryInc inc) => $"{stringify.Invoke(inc.Val)}++");

var addition = new BinaryPlus(new Constant(2), new Constant(3));
var increment = new UnaryInc(new Constant(6));

var addResult = eval.Invoke(addition); // => 5
var incResult = eval.Invoke(increment); // => 7
var strAddition = stringify.Invoke(addition); // => "2 + 3"
var strIncrement = stringify.Invoke(increment); // => "6++"
```

The previous example is also an example of solving the expression problem with multimethod.

## Property based dispatching

Sometimes you have an object representing a polymorphic value, which can have only one property defined of multiple possible properties, see the example bellow. In this case it is useful to dispatch on presence of value for any property.

```csharp
public record ObservationValue(
    int? Integer = null,
    string? String = null,
    bool? Boolean = null,
    DateTime? DateTime = null,
    Period? Period = null);
```

```json
{"Integer": 25},
{"String": "John"},
{"Boolean": true},
```

```csharp
var stringifyObs = DefMulti(
	contract: (ObservationValue obsVal) => default(string),
	dispatch: DispatchByProp<ObservationValue>());
stringifyObs.DefMethod(nameof(ObservationValue.Integer), (intVal) => $"{intVal.Integer}");
stringifyObs.DefMethod(nameof(ObservationValue.String), (strVal) => $"{strVal.String}");
stringifyObs.DefMethod(nameof(ObservationValue.Boolean), (boolVal) => $"{(boolVal.Boolean == true ? "YES" : "NO")}");
stringifyObs.DefMethod(nameof(ObservationValue.DateTime), (dateVal) => $"{dateVal.DateTime?.ToString("d")} ????");
stringifyObs.DefMethod(nameof(ObservationValue.Period), (periodVal) => $"[{periodVal.Period?.Start?.ToString("d")} - {periodVal.Period?.End?.ToString("d")}]");

var strHt = stringifyObs.Invoke(new ( Integer: 172 )); // => "172"
var strSmokes = stringifyObs.Invoke(new ( Boolean: false )); // => "NO"
var strCoB = stringifyObs.Invoke(new ( String: "John" )); // => "John"
var strDoB = stringifyObs.Invoke(new ( DateTime: new DateTime(1988, 4, 18) )); // => "4/18/1988 ????"
var strValidity = stringifyObs.Invoke(new (
	Period: new ( 
		Start: new DateTime(2022, 2, 14),
		End: new DateTime(2023, 2, 14)))); // => "[2/14/2022 - 2/14/2023]"
```

## Multiple dispatching

Can be achieved by using tuples or any compound type you prefer, see the example below.

```csharp
var eats = DefMulti(
	contract: ((Animal animal, Food food) pair) => default(bool),
	dispatch: (pair) => (pair.animal.Kind, pair.food.Type));

eats.DefMethod(("lion", FoodType.Meat), impl: (_) => true);
eats.DefMethod(("cow", FoodType.Vegetable), impl: (_) => true);
eats.DefMethod(("dog", FoodType.Meat), impl: (_) => true);
eats.DefMethod(("dog", FoodType.Eggs), impl: (_) => true);
eats.DefDefault((_,_) => false);

var lionEatsVegetable = eats.Invoke((
	animal: new Animal(Kind: "lion"), 
	food: new Food(Type: FoodType.Vegetable))); // => false
var lionEatsMeat = eats.Invoke((
	animal: new Animal(Kind: "lion"), 
	food: new Food(Type: FoodType.Meat))); // => true
var dogEatsEggs = eats.Invoke((
	animal: new Animal(Kind: "dog"), 
	food: new Food(Type: FoodType.Eggs))); // => true
var cowEatsVegetable = eats.Invoke((
	animal: new Animal(Kind: "cow"), 
	food: new Food(Type: FoodType.Vegetable))); // => true
```