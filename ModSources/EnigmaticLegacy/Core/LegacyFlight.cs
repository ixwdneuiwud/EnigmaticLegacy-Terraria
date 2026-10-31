// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Core;

// Shared by creative flight, flight scrolls and future imported elytras.
// Movement, gravity inversion, mounts and wing controls remain in Player's wing code.
public static class LegacyFlight
{
    public const int AnchorRadius = 50;
    private static readonly HashSet<int> Anchors = new() { TileID.CrystalBall };
    public static bool NearAnchor(Player player)
    {
        Point center = player.Center.ToTileCoordinates();
        for (int x = Math.Max(1, center.X - AnchorRadius); x <= Math.Min(Main.maxTilesX - 2, center.X + AnchorRadius); x++)
            for (int y = Math.Max(1, center.Y - AnchorRadius); y <= Math.Min(Main.maxTilesY - 2, center.Y + AnchorRadius); y++)
                if (Main.tile[x, y].HasTile && Anchors.Contains(Main.tile[x, y].TileType) &&
                    LegacyRealms.Same(player.Center, new Vector2(x * 16 + 8, y * 16 + 8))) return true;
        return false;
    }
    public static void RegisterAnchor(int tileType) => Anchors.Add(tileType);
    public static void Reset() { Anchors.Clear(); Anchors.Add(TileID.CrystalBall); }

    public static void Grant(Player player, Item fallback, bool infinite)
    {
        if (player.mount.Active) return;
        if (player.wingsLogic == 0)
        {
            player.wingsLogic = fallback.wingSlot;
            player.wings = fallback.wingSlot;
            player.equippedWings = fallback;
            player.wingTimeMax = player.GetWingStats(fallback.wingSlot).FlyTime;
        }
        player.noFallDmg = true;
        if (infinite) player.wingTime = Math.Max(player.wingTime, player.wingTimeMax);
    }
}
