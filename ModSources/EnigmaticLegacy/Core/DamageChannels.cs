// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using EnigmaticLegacy.Content;

namespace EnigmaticLegacy.Core;

// Attributes and event multipliers are different units. A channel is identified
// by its source attribute/event, never by a translated tooltip's wording.
public enum DamageOperation { Addition, MultiplyBase, MultiplyTotal }

public static class DamageArithmetic
{
    public static StatModifier Attribute(StatModifier value, DamageOperation operation, float amount)
    {
        if (!float.IsFinite(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
        switch (operation)
        {
            case DamageOperation.Addition: value.Base += amount; break;
            case DamageOperation.MultiplyBase: value += amount; break;
            case DamageOperation.MultiplyTotal: value *= 1 + amount; break;
        }
        return value;
    }
    // Explicit expressions preserve (fire * melee) and (1 + fireBonus + meleeBonus)
    // as different formulae. No element receives an implicit stacking policy.
    public static float Product(params float[] factors) { float result = 1; foreach (float f in factors) result *= f; return result; }
    public static float AddBonuses(params float[] factors) => 1 + factors.Sum(f => f - 1);
    public static StatModifier Scale(StatModifier value, float factor)
    { value *= factor; value.Flat *= factor; return value; }
}

public readonly record struct DamageHitContext(Player Player, NPC Target, Item Item, Projectile Projectile)
{
    public DamageClass Profession => Projectile?.DamageType ?? Item?.DamageType ?? DamageClass.Generic;
    public bool CountsAs(DamageClass profession) => Profession.CountsAsClass(profession);
    public bool HasTag(string tag) => DamageChannels.HasTag(this, tag);
}
public delegate void DamageHitRule(in DamageHitContext context, ref NPC.HitModifiers modifiers);

public static class DamageChannels
{
    private static readonly Dictionary<string, (DamageHitRule Rule, int Priority, long Sequence)> rules = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Func<DamageHitContext, bool>> tags = new(StringComparer.Ordinal);
    private static long nextSequence;
    public static bool RegisterRule(string source, DamageHitRule rule, int priority = 0)
    { if (string.IsNullOrWhiteSpace(source) || !source.Contains(':') || rule == null) throw new ArgumentException("A namespaced source and rule are required."); rules.Add(source, (rule, priority, nextSequence++)); return true; }
    public static bool UnregisterRule(string source) => rules.Remove(source);
    public static bool RegisterTag(string tag, Func<DamageHitContext, bool> classifier)
    { if (string.IsNullOrWhiteSpace(tag) || classifier == null) throw new ArgumentException("A tag and classifier are required."); tags.Add(tag, classifier); return true; }
    public static bool UnregisterTag(string tag) => tags.Remove(tag);
    public static bool HasTag(in DamageHitContext context, string tag)
    {
        if (context.Projectile != null && Compatibility.HasProjectileTag(context.Projectile, tag) || tags.TryGetValue(tag, out var classifier) && classifier(context)) return true;
        if (context.Profession is not ImportedDamageClass imported) return false;
        return tag switch {
            "fire" => imported.Traits.HasFlag(ImportedDamageTraits.Fire),
            "void" => imported.Traits.HasFlag(ImportedDamageTraits.Void),
            "wither" => imported.Traits.HasFlag(ImportedDamageTraits.Wither),
            "explosion" => imported.Traits.HasFlag(ImportedDamageTraits.Explosion),
            "magic" => imported.Traits.HasFlag(ImportedDamageTraits.Magic),
            _ => false
        };
    }
    internal static void Apply(in DamageHitContext context, ref NPC.HitModifiers modifiers)
    { foreach (var rule in rules.Values.OrderBy(r => r.Priority).ThenBy(r => r.Sequence)) rule.Rule(context, ref modifiers); }
    internal static void Clear() { rules.Clear(); tags.Clear(); nextSequence = 0; }
}

// Source-specific universal attributes remain separate from native Generic and
// from one another. Two changes to the SAME attribute share its original ops.
public sealed class DamageChannelPlayer : ModPlayer
{
    private readonly Dictionary<string, StatModifier> universal = new(StringComparer.Ordinal);
    public override void ResetEffects() => universal.Clear();
    public void AddUniversal(string source, DamageOperation operation, float amount)
    { if (string.IsNullOrWhiteSpace(source) || !source.Contains(':')) throw new ArgumentException("A namespaced source is required."); universal[source] = DamageArithmetic.Attribute(universal.GetValueOrDefault(source, StatModifier.Default), operation, amount); }
    public float UniversalFactor => Math.Max(0, universal.Values.Aggregate(1f, (f, m) => f * m.ApplyTo(1)));
    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) => DamageChannels.Apply(new(Player,target,item,null),ref modifiers);
    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) => DamageChannels.Apply(new(Player,target,null,proj),ref modifiers);
}

public sealed class DamageStatBridge : ModSystem
{
    public override void Load() => On_Player.GetTotalDamage += TotalDamage;
    private static StatModifier TotalDamage(On_Player.orig_GetTotalDamage orig, Player player, DamageClass type)
    {
        // Keep every native/mod hook in the chain. Isolate the native all-class
        // additive bucket from the profession bucket, after inheritance resolves.
        StatModifier result = orig(player,type);
        float weight = type == DamageClass.Generic ? 1 : type.GetModifierInheritance(DamageClass.Generic).damageInheritance;
        float allBonus = (player.GetDamage(DamageClass.Generic).Additive - 1) * weight;
        result -= allBonus;
        return DamageArithmetic.Scale(result, Math.Max(0,1+allBonus) * player.GetModPlayer<DamageChannelPlayer>().UniversalFactor);
    }
    public override void Unload() => DamageChannels.Clear();
}
