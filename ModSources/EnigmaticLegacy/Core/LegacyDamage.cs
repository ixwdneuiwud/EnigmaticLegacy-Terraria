// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Terraria;
using Terraria.DataStructures;
using System;
using EnigmaticLegacy.Content;
using EnigmaticLegacy.Players;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public static class LegacyDamage
{
    public static ImportedDamageClass Type(string id) => id switch
    {
        "darkness" => ModContent.GetInstance<DarknessDamage>(), "nemesis_curse" => ModContent.GetInstance<NemesisDamage>(),
        "wither" => ModContent.GetInstance<WitherDamage>(), "void" => ModContent.GetInstance<VoidDamage>(),
        "indirect_magic" => ModContent.GetInstance<IndirectMagicDamage>(), "explosion" => ModContent.GetInstance<ExplosionDamage>(),
        _ => throw new ArgumentException("Unknown imported damage type: " + id)
    };
    public static double Hurt(Player target, ImportedDamageClass type, PlayerDeathReason source, int damage, int direction = 0, bool pvp = false)
    {
        var legacy = target.GetModPlayer<LegacyPlayer>();
        ImportedDamageClass previous = legacy.IncomingDamageType;
        try { legacy.IncomingDamageType = type; return target.Hurt(source, damage, direction, pvp: pvp); }
        finally { legacy.IncomingDamageType = previous; }
    }
    // Minecraft distinguishes an attacking entity from falling, drowning and world hazards.
    public static bool IsCreatureSource(PlayerDeathReason source)
    {
        if (source.SourceNPCIndex >= 0 && source.SourceNPCIndex < Main.maxNPCs && Main.npc[source.SourceNPCIndex].active) return true;
        if (source.SourcePlayerIndex >= 0 && source.SourcePlayerIndex < Main.maxPlayers && Main.player[source.SourcePlayerIndex].active) return true;
        int index = source.SourceProjectileLocalIndex;
        return index >= 0 && index < Main.maxProjectiles && Main.projectile[index].active &&
            !Main.projectile[index].trap && !Compatibility.HasProjectileTag(Main.projectile[index], "environment");
    }
}
