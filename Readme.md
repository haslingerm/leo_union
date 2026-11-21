# LeoUnion

Generic ad-hoc union types for C# 15 / .NET 11, used at [HTL Leonding](https://www.htl-leonding.at/).

C# 15 added native unions (`public union Pet(Cat, Dog);`), but no predefined generic union types.
This package provides `Union<T1, ..., T10>` (2 to 10 cases), so a method can return "one of these types" without
declaring a named union first, plus marker types for the most common outcomes.

`Union<...>` is a real C# union: case values convert implicitly, and `switch` expressions over it are checked for
exhaustiveness by the compiler.

> If the same set of cases is used in several places, prefer declaring a named union (`public union GetRocketResult(Rocket, NotFound);`).
> The generic types are meant for ad-hoc cases.

## Installation

> `dotnet add package HTLLeonding.Utility.LeoUnion`

Requires .NET 11 (C# 15).

## Usage

```csharp
using LeoUnion;
using LeoUnion.Markers;

public Union<Rocket, NotFound> GetRocket(int id) =>
    rockets.TryGetValue(id, out var rocket) ? rocket : new NotFound();

string message = GetRocket(3) switch
{
    Rocket rocket => $"Found {rocket.Name}",
    NotFound => "No such rocket"
}; // adding a third case to the union makes this a compile error until it is handled
```

### Marker types

`LeoUnion.Markers` contains small case types for common outcomes:

| Type | Meaning |
|---|---|
| `Success` | The operation succeeded and has no result value |
| `NotFound` | The requested item does not exist |
| `Conflict` | The operation conflicts with the current state (e.g. already in use, already exists) |
| `Error(string Message)` | The operation failed for an expected reason |

They are in their own namespace because `NotFound` and `Conflict` also exist in ASP.NET Core
(`Microsoft.AspNetCore.Http.HttpResults`). Where both are needed, use an alias:

```csharp
using ServiceNotFound = LeoUnion.Markers.NotFound;
```

For domain-specific outcomes (e.g. `NinjaBusy`), declare your own `readonly record struct`.

### Switch statements

The compiler checks switch *expressions* for exhaustiveness, but not switch *statements*, and its flow analysis does
not know that a complete switch statement cannot fall through. Use the
[LeoAnalyzers](https://www.nuget.org/packages/HTLLeonding.Utility.LeoAnalyzers) package (1.3.0 or newer): rule `LA0003`
reports switch statements over union types (including these) that do not handle every case. A `default` whose only
statement is a `throw` is treated as a guard, not as coverage, so value-returning switch statements compile while
missing cases are still reported:

```csharp
switch (GetRocket(id))
{
    case Rocket rocket:
        return rocket.Name;
    case NotFound:
        return "No such rocket";
    default:
        throw new UnreachableException(); // guard; LA0003 still reports missing cases
}
```

## Semantics to be aware of

- **`default` holds no case.** `default(Union<A, B>)` (and an uninitialized field) has a `null` `Value`; a switch
  expression over it throws `SwitchExpressionException`.
- **Boxing.** The union holds its value as `object`, so a value-type case is boxed once when the union is created.
  Pattern matching does not allocate.
- **Nullable cases.** A union holding a `null` value of a nullable case type matches the `null` pattern, not the case type.
- **Overlapping cases.** A held value matches every case type it is an instance of, e.g. `Union<Animal, Dog>` created
  from an `Animal`-typed variable holding a `Dog` matches `Dog`.
- **Equality.** Two unions are equal if their held values are equal.
- **`ToString()`** returns the held value's string representation, or an empty string if there is none.

## Development

The union types and their contract tests are generated:

> `dotnet run tools/Generate.cs`

Change the generator, never the `*.g.cs` files. CI fails if the generated files are out of date.

Run the tests with `dotnet test --solution LeoUnion.slnx`.

Publishing to NuGet happens via the manually triggered `Build & Publish NuGet` workflow, using NuGet trusted publishing.
Bump `<Version>` in `LeoUnion/LeoUnion.csproj` before running it.
