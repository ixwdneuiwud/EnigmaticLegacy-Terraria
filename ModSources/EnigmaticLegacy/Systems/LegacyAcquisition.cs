// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Systems;

public sealed class AcquisitionChestRule
{
    public string item, pool;
    public int numerator = 1, denominator = 1, minimum = 1, maximum = 1;
}
public sealed class AcquisitionNpcRule
{
    public string item, group, condition;
    public int denominator = 1, minimum = 1, maximum = 1;
}
public sealed class AcquisitionRules
{
    public AcquisitionChestRule[] chest_rules;
    public AcquisitionNpcRule[] npc_rules;
}

public static class LegacyAcquisition
{
    private static AcquisitionRules data;
    public static AcquisitionRules Rules => data ??= JsonSerializer.Deserialize<AcquisitionRules>(
        ImportCatalog.Owner.GetFileBytes("Data/acquisition.json"), new JsonSerializerOptions { IncludeFields = true });
    public static void Reset() => data = null;
    public static bool WearingCurses(Player player) => player != null && player.active && player.GetModPlayer<LegacyPlayer>().Has("cursed_ring");
    public static Player CreditedKiller(NPC npc) => npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers ? Main.player[npc.lastInteraction] : null;
    public static bool Eligible(NPC npc) => LegacyEconomy.Eligible(npc);
    public static bool IsUndead(NPC npc) => Compatibility.HasNpcTag(npc, "undead") ||
        npc.type >= 0 && npc.type < NPCID.Sets.Zombies.Length && (NPCID.Sets.Zombies[npc.type] || NPCID.Sets.Skeletons[npc.type]);
    public static bool IsPillarEnemy(int type) => type is
        NPCID.SolarCrawltipedeHead or NPCID.SolarDrakomire or NPCID.SolarDrakomireRider or NPCID.SolarSroller or NPCID.SolarCorite or NPCID.SolarSolenian or NPCID.SolarSpearman or
        NPCID.VortexRifleman or NPCID.VortexHornetQueen or NPCID.VortexHornet or NPCID.VortexLarva or NPCID.VortexSoldier or
        NPCID.NebulaBrain or NPCID.NebulaHeadcrab or NPCID.NebulaBeast or NPCID.NebulaSoldier or
        NPCID.StardustWormHead or NPCID.StardustCellBig or NPCID.StardustCellSmall or NPCID.StardustJellyfishBig or NPCID.StardustJellyfishSmall or NPCID.StardustSpiderBig or NPCID.StardustSpiderSmall or NPCID.StardustSoldier;
    public static bool MatchesNpc(string group, int type) => group switch
    {
        "pillars" => type is NPCID.LunarTowerSolar or NPCID.LunarTowerVortex or NPCID.LunarTowerNebula or NPCID.LunarTowerStardust,
        "pillar_enemies" => IsPillarEnemy(type),
        _ => NPCID.Search.TryGetId(group, out int id) && type == id
    };
    public static bool CanDrop(AcquisitionNpcRule rule, NPC npc, Player player)
    {
        if (!Eligible(npc)) return false;
        if (rule.group == "undead" && !IsUndead(npc)) return false;
        if (rule.group is not ("hostile" or "undead") && !MatchesNpc(rule.group, npc.type)) return false;
        if (string.IsNullOrEmpty(rule.condition)) return true;
        // The killer must actually wear the ring; a saved binding alone is insufficient.
        if (player == null || npc.lastInteraction != player.whoAmI || !WearingCurses(player)) return false;
        return rule.condition == "worn_curses" || rule.condition == "worn_curses_overworld_underground" &&
            LegacyRealms.At(player.Center) == LegacyRealms.Overworld && LegacyRealms.At(npc.Center) == LegacyRealms.Overworld &&
            player.Center.Y / 16f > Main.worldSurface && npc.Center.Y / 16f > Main.worldSurface;
    }
    internal static bool GivePersonal(Player player, string id)
    {
        bool server = Main.netMode == NetmodeID.Server;
        // A separate source prevents Looting from duplicating once-per-character rewards.
        int index = Item.NewItem(new EntitySource_Misc("ELCharacterReward"), player.Hitbox, ImportCatalog.Type(id), 1, noBroadcast: server);
        if (index < 0 || index >= Main.maxItems) return false;
        if (server)
        {
            Main.timeItemSlotCannotBeReusedFor[index] = 54000;
            NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, index);
            Main.item[index].active = false;
        }
        return true;
    }
    public static bool TryFirstChest(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        int index = player.chest;
        if (Main.netMode == NetmodeID.MultiplayerClient || legacy.FirstChestReward || !player.active || player.dead ||
            index < 0 || index >= Main.maxChests || Main.chest[index] == null) return false;
        Chest chest = Main.chest[index];
        if (!WorldGen.InWorld(chest.x, chest.y) || !Main.tile[chest.x, chest.y].HasTile || Chest.IsLocked(chest.x, chest.y)) return false;
        if (!GivePersonal(player, "enigmatic_eye")) return false;
        legacy.FirstChestReward = true; LegacyNetwork.SendAcquisition(player); return true;
    }
    public static bool TryFirstMoonLord(Player player)
    {
        if (player == null || !player.active) return false;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (Main.netMode == NetmodeID.MultiplayerClient || legacy.FirstMoonLordReward || !GivePersonal(player, "astral_fruit")) return false;
        legacy.FirstMoonLordReward = true; LegacyNetwork.SendAcquisition(player); return true;
    }
    public static bool MatchesChest(string pool, int tileType, int style, int y, bool dungeonWall)
    {
        if (tileType is not (TileID.Containers or TileID.Containers2)) return false;
        bool overworld = y + 1 < Main.UnderworldLayer;
        bool gold = tileType == TileID.Containers && style is 1 or 2;
        return pool switch
        {
            "overworld" => overworld,
            "gold" => gold,
            "dungeon_gold" => gold && dungeonWall,
            "dungeon" => dungeonWall,
            "hell_shadow" => !overworld && tileType == TileID.Containers && style is 3 or 4,
            "sky" => tileType == TileID.Containers && style == 13 && y < Main.worldSurface,
            _ => false
        };
    }
    public static void FillChest(Chest chest, Terraria.Utilities.UnifiedRandom random)
    {
        if (!WorldGen.InWorld(chest.x, chest.y) || !chest.item.Any(i => i != null && !i.IsAir)) return;
        Tile tile = Main.tile[chest.x, chest.y];
        if (!tile.HasTile) return;
        foreach (var rule in Rules.chest_rules)
        {
            if (!MatchesChest(rule.pool, tile.TileType, tile.TileFrameX / 36, chest.y, Main.wallDungeon[tile.WallType]) || random.Next(rule.denominator) >= rule.numerator) continue;
            int slot = Array.FindIndex(chest.item, i => i == null || i.IsAir);
            if (slot < 0) break; // Existing contents, including other mods' loot, are never replaced.
            chest.item[slot] = new Item(ImportCatalog.Type(rule.item), random.Next(rule.minimum, rule.maximum + 1));
        }
    }
}

