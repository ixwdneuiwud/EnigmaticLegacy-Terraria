// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Systems;

public static class LegacyLiquids
{
    // Server changes liquids; the ordinary Terraria liquid packets synchronize clients.
    public static bool AbsorbTouchingWater(Player player)
    {
        for (int x = (int)player.position.X / 16; x <= (int)(player.position.X + player.width - 1) / 16; x++)
            for (int y = (int)player.position.Y / 16; y <= (int)(player.position.Y + player.height - 1) / 16; y++)
                if (Water(x, y)) { AbsorbConnected(new Point(x, y), 4); return true; }
        return false;
    }
    private static bool Water(int x, int y) => WorldGen.InWorld(x, y, 1) && Main.tile[x, y].LiquidAmount > 0 && Main.tile[x, y].LiquidType == LiquidID.Water;
    internal static int AbsorbConnected(Point start, int radius)
    {
        var seen = new HashSet<Point>();
        var queue = new Queue<(Point Pos, int Depth)>();
        queue.Enqueue((start, 0)); int removed = 0;
        while (queue.Count > 0)
        {
            var (pos, depth) = queue.Dequeue();
            if (!seen.Add(pos) || !Water(pos.X, pos.Y)) continue;
            Main.tile[pos.X, pos.Y].LiquidAmount = 0; removed++;
            Liquid.AddWater(pos.X, pos.Y);
            if (Main.netMode == NetmodeID.Server) NetMessage.sendWater(pos.X, pos.Y);
            if (depth >= radius) continue;
            queue.Enqueue((new Point(pos.X + 1, pos.Y), depth + 1));
            queue.Enqueue((new Point(pos.X - 1, pos.Y), depth + 1));
            queue.Enqueue((new Point(pos.X, pos.Y + 1), depth + 1));
            queue.Enqueue((new Point(pos.X, pos.Y - 1), depth + 1));
        }
        return removed;
    }
}
