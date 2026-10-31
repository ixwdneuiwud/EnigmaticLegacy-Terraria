// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Content.Status;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public static class LegacyRandomEffects
{
    public static (int Buff, int Ticks) Positive()
    {
        int level = Main.rand.NextFloat() <= 0.25f ? 2 : 1;
        int buff = Main.rand.Next(8) switch
        {
            0 => level == 1 ? ModContent.BuffType<AbsorptionI>() : ModContent.BuffType<AbsorptionII>(),
            1 => level == 1 ? ModContent.BuffType<StrengthI>() : ModContent.BuffType<LegacyStrength>(),
            2 => level == 1 ? ModContent.BuffType<RegenerationI>() : ModContent.BuffType<StrongRegeneration>(),
            3 => level == 1 ? ModContent.BuffType<LegacyHaste>() : ModContent.BuffType<StrongHaste>(),
            4 => BuffID.Gills,
            5 => level == 1 ? ModContent.BuffType<SwiftnessI>() : ModContent.BuffType<LegacySwiftness>(),
            6 => level == 1 ? ModContent.BuffType<ResistanceI>() : ModContent.BuffType<ResistanceII>(),
            _ => level == 1 ? ModContent.BuffType<LeapingI>() : ModContent.BuffType<LegacyLeaping>()
        };
        return (buff, Main.rand.Next(100, 600) * 3);
    }
    public static (int Buff, int Ticks) Negative()
    {
        int level = Main.rand.NextFloat() <= 0.15f ? 3 : Main.rand.NextFloat() <= 0.4f ? 2 : 1;
        int buff = Main.rand.Next(9) switch
        {
            0 => BuffID.Blackout,
            1 => BuffID.Confused,
            2 or 3 => 0, // Mining fatigue and hunger: deliberately empty, no substitute.
            4 => BuffID.Darkness,
            5 => level == 1 ? ModContent.BuffType<SlownessI>() : level == 2 ? ModContent.BuffType<SlownessII>() : ModContent.BuffType<SlownessIII>(),
            6 => level == 1 ? ModContent.BuffType<LegacyWeakness>() : level == 2 ? ModContent.BuffType<WeaknessII>() : ModContent.BuffType<WeaknessIII>(),
            7 => BuffID.Poisoned,
            _ => level == 1 ? ModContent.BuffType<LesserWithering>() : ModContent.BuffType<Withering>()
        };
        return (buff, Main.rand.Next(200, 1200) * 3);
    }
}
