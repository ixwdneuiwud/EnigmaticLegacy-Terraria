// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaticLegacy.Content;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortAdaptationChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        int mode = Main.netMode; Main.netMode = NetmodeID.SinglePlayer;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        ImportedItem Imported(string id) => (ImportedItem)new Item(ImportCatalog.Type(id)).ModItem;
        void Reset(params string[] ids)
        {
            foreach (Item item in player.inventory.Concat(player.armor)) item.TurnToAir();
            Array.Clear(player.buffType); Array.Clear(player.buffTime); Array.Clear(player.buffImmune);
            player.ResetEffects(); legacy.ResetEffects(); legacy.BoundCurses = legacy.BoundDesolation = false;
            legacy.LostSouls = legacy.SpellCooldown = 0;
            player.immune = player.dead = false; player.immuneTime = 0;
            player.wings = player.wingsLogic = player.wingTimeMax = 0; player.wingTime = 0; player.equippedWings = null;
            player.controlJump = false; player.velocity = Vector2.Zero;
            player.statLife = player.statLifeMax = player.statLifeMax2 = 1000;
            player.statMana = player.statManaMax = player.statManaMax2 = 200;
            legacy.PostUpdateEquips(); // Clear scroll payment/anchor state between cases.
            foreach (string id in ids) legacy.Equip(Imported(id));
        }
        Point point = new(140, (int)Main.rockLayer + 90);
        for (int x = point.X - 52; x <= point.X + 52; x++)
            for (int y = point.Y - 52; y <= point.Y + 52; y++) { Tile tile = Main.tile[x, y]; tile.HasTile = false; }
        player.Center = point.ToWorldCoordinates();
        Reset("enigmatic_item"); player.velocity = new Vector2(3, 4); legacy.PostUpdateEquips();
        check("Creative flight uses native wing logic without replacing velocity", player.wingsLogic > 0 && player.wingTime >= player.wingTimeMax && player.velocity == new Vector2(3, 4));
        player.controlJump = true;
        typeof(Player).GetMethod("WingMovement", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(player, null);
        check("Native wing movement actually lifts the flight bearer", player.velocity.Y < 4);
        Reset("enigmatic_elytra"); legacy.PostUpdateEquips();
        int wing = player.wingsLogic; var elytra = Imported("enigmatic_elytra"); float speed = 1, acceleration = 1;
        player.controlJump = true; elytra.HorizontalWingSpeeds(player, ref speed, ref acceleration);
        check("Enhanced elytra retains boost and unlimited native wing time", wing == elytra.Item.wingSlot && player.wingTimeMax > 0 && player.wingTime == player.wingTimeMax && speed == 18 && acceleration > 1);
        Reset("heaven_scroll"); legacy.PostUpdateEquips();
        check("Heaven scroll does not grant flight outside an anchor", player.wingsLogic == 0 && player.statMana == 200);
        Tile anchor = Main.tile[point.X + 2, point.Y]; anchor.HasTile = true; anchor.TileType = TileID.CrystalBall;
        Reset("heaven_scroll"); player.controlJump = true; player.velocity.Y = 1; legacy.PostUpdateEquips();
        check("Heaven scroll grants native wings and pays mana near a crystal ball", player.wingsLogic > 0 && player.statMana == 198 && player.wingTime > 0);
        for (int i = 0; i < 59; i++) legacy.PostUpdateEquips();
        check("Flight payment occurs once per second rather than every frame", player.statMana == 198);
        legacy.PostUpdateEquips();
        check("Sustained flight renews its mana payment", player.statMana == 196);
        Reset("heaven_scroll"); player.statMana = 2; player.controlJump = true; player.velocity.Y = 1;
        legacy.PostUpdateEquips(); player.wings = player.wingsLogic = player.wingTimeMax = 0; player.equippedWings = null;
        legacy.PostUpdateEquips();
        check("The last mana payment still buys its complete second of flight", player.statMana == 0 && player.wingsLogic > 0);
        Reset("heaven_scroll"); legacy.PostUpdateEquips();
        Vector2 anchorPosition = player.Center; player.Center = new Vector2(player.Center.X, (Main.UnderworldLayer + 2) * 16);
        player.wings = player.wingsLogic = player.wingTimeMax = 0; player.equippedWings = null;
        legacy.PostUpdateEquips();
        check("Crossing the realm boundary immediately invalidates cached flight anchors", player.wingsLogic == 0);
        player.Center = anchorPosition;
        Reset("fabulous_scroll"); player.statMana = 0; player.controlJump = true; player.velocity.Y = 1; legacy.PostUpdateEquips();
        check("Creator scroll flies free inside anchor range even with zero mana", player.wingsLogic > 0 && player.statMana == 0);
        anchor.HasTile = false;
        Reset("fabulous_scroll"); player.controlJump = true; player.velocity.Y = 1; legacy.PostUpdateEquips();
        check("Creator scroll flies outside anchor range at eight mana per second", player.wingsLogic > 0 && player.statMana == 192);
        Reset("fabulous_scroll"); player.statMana = 0; player.controlJump = true; player.velocity.Y = 1; legacy.PostUpdateEquips();
        check("Mana depletion disables paid flight without free replenishment", player.wingsLogic == 0 && player.statMana == 0);

        Reset(); legacy.BoundCurses = true; int bonus = player.manaRegenBonus; legacy.PostUpdateMiscEffects(); int cursesBonus = player.manaRegenBonus - bonus;
        Reset("enchanter_pearl"); legacy.BoundCurses = true; bonus = player.manaRegenBonus; legacy.PostUpdateMiscEffects();
        check("Curses and enchanter pearl map enchanting strength to native mana regeneration", cursesBonus > 0 && player.manaRegenBonus - bonus > cursesBonus && player.manaRegenDelayBonus > 0);
        Reset(); legacy.BoundCurses = true; legacy.Carry("enchanter_pearl"); bonus = player.manaRegenBonus; legacy.PostUpdateMiscEffects();
        check("A spare enchanter pearl in the backpack grants no equipment effect", player.manaRegenBonus - bonus == cursesBonus);
        Reset("desolation_ring"); int rate = 600, max = 6; var global = new LegacyGlobalNPC(); global.EditSpawnRate(player, ref rate, ref max);
        check("Desolation slows all native and modded spawn pools through the native scheduler", rate == 1200 && max == 3);
        rate = 600; max = 1; global.EditSpawnRate(player, ref rate, ref max);
        check("Desolation preserves a positive spawn cap at its minimum", max == 1);
        Reset("monster_charm"); float emblemChance = LegacyEconomy.DoubleLootChance(player); legacy.BoundCurses = true;
        check("Looting sources combine into a twenty percent double loot chance", Math.Abs(emblemChance - 0.1f) < 0.0001f && Math.Abs(LegacyEconomy.DoubleLootChance(player) - 0.2f) < 0.0001f);
        NPC npc = Main.npc[197]; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = 197; npc.active = true; npc.Center = player.Center;
        npc.lastInteraction = player.whoAmI; npc.SpawnedFromStatue = true; npc.GetGlobalNPC<LegacyGlobalNPC>().PreKill(npc);
        check("Statue mobs cannot trigger double loot", !npc.GetGlobalNPC<LegacyGlobalNPC>().DoubleLoot);
        npc.SpawnedFromStatue = false; npc.GetGlobalNPC<LegacyGlobalNPC>().DoubleLoot = true;
        Item relic = Imported("iron_ring").Item; relic.Prefix(PrefixID.Warding); relic.GetGlobalItem<ItemMetadata>().VoidFilters.Add("diagnostic");
        int CountRelics() => Main.item.Count(i => i.active && i.type == relic.type && i.prefix == PrefixID.Warding && i.GetGlobalItem<ItemMetadata>().VoidFilters.Contains("diagnostic"));
        int before = CountRelics(); Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, relic);
        check("Double loot clones unstackable items and preserves prefixes and GlobalItem data", CountRelics() == before + 2);
        int CountRope() => Main.item.Where(i => i.active && i.type == ItemID.Rope).Sum(i => i.stack);
        before = CountRope(); Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.Rope, 7);
        check("A successful loot roll doubles the exact dropped stack", CountRope() == before + 14);
        before = CountRope(); Item.NewItem(player.GetSource_Misc("NotLoot"), player.Hitbox, ItemID.Rope, 7);
        check("Loot duplication cannot affect unrelated drops or recurse", CountRope() == before + 7);
        npc.active = false;

        Reset("mining_charm"); float miningChance = LegacyMining.DoubleOreChance(player);
        legacy.BoundCurses = true; legacy.Equip(Imported("the_cube")); legacy.Equip(Imported("avarice_scroll"));
        check("Fortune sources stack from ten to forty percent", Math.Abs(miningChance - 0.1f) < 0.0001f && Math.Abs(LegacyMining.DoubleOreChance(player) - 0.4f) < 0.0001f);
        Reset("mining_charm");
        int CountOre() => Main.item.Where(i => i.active && i.type == ItemID.IronOre).Sum(i => i.stack);
        int oreBefore = CountOre();
        Point mine = new(point.X + 8, point.Y + 8);
        for (int i = 0; i < 240; i++)
        {
            Tile tile = Main.tile[mine.X, mine.Y]; tile.HasTile = true; tile.TileType = TileID.Iron;
            player.PickTile(mine.X, mine.Y, 1000);
        }
        int mined = CountOre() - oreBefore;
        check("Actual native pickaxe mining can double ore at the configured probability", mined > 245 && mined < 300 && !Main.tile[mine.X, mine.Y].HasTile);
        oreBefore = CountOre();
        Item.NewItem(new EntitySource_TileBreak(mine.X, mine.Y), player.Hitbox, ItemID.IronOre, 5);
        check("Unattributed tile drops cannot borrow a nearby player's Fortune", CountOre() == oreBefore + 5);
        Tile plain = Main.tile[mine.X, mine.Y]; plain.HasTile = true; plain.TileType = TileID.Stone;
        using (LegacyMining.Begin(player, mine.X, mine.Y))
        {
            oreBefore = CountOre(); Item.NewItem(new EntitySource_TileBreak(mine.X, mine.Y), player.Hitbox, ItemID.IronOre, 5);
            check("Nonmineral tiles cannot trigger Fortune", CountOre() == oreBefore + 5);
        }
        plain.TileType = TileID.Iron;
        using (LegacyMining.Begin(player, mine.X, mine.Y))
        {
            oreBefore = CountOre(); Item.NewItem(new EntitySource_TileBreak(mine.X + 1, mine.Y), player.Hitbox, ItemID.IronOre, 5);
            check("Fortune is limited to the exact tile the owner is mining", CountOre() == oreBefore + 5);
        }
        Main.netMode = NetmodeID.MultiplayerClient;
        using (LegacyMining.Begin(player, mine.X, mine.Y))
        {
            check("Clients cannot duplicate server-owned mineral drops", !LegacyMining.ShouldDouble(new EntitySource_TileBreak(mine.X, mine.Y)));
        }
        Main.netMode = NetmodeID.Server;
        var miningPacket = new MessageBuffer { whoAmI = player.whoAmI };
        miningPacket.readBuffer[0] = MessageID.TileManipulation; miningPacket.readBuffer[1] = 0;
        BitConverter.GetBytes((short)mine.X).CopyTo(miningPacket.readBuffer, 2); BitConverter.GetBytes((short)mine.Y).CopyTo(miningPacket.readBuffer, 4);
        bool serverDouble = false;
        for (int i = 0; i < 240; i++)
            using (LegacyMining.PacketScope(miningPacket, 0, 9)) serverDouble |= LegacyMining.ShouldDouble(new EntitySource_TileBreak(mine.X, mine.Y));
        check("Native mining packet identifies the correct Fortune owner on the server", serverDouble && !LegacyMining.ShouldDouble(new EntitySource_TileBreak(mine.X, mine.Y)));
        miningPacket.readBuffer[1] = 1;
        using (LegacyMining.PacketScope(miningPacket, 0, 9))
            check("Placement packets cannot create a mining reward scope", !LegacyMining.ShouldDouble(new EntitySource_TileBreak(mine.X, mine.Y)));
        plain.HasTile = false; Main.netMode = NetmodeID.SinglePlayer;

        Reset("avarice_scroll"); legacy.BoundCurses = true;
        var stock = new Item(ItemID.Rope) { value = 10000, stack = 9999 };
        Item[] shop = { stock, new Item(ItemID.Rope) { value = 10000, buyOnce = true }, null };
        global.ModifyActiveShop(new NPC(), "diagnostics", shop);
        check("All NPC shops receive the discount without mutating shared stock or buybacks", shop[0].shopCustomPrice == 6500 && stock.shopCustomPrice == null && shop[1].shopCustomPrice == null && shop[2] == null);
        var trade = player.GetModPlayer<LegacyTradePlayer>(); before = CountRope();
        for (int i = 0; i < 20; i++) { trade.CanBuyItem(new NPC(), shop, stock); trade.PostBuyItem(new NPC(), shop, new Item(ItemID.Rope, 500)); }
        check("Barter quantity bonus counts purchases rather than the full mouse stack", CountRope() > before && CountRope() <= before + 40);
        before = CountRope(); trade.CanBuyItem(new NPC(), shop, shop[1]); trade.PostBuyItem(new NPC(), shop, shop[1]);
        check("Buyback transactions cannot generate barter bonus copies", CountRope() == before);
        int price = 10000; trade.ModifyNursePrice(new NPC(), 10, false, ref price);
        check("NPC healing services receive the same trade discount", price == 6500);
        bool discount = true; int reforge = 10000; new LegacyLootItems().ReforgePrice(stock, ref reforge, ref discount);
        check("Reforging receives the NPC trade discount and retains native discount stacking", reforge == 6500 && discount);

        bool rain = Main.raining; double rainTime = Main.rainTime; float intensity = Main.maxRaining;
        try
        {
            Reset("ocean_stone"); Main.raining = false; LegacyActions.ActivateSpellstone(player, player.Center);
            check("Ocean storm pays one hundred fifty mana and starts native rain", Main.raining && player.statMana == 50 && legacy.SpellCooldown == 1800 && Main.rainTime >= 30000);
            Reset("ocean_stone"); Main.raining = false; player.statMana = 149; LegacyActions.ActivateSpellstone(player, player.Center);
            check("Insufficient mana cannot trigger a storm or consume resources", !Main.raining && player.statMana == 149 && legacy.SpellCooldown == 0);
            Vector2 previous = player.Center; player.Center = new Vector2(player.Center.X, (Main.UnderworldLayer + 10) * 16);
            Reset("ocean_stone"); Main.raining = false; LegacyActions.ActivateSpellstone(player, player.Center);
            check("Storms cannot be summoned from the Nether height region", !Main.raining && player.statMana == 200 && legacy.SpellCooldown == 0);
            player.Center = previous;
        }
        finally { Main.raining = rain; Main.rainTime = rainTime; Main.maxRaining = intensity; }

        Reset(); var dragon = new Projectile(); dragon.SetDefaults(ProjectileID.StardustDragon1); dragon.active = true; dragon.owner = player.whoAmI;
        check("Stardust Dragon is recognized as a combat summon", LegacySummons.IsSummon(dragon));
        bool segments = true;
        foreach (int type in new[] { ProjectileID.StardustDragon1, ProjectileID.StardustDragon2, ProjectileID.StardustDragon3, ProjectileID.StardustDragon4 })
        { var segment = new Projectile(); segment.SetDefaults(type); segment.active = true; segments &= LegacySummons.IsSummon(segment); }
        check("Every Stardust Dragon segment maps to a combat summon", segments);
        details.Add($"Stardust Dragon: minion={dragon.minion}, friendly={dragon.friendly}, projPet={Main.projPet[dragon.type]}.");
        check("Summon classifier does not treat an ordinary arrow as a pet", !LegacySummons.IsSummon(new Projectile { active = true, friendly = true, type = ProjectileID.WoodenArrowFriendly }));
        Reset("enigmatic_amulet"); legacy.PostUpdateEquips();
        check("Original amulet attack attribute boosts native melee without leaking into summons", Math.Abs(player.GetTotalDamage(DamageClass.Melee).ApplyTo(100) - (100 + BridgeUnits.Health(1.5f))) < 0.01f && Math.Abs(player.GetTotalDamage(DamageClass.Summon).ApplyTo(100) - 100) < .01f);
        Reset("monster_charm");
        var summonHit = new NPC.HitModifiers { DamageType = DamageClass.Summon }; legacy.ModifyHitNPC(new NPC { type = NPCID.Zombie }, ref summonHit);
        check("Combat summons receive the hunter emblem's target damage bonus", Math.Abs(summonHit.FinalDamage.ApplyTo(100) - 125) < 0.01f);
        legacy.Carry("hunter_guidebook"); legacy.Carry("animal_guidebook"); int life = player.statLife;
        new LegacyGlobalProjectile().PostAI(dragon);
        check("Guidebooks do not invent summon injuries or drain the owner", player.statLife == life);

        Reset(); var darkness = LegacyDamage.Type("darkness"); var modifiers = new NPC.HitModifiers { DamageType = darkness };
        global.ModifyIncomingHit(new NPC(), ref modifiers);
        check("Darkness source is reusable and bypasses armor and resistance", darkness.Traits.HasFlag(ImportedDamageTraits.BypassesResistance) && modifiers.DefenseEffectiveness.Value == 0);
        check("Future ports can obtain the six imported source damage categories", new[] { "darkness", "nemesis_curse", "wither", "void", "indirect_magic", "explosion" }.All(id => LegacyDamage.Type(id) != null));
        npc.SetDefaults(NPCID.BlueSlime); npc.active = true; npc.Center = player.Center; npc.life = npc.lifeMax = 1000; npc.defense = 0;
        player.ApplyDamageToNPC(npc, 20, 0, 0, false, darkness, damageVariation: false);
        int unarmoredDarkness = 1000 - npc.life;
        npc.SetDefaults(NPCID.BlueSlime); npc.active = true; npc.Center = player.Center; npc.life = npc.lifeMax = 1000; npc.defense = 1000;
        npc.AddBuff(ModContent.BuffType<ResistanceIII>(), 600);
        player.ApplyDamageToNPC(npc, 20, 0, 0, false, darkness, damageVariation: false);
        details.Add($"Native darkness actual hit: unarmored {unarmoredDarkness}, 1000 defense plus Resistance III {1000 - npc.life}, player {player.whoAmI}, class {darkness.FullName}, class damage {player.GetTotalDamage(darkness).ApplyTo(100)}.");
        // Preserve unrelated installed-mod hit hooks; compare against the same
        // source without armor/resistance rather than overriding their damage.
        check("Native darkness strikes bypass defense and the resistance buff in actual combat", unarmoredDarkness > 0 && 1000 - npc.life == unarmoredDarkness);
        npc.active = false;
        Reset("void_pearl"); player.statLife = 500;
        LegacyDamage.Hurt(player, LegacyDamage.Type("indirect_magic"), PlayerDeathReason.ByOther(0), 20);
        check("Void pearl converts imported indirect magic into healing", player.statLife > 500 && legacy.IncomingDamageType == null);
        var positives = new HashSet<int>(); bool durations = true, emptyDebuffs = false;
        for (int i = 0; i < 2000; i++)
        {
            var positive = LegacyRandomEffects.Positive(); var negative = LegacyRandomEffects.Negative(); positives.Add(positive.Buff);
            durations &= positive.Ticks >= 300 && positive.Ticks <= 1797 && negative.Ticks >= 600 && negative.Ticks <= 3597;
            emptyDebuffs |= negative.Buff == 0;
        }
        check("Cube random pool covers eight positive families and source durations", positives.Count == 15 && positives.Contains(BuffID.Gills) && durations);
        check("Cube leaves hunger and mining fatigue slots empty", emptyDebuffs);
        Reset("angel_blessing", "cosmic_scroll"); legacy.PostUpdateEquips(); bool sound = false;
        var extraJump = ModContent.GetInstance<AngelFlightJump>(); extraJump.OnStarted(player, ref sound);
        check("Angel uses a native extra jump with its special three quarter second cooldown", player.GetJumpState<AngelFlightJump>().Enabled && legacy.SpellCooldown == 45 && !extraJump.CanStart(player));
        Reset("mega_sponge"); Tile water = Main.tile[point.X, point.Y]; water.HasTile = false; water.LiquidType = LiquidID.Water; water.LiquidAmount = 255; player.Center = point.ToWorldCoordinates();
        legacy.PostUpdate();
        check("Megasponge remains an inert easter egg and cannot drain water", water.LiquidAmount == 255 && !LegacyActions.CanUse(player, Imported("mega_sponge")));
        check("Text crop and durability utilities have no active behavior", new[] { "lore_inscriber", "lore_fragment", "infinimeal", "mending_mixture", "insignia" }.All(id => !LegacyActions.CanUse(player, Imported(id))));

        // Actual liquid tiles distinguish immersion from rain, honey and lava.
        Reset("ocean_stone"); player.Center = point.ToWorldCoordinates();
        for (int x = point.X - 2; x <= point.X + 2; x++)
            for (int y = point.Y - 3; y <= point.Y + 3; y++)
            { Tile tile = Main.tile[x, y]; tile.HasTile = false; tile.LiquidType = LiquidID.Water; tile.LiquidAmount = 255; }
        player.wet = true; player.lavaWet = player.honeyWet = player.shimmerWet = false;
        player.gravity = Player.defaultGravity; player.velocity = new Vector2(2, -1);
        legacy.PostUpdateEquips(); legacy.PostUpdateRunSpeeds();
        check("Ocean immersion removes gravity without replacing swimming velocity", player.gravity == 0 && player.velocity == new Vector2(2, -1) && player.nightVision);
        for (int x = point.X - 2; x <= point.X + 2; x++)
            for (int y = point.Y - 3; y < point.Y; y++) Main.tile[x, y].LiquidAmount = 0;
        Reset("ocean_stone"); player.wet = true; player.gravity = Player.defaultGravity;
        legacy.PostUpdateEquips(); legacy.PostUpdateRunSpeeds();
        check("Ocean gravity returns when the player's head leaves the water", player.gravity == Player.defaultGravity && !player.nightVision);
        for (int x = point.X - 2; x <= point.X + 2; x++)
            for (int y = point.Y - 3; y <= point.Y + 3; y++) Main.tile[x, y].LiquidAmount = 0;
        Reset("ocean_stone"); player.wet = player.honeyWet = true; player.gravity = Player.defaultGravity;
        legacy.PostUpdateEquips(); legacy.PostUpdateRunSpeeds();
        check("Honey does not grant Ocean water-only vision or zero gravity", player.gravity == Player.defaultGravity && !player.nightVision);
        player.wet = player.honeyWet = false;


        Reset(); var eye = Imported("enigmatic_eye"); var narrator = player.GetModPlayer<LegacyNarrator>(); narrator.Unlocked = false; narrator.Heard.Clear(); LegacyNarrative.Clear();
        check("Dormant eye cannot be equipped", !eye.CanEquipAccessory(player, 3, false));
        LegacyActions.ExecuteUtility(player, eye, player.Center, false);
        check("Eye awakening uses both original transition textures before enabling equipment", !eye.Tainted && eye.EyeAwakeningTicks == 12 && LegacyEyeVisual.Texture(eye).EndsWith("_dormant_1"));
        for (int tick = 0; tick < 6; tick++) eye.UpdateInventory(player);
        check("Eye awakening switches to the original second transition after two source ticks", eye.EyeAwakeningTicks == 6 && LegacyEyeVisual.Texture(eye).EndsWith("_dormant_2"));
        for (int tick = 0; tick < 6; tick++) eye.UpdateInventory(player);
        check("Eye awakened frames preserve original long idle and six-frame order", LegacyEyeVisual.ActiveFrame(0) == 0 && LegacyEyeVisual.ActiveFrame(143) == 0 && LegacyEyeVisual.ActiveFrame(144) == 1 && LegacyEyeVisual.ActiveFrame(173) == 5 && LegacyEyeVisual.ActiveFrame(174) == 0);
        check("Awakened eye selects one square frame rather than the entire strip", LegacyEyeVisual.Frame(eye, 32, 192, 150) == new Rectangle(0, 64, 32, 32));
        var reloadedEye = (ImportedItem)ItemIO.Load(ItemIO.Save(eye.Item)).ModItem;
        check("Eye instance awakening persists without changing dormant instances", reloadedEye.Tainted && !Imported("enigmatic_eye").Tainted);
        check("Awakening unlocks original narration once and enables equipping", eye.Tainted && narrator.Unlocked && LegacyNarrative.PendingCount == 1 && eye.CanEquipAccessory(player, 3, false) && !LegacyActions.CanUse(player, eye));
        narrator.Unlock(1); narrator.Once("sulfur_air", 1); narrator.Once("sulfur_air", 1);
        check("Narrator introductions and Nether lines do not duplicate on inventory updates", LegacyNarrative.PendingCount == 2 && narrator.Heard.Contains("sulfur_air"));
        var saved = new TagCompound(); narrator.SaveData(saved); narrator.Unlocked = false; narrator.Heard.Clear(); narrator.LoadData(saved);
        check("Narrator unlock and heard-once state persist", narrator.Unlocked && narrator.Heard.Contains("sulfur_air"));
        check("All twenty eight original voice files and subtitle records are bundled", LegacyNarrative.ClipCount == 28 && ImportCatalog.Owner.FileExists("Assets/Sounds/Quotes/sulfur_air.ogg"));
        Reset(); eye.UpdateAccessory(player, false); int reach = player.blockRange; legacy.PostUpdateEquips();
        check("Awakened eye grants three native tiles of reach", legacy.Has("enigmatic_eye") && player.blockRange == reach + 3);
        LegacyNarrative.Clear();
        Reset(); Main.netMode = mode;
    }
}
