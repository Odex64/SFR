using System;
using System.Runtime.CompilerServices;
using SFDGameScriptInterface;

namespace SFR.Fighter;

/// <summary>
///     This class is used to extend the PlayerExt modifiers and save new data to them.
///     Basically clones the base class and appends it as a modifier extension to them.
/// </summary>
internal class ExtendedModifiers(PlayerModifiers modifiers) : IEquatable<ExtendedModifiers>
{
    internal static readonly ConditionalWeakTable<PlayerModifiers, ExtendedModifiers> ExtendedModifiersTable = new();
    internal readonly PlayerModifiers Modifiers = modifiers;
    public float BulletDodgeChance = -1;
    public float JumpHeightModifier = -1;

    public bool Equals(ExtendedModifiers other) => other != null && other.Modifiers.Equals(Modifiers) && other.JumpHeightModifier == JumpHeightModifier && other.BulletDodgeChance == BulletDodgeChance;
}