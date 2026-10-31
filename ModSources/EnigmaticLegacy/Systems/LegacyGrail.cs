// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public static class LegacyGrail
{
    public static void Drink(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (legacy.AteForbiddenFruit && legacy.Cursed)
        {
            LegacyStatus.Apply(player, ModContent.BuffType<RegenerationIII>(), 1500);
            LegacyStatus.Apply(player, ModContent.BuffType<AbsorptionII>(), 2400);
            LegacyStatus.Apply(player, ModContent.BuffType<ResistanceII>(), 3600);
            LegacyStatus.Apply(player, ModContent.BuffType<LegacyStrength>(), 3000);
            LegacyStatus.Apply(player, ModContent.BuffType<SwiftnessI>(), 3600);
        }
        else
        {
            LegacyStatus.Apply(player, ModContent.BuffType<Withering>(), 300);
            LegacyStatus.Apply(player, ModContent.BuffType<StrongPoison>(), 480);
            LegacyStatus.Apply(player, BuffID.Confused, 720);
            LegacyStatus.Apply(player, ModContent.BuffType<WeaknessII>(), 600);
            // Only this item's formerly unsupported hunger effect receives the
            // requested Terraria substitute. The global hunger system stays absent.
            if (!legacy.AteForbiddenFruit) LegacyStatus.Apply(player, BuffID.Weak, 480);
            LegacyStatus.Apply(player, ModContent.BuffType<SlownessI>(), 720);
        }
    }
    public static void EatForbiddenFruit(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (legacy.AteForbiddenFruit) return;
        legacy.AteForbiddenFruit = true;
        LegacyStatus.Apply(player, ModContent.BuffType<WitheringIV>(), 900);
        LegacyStatus.Apply(player, BuffID.Confused, 900);
        LegacyStatus.Apply(player, ModContent.BuffType<WeaknessIV>(), 1200);
        LegacyStatus.Apply(player, ModContent.BuffType<SlownessIII>(), 1500);
    }
}
