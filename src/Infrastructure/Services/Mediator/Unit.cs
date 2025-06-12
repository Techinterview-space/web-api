namespace Infrastructure.Services.Mediator;

/// <summary>
/// Represents a void type, since Void is not a valid return type in C#.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>
{
    /// <summary>
    /// Default and only value of the <see cref="Unit"/> type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>Always returns 0 as all units are equal.</returns>
    public int CompareTo(Unit other) => 0;

    /// <summary>
    /// Determines whether the specified <see cref="Unit"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Unit"/> to compare with this instance.</param>
    /// <returns>Always returns <c>true</c> as all units are equal.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is a <see cref="Unit"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>Always returns 0.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    public override string ToString() => "()";

    /// <summary>
    /// Equality operator.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns>Always returns <c>true</c> as all units are equal.</returns>
    public static bool operator ==(Unit first, Unit second) => true;

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns>Always returns <c>false</c> as all units are equal.</returns>
    public static bool operator !=(Unit first, Unit second) => false;
}