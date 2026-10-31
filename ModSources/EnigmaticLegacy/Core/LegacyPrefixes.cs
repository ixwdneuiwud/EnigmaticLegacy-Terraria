// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public static class LegacyPrefixes
{
    // Vanilla's overall negative modifiers. Mixed modifiers such as Ruthless are
    // not classified merely because they have one unfavorable statistic.
    private static readonly string[] VanillaNegativeNames = {
        "Tiny", "Terrible", "Small", "Dull", "Unhappy", "Shameful", "Awful", "Lethargic", "Awkward",
        "Inept", "Ignorant", "Deranged", "Broken", "Damaged", "Shoddy", "Slow", "Sluggish", "Lazy", "Annoying", "Weak"
    };
    private static readonly HashSet<int> Negative = new();
    public static IReadOnlyCollection<int> NegativeIds => Negative;
    public static void Initialize()
    {
        foreach (string name in VanillaNegativeNames)
            if (PrefixID.Search.TryGetId(name, out int id)) Negative.Add(id);
    }
    public static void Unload() => Negative.Clear();
    public static bool IsNegative(int prefix) => prefix > 0 && Negative.Contains(prefix);
    public static void Register(int prefix, bool negative)
    {
        if (prefix < 1 || prefix >= PrefixLoader.PrefixCount) throw new ArgumentOutOfRangeException(nameof(prefix));
        if (negative) Negative.Add(prefix); else Negative.Remove(prefix);
    }
    public static IEnumerable<Item> ActiveEquipment(Player player)
    {
        var items = new HashSet<Item>();
        for (int i = 0; i < 10; i++)
            if (i < 3 || player.IsItemSlotUnlockedAndUsable(i)) items.Add(player.armor[i]);
        using (new Main.CurrentPlayerOverride(player))
            foreach (var slot in ModContent.GetContent<ModAccessorySlot>())
                if (slot.IsEnabled()) items.Add(slot.FunctionalItem);
        Item held = player.HeldItem;
        if (held.damage > 0 || held.pick > 0 || held.axe > 0 || held.hammer > 0) items.Add(held);
        return items.Where(item => !item.IsAir);
    }
    public static int CurseCount(Player player) => ActiveEquipment(player).Count(item => IsNegative(item.prefix));
    public static int TotalCurses(Player player) => CurseCount(player) + (player.GetModPlayer<global::EnigmaticLegacy.Players.LegacyPlayer>().Cursed ? 7 : 0) + BridgeExtensions.AdditionalCurses(player);
}
