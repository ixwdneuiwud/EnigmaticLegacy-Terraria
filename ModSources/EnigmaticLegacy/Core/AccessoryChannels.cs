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

// Category is a restriction on the slot, not on a universal accessory's effects.
public interface IRelicAccessorySlot { string AccessoryCategory { get; } }

public static class AccessoryChannels
{
    private static readonly Dictionary<string, bool> declared = new(StringComparer.Ordinal);
    private static Item[] probes;
    public static bool Register(string slotFullName, bool universal)
    { if (string.IsNullOrWhiteSpace(slotFullName)) throw new ArgumentException("An accessory slot FullName is required."); declared[slotFullName] = universal; return true; }
    public static bool AcceptsUniversalItem(Item item) => item.IsAir || item.accessory && item.headSlot < 0 && item.bodySlot < 0 && item.legSlot < 0;
    public static int NativeCapacity(Player player) => Enumerable.Range(3, 7).Count(player.IsItemSlotUnlockedAndUsable);
    public static bool IsUniversal(ModAccessorySlot slot)
    {
        if (slot is IRelicAccessorySlot relic) return relic.AccessoryCategory == "curio";
        if (declared.TryGetValue(slot.FullName, out bool universal)) return universal;
        // tML does not expose a universal/type-restricted slot flag. Probe only
        // the slot's acceptance policy, never bypass the item's native hooks.
        probes ??= new[] { ItemID.WarriorEmblem, ItemID.CharmofMyths, ItemID.FairyWings, ItemID.ObsidianShield }.Select(t => new Item(t)).ToArray();
        return probes.All(item => slot.CanAcceptItem(item, AccessorySlotType.FunctionalSlot));
    }
    public static void Clear() { declared.Clear(); probes = null; }
}
