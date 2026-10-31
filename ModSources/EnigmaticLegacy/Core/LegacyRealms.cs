// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace EnigmaticLegacy.Core;

// Logical Minecraft dimensions within one Terraria world. Height changes take effect immediately.
public static class LegacyRealms
{
    public const string Overworld = "overworld", Nether = "nether";
    public static string At(Vector2 center) => center.Y >= Main.UnderworldLayer * 16f ? Nether : Overworld;
    public static bool Same(Vector2 first, Vector2 second) => At(first) == At(second);
    public static string KeyAt(Vector2 center) => Main.worldID + "|" + At(center);
    public static bool InCurrentWorld(string key) => key == Main.worldID.ToString() || key.StartsWith(Main.worldID + "|", StringComparison.Ordinal);
    public static string StoredRealm(string key, Vector2 oldPosition)
    {
        int separator = key.IndexOf('|');
        // Early port saves stored the player's top-left and only a world ID.
        return separator >= 0 ? key[(separator + 1)..] : At(oldPosition + new Vector2(10, 21));
    }
    public static bool Matches(string key, Vector2 oldPosition, Vector2 center) => InCurrentWorld(key) && StoredRealm(key, oldPosition) == At(center);
    public static string Label(string realm) => realm == Nether ? BridgeUnits.Text("下界（地狱高度区）", "Nether (Underworld layer)") : BridgeUnits.Text("主世界", "Overworld");
    public static string Describe(string key, Vector2 position) => string.IsNullOrEmpty(key) ? BridgeUnits.Text("未绑定", "Unbound") :
        (InCurrentWorld(key) ? "" : BridgeUnits.Text("其他世界 / ", "Another world / ")) + Label(StoredRealm(key, position));
}
