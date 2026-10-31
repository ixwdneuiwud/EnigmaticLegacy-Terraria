// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Light;

namespace EnigmaticLegacy.Core;

// Gameplay light must not depend on a particular client's camera or Lighting buffer.
// Use the native tile light scanner and propagation for both single player and servers.
public sealed class LegacyLight
{
    private readonly TileLightScanner scanner = new();
    private readonly LightMap map = new();
    private Rectangle area;

    public void Scan(Vector2 center, int radius)
    {
        Point point = center.ToTileCoordinates();
        int left = System.Math.Max(5, point.X - radius - 16), top = System.Math.Max(5, point.Y - radius - 16);
        int right = System.Math.Min(Main.maxTilesX - 5, point.X + radius + 17), bottom = System.Math.Min(Main.maxTilesY - 5, point.Y + radius + 17);
        area = new Rectangle(left, top, right - left, bottom - top);
        map.SetSize(area.Width, area.Height); map.Clear(); map.NonVisiblePadding = 0;
        map.LightDecayThroughAir = 0.91f; map.LightDecayThroughSolid = 0.56f;
        map.LightDecayThroughWater = new Vector3(0.8f, 0.88f, 0.91f);
        map.LightDecayThroughHoney = new Vector3(0.75f, 0.7f, 0.6f);
        scanner.Update();
        scanner.ExportTo(area, map, new TileLightScannerOptions { DrawInvisibleWalls = false });
        // Dedicated servers do not update the renderer's sunlight color.
        // Supply sky light as a source before applying the same native propagation.
        for (int x = 0; x < area.Width; x++)
            for (int y = 0; y < area.Height && top + y < Main.worldSurface; y++)
            {
                Tile tile = Main.tile[left + x, top + y];
                if (tile.WallType == 0 && (!tile.HasTile || !Main.tileBlockLight[tile.TileType]))
                    map[x, y] = Vector3.Max(map[x, y], new Vector3(Main.dayTime ? 0.8f : 0.08f));
            }
        foreach (var source in Systems.LegacyDynamicLight.Sources())
            if (area.Contains(source.Position))
            {
                int x = source.Position.X - area.X, y = source.Position.Y - area.Y;
                map[x, y] = Vector3.Max(map[x, y], source.Color);
            }
        map.Blur();
    }

    public float Brightness(Vector2 position)
    {
        Point point = position.ToTileCoordinates();
        if (!area.Contains(point)) return 1;
        Vector3 light = map[point.X - area.X, point.Y - area.Y];
        return (light.X + light.Y + light.Z) / 3;
    }
}
