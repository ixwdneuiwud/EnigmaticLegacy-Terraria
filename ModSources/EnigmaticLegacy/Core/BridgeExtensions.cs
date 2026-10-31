// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Players;
using Terraria;
using EnigmaticLegacy.Content;

namespace EnigmaticLegacy.Core;

// Ports share the existing loot/mining roll, equipment slots and eligibility.
// A blessing grants access without enabling the seven curses themselves.
public static class BridgeExtensions
{
    public static readonly List<Func<Player, float>> LootProviders = new();
    public static readonly List<Func<Player, float>> FortuneProviders = new();
    public static readonly List<Func<Player, string, bool>> AccessProviders = new();
    public static readonly List<Func<Player, string, string, bool>> ItemAccessProviders = new();
    public static readonly List<Func<Player, bool>> ShieldProviders = new();
    public static readonly List<Func<Player, int>> CurseProviders = new();
    public static readonly List<Func<Player,NPC,ImportedDamageTraits>> AttackTraitProviders=new();
    public static ImportedDamageTraits AttackTraits(Player player,NPC target)
    { var result=ImportedDamageTraits.None;if(player!=null)foreach(var provider in AttackTraitProviders)result|=provider(player,target);return result; }
    public static int AdditionalCurses(Player player) { int count = 0; foreach (var provider in CurseProviders) count += Math.Max(0, provider(player)); return count; }
    public static bool ExtraShield(Player player) { foreach (var provider in ShieldProviders) if (provider(player)) return true; return false; }
    private static readonly Dictionary<int, string> Categories = new();
    private static readonly Dictionary<string, int> ExternalItems = new(StringComparer.Ordinal);
    public static void RegisterExternalItem(string id, int type) => ExternalItems[id] = type;
    public static bool TryExternalItem(string id, out int type) => ExternalItems.TryGetValue(id, out type);
    public static void RegisterCategory(int type, string category) => Categories[type] = category;
    public static string Category(Item item) => item.ModItem is ImportedItem imported
        ? imported.Definition.Kind == "spellstone" ? "spellstone" : imported.Definition.Accessory ? imported.Definition.Category : ""
        : Categories.GetValueOrDefault(item.type, "");
    public static bool HasAccess(Player player, string gate, string id = "")
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (gate == "eldritch" ? legacy.Worthy : legacy.Cursed) return true;
        foreach (var provider in AccessProviders) if (provider(player, gate)) return true;
        foreach (var provider in ItemAccessProviders) if (provider(player, gate, id)) return true;
        return false;
    }
    public static float Sum(List<Func<Player, float>> providers, Player player)
    {
        float value = 0;
        foreach (var provider in providers) value += Math.Max(0, provider(player));
        return value;
    }
    public static void Clear() { LootProviders.Clear(); FortuneProviders.Clear(); AccessProviders.Clear(); ItemAccessProviders.Clear(); ShieldProviders.Clear(); CurseProviders.Clear(); AttackTraitProviders.Clear(); Categories.Clear(); ExternalItems.Clear(); }
}
