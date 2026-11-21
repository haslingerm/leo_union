using System.Diagnostics;
using LeoUnion.Markers;
using ServiceNotFound = LeoUnion.Markers.NotFound;

namespace LeoUnion.Tests;

/// <summary>Keeps the claims and examples in Readme.md honest.</summary>
public sealed class ReadmeExamplesTests
{
    private readonly Dictionary<int, Rocket> _rockets = new() { [1] = new Rocket("Falcon") };

    [Fact]
    public void UsageExample_CompilesAndMatches()
    {
        Describe(1).Should().Be("Found Falcon");
        Describe(3).Should().Be("No such rocket");

        string Describe(int id) => GetRocket(id) switch
        {
            Rocket rocket => $"Found {rocket.Name}",
            NotFound => "No such rocket"
        };
    }

    [Fact]
    public void AliasExample_Compiles()
    {
        Union<Rocket, ServiceNotFound> result = new ServiceNotFound();

        (result is ServiceNotFound).Should().BeTrue();
    }

    [Fact]
    public void SwitchStatementWithGuard_Compiles()
    {
        NameOf(1).Should().Be("Falcon");
        NameOf(3).Should().Be("No such rocket");

        string NameOf(int id)
        {
            switch (GetRocket(id))
            {
                case Rocket rocket:
                    return rocket.Name;
                case NotFound:
                    return "No such rocket";
                default:
                    throw new UnreachableException();
            }
        }
    }

    [Fact]
    public void Equality_ComparesHeldValues()
    {
        Union<int, string> one = 1;
        Union<int, string> otherOne = 1;
        Union<int, string> two = 2;
        Union<int, string> text = "1";

        one.Equals(otherOne).Should().BeTrue();
        one.GetHashCode().Should().Be(otherOne.GetHashCode());
        one.Equals(two).Should().BeFalse();
        one.Equals(text).Should().BeFalse();
    }

    private Union<Rocket, NotFound> GetRocket(int id) =>
        _rockets.TryGetValue(id, out var rocket) ? rocket : new NotFound();

    private sealed record Rocket(string Name);
}
