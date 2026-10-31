// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyDynamicLight : ModSystem
{
    private static readonly ConcurrentDictionary<Point, (Vector3 Color, ulong Tick)> emitted = new();
    private static readonly List<Func<IEnumerable<(Vector2 Position, Vector3 Color)>>> providers = new();
    public static bool Register(Func<IEnumerable<(Vector2 Position, Vector3 Color)>> provider)
    { if (provider == null) throw new ArgumentNullException(nameof(provider)); providers.Add(provider); return true; }
    public override void Load() => On_Lighting.AddLight_int_int_float_float_float += Capture;
    private static void Capture(On_Lighting.orig_AddLight_int_int_float_float_float orig, int x, int y, float r, float g, float b)
    {
        var point = new Point(x, y); var color = new Vector3(r, g, b);
        if (float.IsFinite(r) && float.IsFinite(g) && float.IsFinite(b))
            emitted.AddOrUpdate(point, (color, Main.GameUpdateCount), (_, old) =>
                (old.Tick == Main.GameUpdateCount ? Vector3.Max(old.Color, color) : color, Main.GameUpdateCount));
        orig(x, y, r, g, b);
    }
    public static IEnumerable<(Point Position, Vector3 Color)> Sources()
    {
        foreach (var pair in emitted)
            if (Main.GameUpdateCount - pair.Value.Tick <= 2) yield return (pair.Key, pair.Value.Color);
        // The native scalar is also available on dedicated servers, where many
        // visual-only AI branches intentionally skip Lighting.AddLight.
        foreach (Projectile shot in Main.projectile)
            if (shot.active && shot.light > 0) yield return (shot.Center.ToTileCoordinates(), new Vector3(shot.light));
        foreach (var provider in providers)
            foreach (var source in provider()) yield return (source.Position.ToTileCoordinates(), source.Color);
    }
    public override void PostUpdateEverything()
    {
        if (Main.GameUpdateCount % 60 != 0) return;
        foreach (var pair in emitted)
            if (Main.GameUpdateCount - pair.Value.Tick > 2) emitted.TryRemove(pair.Key, out _);
    }
    public override void OnWorldUnload() => emitted.Clear();
    public override void Unload() { emitted.Clear(); providers.Clear(); }
}
