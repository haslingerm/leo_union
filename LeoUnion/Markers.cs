namespace LeoUnion.Markers;

// Small, data-less (or nearly) case types for the common outcomes of an operation, e.g.
//     Union<Rocket, NotFound> GetRocket(int id)
//     Union<Success, NotFound, Conflict> AssignMission(...)
// They live in their own namespace because names like NotFound and Conflict also exist elsewhere
// (e.g. ASP.NET Core's typed results); import them only where needed or alias them.

/// <summary>The operation succeeded and has no result value.</summary>
public readonly record struct Success;

/// <summary>The requested item does not exist.</summary>
public readonly record struct NotFound;

/// <summary>The operation conflicts with the current state, e.g. the item is already in use or already exists.</summary>
public readonly record struct Conflict;

/// <summary>The operation failed for an expected reason, described by <see cref="Message" />.</summary>
/// <param name="Message">A description of the failure. <see langword="null" /> only for <c>default(Error)</c>.</param>
public readonly record struct Error(string Message);
