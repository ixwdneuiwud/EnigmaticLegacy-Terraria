// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Core;

/// <summary>Per-instance relic state. Prefixes and other mods' GlobalItem data are never replaced.</summary>
public sealed class ItemMetadata : GlobalItem
{
    public override bool InstancePerEntity => true;
    public HashSet<string> UniqueKills = new();
    public HashSet<string> VoidFilters = new();
    public byte VoidMode;
    public string BoundOwner = "", BoundName = "";
    public string WeaponIdentity = "";
    public static string ItemKey(Item item) => item.ModItem?.FullName ?? "Terraria/" + item.netID;
    public override GlobalItem Clone(Item from, Item to)
    {
        var clone = (ItemMetadata)base.Clone(from, to);
        clone.UniqueKills = new(UniqueKills); clone.VoidFilters = new(VoidFilters);
        return clone;
    }
    public override void SaveData(Item item, TagCompound tag)
    {
        if (UniqueKills.Count > 0) tag["kills"] = UniqueKills.ToList();
        if (VoidFilters.Count > 0) tag["filters"] = VoidFilters.ToList();
        if (VoidMode > 0) tag["voidMode"] = VoidMode;
        if (BoundOwner != "") { tag["amuletOwner"] = BoundOwner; tag["amuletName"] = BoundName; }
        if (WeaponIdentity != "") tag["weaponIdentity"] = WeaponIdentity;
    }
    public override void LoadData(Item item, TagCompound tag)
    {
        UniqueKills = tag.GetList<string>("kills").Take(100).ToHashSet();
        VoidFilters = tag.GetList<string>("filters").Take(100).ToHashSet();
        VoidMode = (byte)System.Math.Clamp((int)tag.GetByte("voidMode"), 0, 2);
        BoundOwner = tag.GetString("amuletOwner"); BoundName = tag.GetString("amuletName");
        WeaponIdentity = tag.GetString("weaponIdentity");
    }
    public override void NetSend(Item item, BinaryWriter writer)
    {
        writer.Write((byte)UniqueKills.Count); foreach (string key in UniqueKills) writer.Write(key);
        writer.Write((byte)VoidFilters.Count); foreach (string key in VoidFilters) writer.Write(key);
        writer.Write(VoidMode);
        writer.Write(BoundOwner); writer.Write(BoundName);
        writer.Write(WeaponIdentity);
    }
    public override void NetReceive(Item item, BinaryReader reader)
    {
        UniqueKills.Clear(); int count = reader.ReadByte(); for (int i = 0; i < count; i++) { string key = reader.ReadString(); if (i < 100) UniqueKills.Add(key); }
        VoidFilters.Clear(); count = reader.ReadByte(); for (int i = 0; i < count; i++) { string key = reader.ReadString(); if (i < 100) VoidFilters.Add(key); }
        VoidMode = (byte)System.Math.Clamp((int)reader.ReadByte(), 0, 2);
        BoundOwner = reader.ReadString(); BoundName = reader.ReadString();
        WeaponIdentity = reader.ReadString();
    }
    public override bool OnPickup(Item item, Player player)
    {
        Systems.LegacyAmulets.Bind(item, player);
        return true;
    }
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        // The original owner row is rendered from source-tooltips.json.
    }
}
