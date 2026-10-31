// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyLavaView : ModSystem
{
    public static float Opacity(int waterStyle, float opacity) => !Main.gameMenu && waterStyle == 1 &&
        Main.LocalPlayer.lavaWet && Main.LocalPlayer.GetModPlayer<LegacyPlayer>().Has("blazing_core") ? opacity * .25f : opacity;
    public override void Load() { if (!Main.dedServ) On_Main.DrawLiquid += Draw; }
    private static void Draw(On_Main.orig_DrawLiquid orig, Main main, bool background, int style, float alpha, bool shiny)
        => orig(main, background, style, Opacity(style, alpha), shiny);
}
