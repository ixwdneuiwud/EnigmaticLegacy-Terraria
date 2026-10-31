// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public static class LegacyEconomy
{
    // Each source Looting level adds ten percentage points; roll once per kill.
    public static float DoubleLootChance(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        return Math.Min(0.5f, (legacy.Cursed ? 0.1f : 0) + (legacy.Has("monster_charm") ? 0.1f : 0) + BridgeExtensions.Sum(BridgeExtensions.LootProviders, player));
    }
    public static bool Eligible(NPC npc) => !npc.friendly && !npc.townNPC && !npc.SpawnedFromStatue && npc.lifeMax > 5 &&
        !npc.immortal && npc.realLife < 0 && npc.type != NPCID.TargetDummy;
    public static bool Avarice(Player player) => player.GetModPlayer<LegacyPlayer>().Cursed && player.GetModPlayer<LegacyPlayer>().Has("avarice_scroll");
}

public sealed class LegacyLootItems : GlobalItem
{
    private static bool duplicating;
    public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
    {
        if (LegacyEconomy.Avarice(Main.LocalPlayer) && reforgePrice > 0) reforgePrice = Math.Max(1, (int)Math.Floor(reforgePrice * 0.65));
        return true;
    }
    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (duplicating || Main.netMode == NetmodeID.MultiplayerClient || item.IsAir ||
            item.type is ItemID.Heart or ItemID.Star or ItemID.CandyApple or ItemID.CandyCane or ItemID.SoulCake or ItemID.SugarPlum) return;
        bool fixedReward = source is EntitySource_Loot { Entity: NPC boss } &&
            (boss.type == NPCID.WallofFlesh && (item.type == ImportCatalog.Type("evil_essence") || item.type == ImportCatalog.Type("nether_star")) ||
             boss.type == NPCID.MoonLordCore && item.type == ImportCatalog.Type("abyssal_heart"));
        if (fixedReward) return;
        bool loot = source is EntitySource_Loot { Entity: NPC npc } && npc.GetGlobalNPC<LegacyGlobalNPC>().DoubleLoot;
        if (!loot && !LegacyMining.ShouldDouble(source)) return;
        int extra = item.stack;
        int intoStack = Math.Min(extra, Math.Max(0, item.maxStack - item.stack));
        item.stack += intoStack; extra -= intoStack;
        try
        {
            duplicating = true;
            while (extra > 0)
            {
                Item clone = item.Clone(); clone.stack = Math.Min(extra, Math.Max(1, clone.maxStack));
                Item.NewItem(source, item.Hitbox, clone);
                extra -= clone.stack;
            }
        }
        finally { duplicating = false; }
    }
}

public sealed class LegacyTradePlayer : ModPlayer
{
    private Item purchasedStock;
    public override bool CanBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        purchasedStock = LegacyEconomy.Avarice(Player) && !item.buyOnce ? item.Clone() : null;
        return true;
    }
    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item)
    {
        // Vanilla passes the entire mouse stack here, although each call buys ONE item.
        Item stock = purchasedStock; purchasedStock = null;
        if (stock == null || !LegacyEconomy.Avarice(Player) || stock.type != item.type) return;
        int bonus = (int)Math.Round(Main.rand.NextDouble() * 2, MidpointRounding.AwayFromZero);
        if (bonus <= 0) return;
        stock.stack = bonus; stock.buyOnce = stock.isAShopItem = false;
        stock.shopCustomPrice = null; stock.shopSpecialCurrency = -1;
        Player.QuickSpawnItem(Player.GetSource_Misc("ELAvariceTrade"), stock, bonus);
    }
    public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
    {
        if (LegacyEconomy.Avarice(Player) && price > 0) price = Math.Max(1, (int)Math.Floor(price * 0.65));
    }
}
