using System;
using System.Runtime.CompilerServices;
using SFDGameScriptInterface;

namespace SFR.Fighter;

/// <summary>
///     This class is used to extend the Player modifiers and save new data to them.
///     Basically clones the base class and appends it as a modifier extension to them.
/// </summary>
public class ExtendedModifiers : IEquatable<ExtendedModifiers>
{
    internal static readonly ConditionalWeakTable<PlayerModifiers, ExtendedModifiers> ExtendedModifiersTable = new();
    internal readonly PlayerModifiers Modifiers;
    public float BulletDodgeChance;
    public float JumpHeightModifier;

    public ExtendedModifiers(PlayerModifiers modifiers, bool defaultValues = false)
    {
        Modifiers = modifiers;
        int num = defaultValues ? -2 : -1;
        JumpHeightModifier = num;
        BulletDodgeChance = num;
    }

    public bool Equals(ExtendedModifiers other) => other != null && other.Modifiers.Equals(Modifiers) && other.JumpHeightModifier == JumpHeightModifier && other.BulletDodgeChance == BulletDodgeChance;
}