internal sealed class AcquisitionDropCondition : IItemDropRuleCondition
{
    private readonly AcquisitionNpcRule rule;
    public AcquisitionDropCondition(AcquisitionNpcRule rule) => this.rule = rule;
    // Native loot supplies the nearest player, which may differ from the killer.
    public bool CanDrop(DropAttemptInfo info) => LegacyAcquisition.CanDrop(rule, info.npc, LegacyAcquisition.CreditedKiller(info.npc));
    public bool CanShowItemDropInUI() => true;
    public string GetConditionDescription() => rule.condition == "worn_curses_overworld_underground" ? BridgeUnits.Text("在主世界地下佩戴七咒之戒击杀", "Kill underground in the Overworld while wearing the Seven Curses") :
        rule.condition == "worn_curses" ? BridgeUnits.Text("佩戴七咒之戒击杀", "Kill while wearing the Seven Curses") :
        rule.group == "undead" ? BridgeUnits.Text("亡灵敌怪", "Undead enemies") : BridgeUnits.Text("敌怪掉落", "Hostile loot");
}

public sealed class AcquisitionNpcDrops : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot loot)
    {
        foreach (var rule in LegacyAcquisition.Rules.npc_rules)
            if (LegacyAcquisition.MatchesNpc(rule.group, npc.type)) loot.Add(CreateRule(rule));
    }
    public override void ModifyGlobalLoot(GlobalLoot loot)
    {
        foreach (var rule in LegacyAcquisition.Rules.npc_rules)
            if (rule.group is "hostile" or "undead") loot.Add(CreateRule(rule));
    }
    public static IItemDropRule CreateRule(AcquisitionNpcRule rule) => ItemDropRule.ByCondition(new AcquisitionDropCondition(rule),
        ImportCatalog.Type(rule.item), rule.denominator, rule.minimum, rule.maximum);
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || npc.type != NPCID.MoonLordCore || npc.SpawnedFromStatue) return;
        foreach (Player player in Main.ActivePlayers)
            if (npc.playerInteraction[player.whoAmI] || npc.lastInteraction == player.whoAmI) LegacyAcquisition.TryFirstMoonLord(player);
    }
}

