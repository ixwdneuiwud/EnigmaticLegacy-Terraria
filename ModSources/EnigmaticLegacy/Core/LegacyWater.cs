// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Core;

public static class LegacyWater
{
    public static bool EyesSubmerged(Player player)
    {
        if (!player.wet || player.lavaWet || player.honeyWet || player.shimmerWet) return false;
        Vector2 eyes = new(player.Center.X, player.gravDir > 0 ? player.Top.Y + 4 : player.Bottom.Y - 4);
        Point at = eyes.ToTileCoordinates();
        if (!WorldGen.InWorld(at.X, at.Y, 1)) return false;
        Tile tile = Main.tile[at.X, at.Y];
        return tile.LiquidType == LiquidID.Water && tile.LiquidAmount > 0 &&
            eyes.Y >= at.Y * 16 + 16 - tile.LiquidAmount / 255f * 16;
    }
}
