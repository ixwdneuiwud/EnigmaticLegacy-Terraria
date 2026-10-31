// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content;

[Flags]
public enum ImportedDamageTraits
{
    None = 0, BypassesArmor = 1, BypassesResistance = 2, Magic = 4,
    Fire = 8, Wither = 16, Void = 32, Explosion = 64
}

// Future ports may subclass this class; traits affect native hurt/strike modifiers.
// These are damage source categories, independent of weapon attack animations.
public abstract class ImportedDamageClass : DamageClass
{
    public abstract ImportedDamageTraits Traits { get; }
    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) =>
        damageClass == Generic || damageClass == Magic && Traits.HasFlag(ImportedDamageTraits.Magic) ? StatInheritanceData.Full : StatInheritanceData.None;
    public override bool GetEffectInheritance(DamageClass damageClass) => damageClass == Magic && Traits.HasFlag(ImportedDamageTraits.Magic);
}
public sealed class DarknessDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.BypassesArmor | ImportedDamageTraits.BypassesResistance;
}
public sealed class NemesisDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.BypassesArmor;
}
public sealed class WitherDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.BypassesArmor | ImportedDamageTraits.Wither;
}
public sealed class VoidDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.BypassesArmor | ImportedDamageTraits.BypassesResistance | ImportedDamageTraits.Void;
}
public sealed class IndirectMagicDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.BypassesArmor | ImportedDamageTraits.Magic;
}
public sealed class ExplosionDamage : ImportedDamageClass
{
    public override ImportedDamageTraits Traits => ImportedDamageTraits.Explosion;
}