public sealed class AcquisitionWorld : ModSystem
{
    private bool seeded;
    public override void ClearWorld() => seeded = false;
    public override void LoadWorldData(TagCompound tag) => seeded = tag.GetBool("chestLootSeededV1");
    public override void SaveWorldData(TagCompound tag) => tag["chestLootSeededV1"] = seeded;
    public override void PostWorldGen() => SeedChests();
    public override void PreUpdateWorld() { if (!seeded) SeedChests(); }
    public void SeedChests()
    {
        if (seeded || Main.netMode == NetmodeID.MultiplayerClient) return;
        // Also supports existing worlds once, after their containers have loaded.
        foreach (Chest chest in Main.chest) if (chest != null) LegacyAcquisition.FillChest(chest, Main.rand);
        seeded = true;
    }
    public override void PostUpdatePlayers()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        foreach (Player player in Main.ActivePlayers) LegacyAcquisition.TryFirstChest(player);
    }
    public override void AddRecipeGroups()
    {
        RecipeGroup.RegisterGroup("EnigmaticLegacy:PersonalAmulet", new RecipeGroup(
            () => BridgeUnits.Text("任意颜色的神秘护身符", "Any personal Enigmatic Amulet"), LegacyAmulets.Colors.Select(ImportCatalog.Type).ToArray()));
        foreach (var entry in new[] {
            ("IronBar", ItemID.IronBar, ItemID.LeadBar, "铁锭或铅锭", "Iron or Lead Bar"),
            ("IronOre", ItemID.IronOre, ItemID.LeadOre, "铁矿或铅矿", "Iron or Lead Ore"),
            ("GoldBar", ItemID.GoldBar, ItemID.PlatinumBar, "金锭或铂金锭", "Gold or Platinum Bar"),
            ("GoldOre", ItemID.GoldOre, ItemID.PlatinumOre, "金矿或铂金矿", "Gold or Platinum Ore"),
            ("EvilBar", ItemID.DemoniteBar, ItemID.CrimtaneBar, "魔矿锭或猩红矿锭", "Demonite or Crimtane Bar"),
            ("GoldSword", ItemID.GoldBroadsword, ItemID.PlatinumBroadsword, "金阔剑或铂金阔剑", "Gold or Platinum Broadsword") })
            RecipeGroup.RegisterGroup("EnigmaticLegacy:" + entry.Item1, new RecipeGroup(() => BridgeUnits.Text(entry.Item4, entry.Item5), entry.Item2, entry.Item3));
    }
}

public static class LegacyAmulets
{
    public static readonly string[] Colors = { "enigmatic_amulet", "enigmatic_amulet_aqua", "enigmatic_amulet_violet", "enigmatic_amulet_magenta", "enigmatic_amulet_green", "enigmatic_amulet_black", "enigmatic_amulet_blue" };
    public static bool Bind(Item item, Player player)
    {
        if (item.ModItem is not ImportedItem { ImportId: "unwitnessed_amulet" }) return false;
        item.SetDefaults(ImportCatalog.Type(Colors[Main.rand.Next(Colors.Length)]));
        var meta = item.GetGlobalItem<ItemMetadata>(); meta.BoundOwner = player.GetModPlayer<LegacyPlayer>().OwnerId; meta.BoundName = player.name;
        return true;
    }
    public static void PreserveOwner(Item output, IEnumerable<Item> consumed)
    {
        Item source = consumed.FirstOrDefault(i => i.ModItem is ImportedItem imported && (Colors.Contains(imported.ImportId) || imported.ImportId == "ascension_amulet"));
        if (source == null) return;
        var from = source.GetGlobalItem<ItemMetadata>(); var to = output.GetGlobalItem<ItemMetadata>();
        to.BoundOwner = from.BoundOwner; to.BoundName = from.BoundName;
    }
}
