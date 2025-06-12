#nullable enable
namespace Infrastructure.Services.Mediator;

/// <summary>
/// Represents a void type, since Void is not a valid return type in C#.
/// </summary>
public readonly struct Nothing : IEquatable<Nothing>, IComparable<Nothing>
{
    /// <summary>
    /// Default and only value of the <see cref="Nothing"/> type.
    /// </summary>
    public static readonly Nothing Value = new ();

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>Always returns 0 as all units are equal.</returns>
    public int CompareTo(Nothing other) => 0;

    /// <summary>
    /// Determines whether the specified <see cref="Nothing"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Nothing"/> to compare with this instance.</param>
    /// <returns>Always returns <c>true</c> as all units are equal.</returns>
    public bool Equals(Nothing other) => true;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified <see cref="object"/> is a <see cref="Nothing"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Nothing;

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
    public static bool operator ==(Nothing first, Nothing second) => true;

    /// <summary>
    /// Inequality operator.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns>Always returns <c>false</c> as all units are equal.</returns>
    public static bool operator !=(Nothing first, Nothing second) => false;
}