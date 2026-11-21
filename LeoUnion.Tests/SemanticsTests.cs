using System.Runtime.CompilerServices;
using LeoUnion.Markers;

namespace LeoUnion.Tests;

/// <summary>Edge-case semantics of <see cref="Union{T1,T2}" /> and the marker types.</summary>
public sealed class SemanticsTests
{
    [Fact]
    public void Default_HoldsNoCase()
    {
        Union<int, string> union = default;

        union.Value.Should().BeNull();
        union.ToString().Should().BeEmpty();
        var match = () => union switch { int => 1, string => 2 };
        match.Should().Throw<SwitchExpressionException>();
    }

    [Fact]
    public void NullValueOfNullableCase_MatchesNullPattern()
    {
        Union<string?, NotFound> union = (string?) null;

        union.Value.Should().BeNull();
        string described = union switch { string => "string", NotFound => "not found", null => "null" };
        described.Should().Be("null");
    }

    [Fact]
    public void OverlappingCases_HeldValueMatchesEveryCaseTypeItIs()
    {
        Union<object, string> stringViaObject = (object) "text";
        Union<object, string> plainObject = new object();

        (stringViaObject is string).Should().BeTrue();
        (plainObject is string).Should().BeFalse();
    }

    [Fact]
    public void BaseCaseHoldingDerivedInstance_MatchesDerivedCase()
    {
        Union<Animal, Dog> union = (Animal) new Dog();

        (union is Dog).Should().BeTrue();
    }

    [Fact]
    public void ValueTypeCases_AreMatchedWithoutConfusion()
    {
        Union<int, long> fromInt = 1;
        Union<int, long> fromLong = 1L;

        (fromInt is long).Should().BeFalse();
        (fromLong is int).Should().BeFalse();
        fromInt.Equals(fromLong).Should().BeFalse();
    }

    [Fact]
    public void Markers_AreValueEqual()
    {
        new Success().Should().Be(new Success());
        new NotFound().Should().Be(new NotFound());
        new Conflict().Should().Be(new Conflict());
        new Error("disk full").Should().Be(new Error("disk full"));
        new Error("disk full").Should().NotBe(new Error("timeout"));
        new Error("disk full").Message.Should().Be("disk full");
    }

    [Fact]
    public void Markers_InAUnion()
    {
        Describe(new Success()).Should().Be("success");
        Describe(new NotFound()).Should().Be("not found");
        Describe(new Conflict()).Should().Be("conflict");
        Describe(new Error("disk full")).Should().Be("error: disk full");

        static string Describe(Union<Success, NotFound, Conflict, Error> outcome) => outcome switch
        {
            Success => "success",
            NotFound => "not found",
            Conflict => "conflict",
            Error error => $"error: {error.Message}"
        };
    }

    [Fact]
    public void Markers_ToString_NamesTheOutcome()
    {
        Union<Success, Error> success = new Success();
        Union<Success, Error> error = new Error("disk full");

        success.ToString().Should().Be("Success { }");
        error.ToString().Should().Be("Error { Message = disk full }");
    }

    private class Animal;

    private sealed class Dog : Animal;
}
