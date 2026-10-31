// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public static class LegacyPotions
{
    public static (int Buff, int Ticks) Effect(ImportedDefinition definition)
    {
        bool ultimate = definition.Id.StartsWith("ultimate_", StringComparison.Ordinal);
        int ticks = ultimate || definition.Variant.StartsWith("long_", StringComparison.Ordinal) ? 28800 : definition.Variant.StartsWith("strong_", StringComparison.Ordinal) ? 5400 : 10800;
        string effect = definition.Variant.Replace("long_", "").Replace("strong_", "");
        return effect switch
        {
            "night_vision" => (BuffID.NightOwl, 57600),
            "invisibility" => (BuffID.Invisibility, 57600),
            "leaping" => (ModContent.BuffType<LegacyLeaping>(), ticks),
            "fire_resistance" => (ModContent.BuffType<LegacyFireResistance>(), 57600),
            "swiftness" => (ModContent.BuffType<LegacySwiftness>(), ticks),
            "slowness" => (ModContent.BuffType<LegacySlowness>(), 3600),
            "turtle_master" => (ModContent.BuffType<LegacyTurtleMaster>(), 2400),
            "water_breathing" => (BuffID.Gills, 57600),
            "poison" => (ModContent.BuffType<StrongPoison>(), 5400),
            "regeneration" => (ModContent.BuffType<StrongRegeneration>(), 5400),
            "strength" => (ModContent.BuffType<LegacyStrength>(), ticks),
            "weakness" => (ModContent.BuffType<LegacyWeakness>(), ticks),
            "slow_falling" => (ModContent.BuffType<LegacySlowFalling>(), ticks),
            "haste" => (ultimate || definition.Variant.StartsWith("strong_", StringComparison.Ordinal) ? ModContent.BuffType<StrongHaste>() : ModContent.BuffType<LegacyHaste>(), ticks),
            "molten_heart" => (ModContent.BuffType<MoltenHeart>(), ultimate ? 57600 : ticks),
            _ => (0, 0)
        };
    }
    public static void Apply(Player player, ImportedDefinition definition, float factor = 1)
    {
        if (definition.Variant == "healing") { player.Heal((int)(BridgeUnits.Health(16) * factor)); return; }
        if (definition.Variant == "harming") { LegacyDamage.Hurt(player, LegacyDamage.Type("indirect_magic"), Terraria.DataStructures.PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral(player.name + " was consumed by a potion.")), Math.Max(1, (int)(BridgeUnits.Health(24) * factor))); return; }
        var (buff, ticks) = Effect(definition);
        if (buff <= 0) return;
        LegacyStatus.Apply(player, buff, (int)MathF.Floor(ticks * factor + .5f));
    }
    public static void Apply(NPC npc, Player owner, ImportedDefinition definition, float factor = 1)
    {
        if (!npc.active || npc.life <= 0 || npc.immortal || npc.dontTakeDamage) return;
        if (definition.Variant is "healing" or "harming")
        {
            bool hurt = definition.Variant == "harming" ^ Compatibility.HasNpcTag(npc, "undead");
            int value = (int)(BridgeUnits.Health(definition.Variant == "healing" ? 16 : 24) * factor);
            if (hurt) owner.ApplyDamageToNPC(npc, value, 0, 0, false, LegacyDamage.Type("indirect_magic"));
            else { npc.life = Math.Min(npc.lifeMax, npc.life + value); npc.HealEffect(value); npc.netUpdate = true; }
            return;
        }
        var (buff, ticks) = Effect(definition);
        if (buff > 0) npc.AddBuff(buff, (int)MathF.Floor(ticks * factor + .5f));
    }
}
