// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public enum LegacyEffect { Regeneration, Strength, Weakness, Haste, Swiftness, Leaping, Slowness, Resistance, Absorption, FireResistance, SlowFalling }

// Ranks stay in native buff slots so native immunity, expiration and network sync apply.
// A weaker rank keeps counting down, but only the strongest rank in a family acts.
public static class LegacyStatus
{
    public static int Level(Player player, LegacyEffect effect) => Level(player.buffType, player.buffTime, effect);
    public static int Level(NPC npc, LegacyEffect effect) => Level(npc.buffType, npc.buffTime, effect);
    private static int Level(int[] types, int[] times, LegacyEffect effect)
    {
        int level = 0;
        for (int i = 0; i < types.Length; i++)
            if (times[i] > 0 && BuffLoader.GetBuff(types[i]) is LeveledLegacyBuff buff && buff.Effect == effect)
                level = Math.Max(level, buff.Level);
        // Turtle Master has two independent vanilla effects at fixed ranks.
        for (int i = 0; i < types.Length; i++)
            if (times[i] > 0 && types[i] == ModContent.BuffType<LegacyTurtleMaster>())
                level = Math.Max(level, effect == LegacyEffect.Slowness ? 6 : effect == LegacyEffect.Resistance ? 4 : 0);
        return level;
    }
    public static void Apply(Player player, int buff, int ticks)
    {
        if (buff <= 0 || ticks <= 0) return;
        player.AddBuff(buff, ticks);
        player.GetModPlayer<LegacyPlayer>().UpdateAbsorption();
    }
    public static void PlayerStats(Player player)
    {
        int strength = Level(player, LegacyEffect.Strength), weakness = Level(player, LegacyEffect.Weakness);
        player.GetDamage(DamageClass.Melee).Base += (strength > 0 ? BridgeUnits.Health(3 * strength) : 0) - (weakness > 0 ? BridgeUnits.Health(4 * weakness) : 0);
        int haste = Level(player, LegacyEffect.Haste);
        player.pickSpeed /= 1 + 0.2f * haste;
        player.GetAttackSpeed(DamageClass.Generic) += 0.1f * haste;
        player.moveSpeed += 0.2f * Level(player, LegacyEffect.Swiftness);
        player.jumpSpeedBoost += 1.5f * Level(player, LegacyEffect.Leaping);
        if (Level(player, LegacyEffect.SlowFalling) > 0) { player.slowFall = true; player.noFallDmg = true; }
    }
    public static float MovementFactor(NPC npc) => (1 + 0.2f * Level(npc, LegacyEffect.Swiftness)) * Math.Max(0.1f, 1 - 0.15f * Level(npc, LegacyEffect.Slowness));
    public static float Resistance(Player player) => Math.Max(0, 1 - 0.2f * Level(player, LegacyEffect.Resistance));
    public static float Resistance(NPC npc) => Math.Max(0, 1 - 0.2f * Level(npc, LegacyEffect.Resistance));
    public static bool FireProtected(NPC npc) => npc.HasBuff(ModContent.BuffType<MoltenHeart>()) || Level(npc, LegacyEffect.FireResistance) > 0;
    public static bool FireProtected(Player player) => player.HasBuff(ModContent.BuffType<MoltenHeart>()) || Level(player, LegacyEffect.FireResistance) > 0;
    public static int RegenerationInterval(int level) => Math.Max(1, 50 >> (level - 1)) * 3;
    public static void Regenerate(Player player, LeveledLegacyBuff buff, int ticks)
    {
        if (player.dead || player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server ||
            Level(player, LegacyEffect.Regeneration) != buff.Level || ticks % RegenerationInterval(buff.Level) != 0) return;
        // Buffs update before equipment. Defer healing until the current tick's
        // pearl/curse modifiers have been gathered, instead of reading stale equips.
        player.GetModPlayer<LegacyPlayer>().QueueStatusRegeneration(BridgeUnits.Health(1));
    }
    public static void Regenerate(NPC npc, LeveledLegacyBuff buff, int ticks)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || npc.immortal || npc.life <= 0 || npc.life >= npc.lifeMax ||
            Compatibility.HasNpcTag(npc, "undead") || Level(npc, LegacyEffect.Regeneration) != buff.Level || ticks % RegenerationInterval(buff.Level) != 0) return;
        int healed = Math.Min(BridgeUnits.Health(1), npc.lifeMax - npc.life);
        npc.life += healed; npc.HealEffect(healed); npc.netUpdate = true;
    }
    public static void ProtectFire(Player player)
    {
        player.lavaImmune = true; player.fireWalk = true;
        player.buffImmune[BuffID.OnFire] = player.buffImmune[BuffID.OnFire3] = player.buffImmune[BuffID.Burning] = true;
    }
}
