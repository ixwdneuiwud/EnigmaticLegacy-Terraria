// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
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
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortAcquisitionChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        int oldMode = Main.netMode; Main.netMode = NetmodeID.SinglePlayer;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        Item Imported(string id) => new(ImportCatalog.Type(id));
        void Reset()
        {
            Array.Clear(player.buffType); Array.Clear(player.buffTime); Array.Clear(player.buffImmune);
            foreach (Item item in player.inventory.Concat(player.armor)) item.TurnToAir();
            player.ResetEffects(); legacy.ResetEffects(); legacy.BoundCurses = false;
            player.active = true; player.dead = false; player.chest = -1;
        }
        Recipe[] Recipes(string id) => Main.recipe.Take(Recipe.numRecipes).Where(r => r.createItem.type == ImportCatalog.Type(id)).ToArray();
        bool Contains(Recipe r, string id, int count) => r.requiredItem.Any(i => i.type == ImportCatalog.Type(id) && i.stack == count);
        Reset();
        foreach (var pair in new[] { (block: "astral_block", material: "astral_dust"), (block: "etherium_block", material: "etherium_ingot") })
        {
            check("Source compression and decomposition: " + pair.block,
                Recipes(pair.block).Any(r => r.createItem.stack == 1 && Contains(r, pair.material, 9)) &&
                Recipes(pair.material).Any(r => r.createItem.stack == 9 && Contains(r, pair.block, 1)) &&
                Imported(pair.block).createTile >= TileID.Count);
        }
        check("Hidden Java recipes and Cursed Scroll register natively", new[] { "the_cube", "ascension_amulet", "eldritch_amulet", "cosmic_scroll", "cursed_scroll" }.All(id => Recipes(id).Length == 1));
        var smelts = Recipes("etherium_ingot").Where(r => Contains(r, "etherium_ore", 1)).ToArray();
        check("Etherium smelts only at the shared Adamantite or Titanium Forge tile", smelts.Length == 2 && smelts.All(r => r.requiredTile.SequenceEqual(new[] { (int)TileID.AdamantiteForge }) && r.createItem.stack == 1));
        check("Hidden Eldritch recipe uses the independent Nether Star", Recipes("eldritch_amulet").Any(r => Contains(r, "nether_star", 1) && !r.requiredItem.Any(i => i.type == ItemID.FragmentSolar)));
        check("Enchanted book and experience bottle recipes have native material substitutes", Recipes("cursed_scroll").Any(r => r.requiredItem.Any(i => i.type == ItemID.SpellTome)) && Recipes("monster_charm").Length > 0 && Recipes("xp_scroll").Length > 0);
        var nativeRecipes = Main.recipe.Take(Recipe.numRecipes).Where(r => r.createItem.ModItem is ImportedItem).ToArray();
        check("Native iron and gold inputs accept the alternate world ores and bars", new[] { (ItemID.IronBar, ItemID.LeadBar), (ItemID.GoldBar, ItemID.PlatinumBar), (ItemID.GoldBroadsword, ItemID.PlatinumBroadsword) }.All(pair => nativeRecipes.Where(r => r.requiredItem.Any(i => i.type == pair.Item1)).Any() && nativeRecipes.Where(r => r.requiredItem.Any(i => i.type == pair.Item1)).All(r => r.acceptedGroups.Any(g => RecipeGroup.recipeGroups[g].ValidItems.Contains(pair.Item1) && RecipeGroup.recipeGroups[g].ValidItems.Contains(pair.Item2)))));
        Recipe twist = Recipes("the_twist").Single(), heart = Recipes("twisted_heart").Single();
        check("Twist keeps its four Evil Essence gate with early metal and lens inputs", Contains(twist, "evil_essence", 4) &&
            twist.requiredItem.Any(i => i.type == ItemID.DemoniteBar && i.stack == 2) &&
            twist.acceptedGroups.Any(g => RecipeGroup.recipeGroups[g].ValidItems.Contains(ItemID.DemoniteBar) && RecipeGroup.recipeGroups[g].ValidItems.Contains(ItemID.CrimtaneBar)) &&
            heart.requiredItem.Any(i => i.type == ItemID.Lens && i.stack == 1) && heart.requiredItem.Any(i => i.type == ItemID.HellstoneBar && i.stack == 2));
        check("Early Minecraft inputs no longer require lunar bars, mechanical souls or living fire", nativeRecipes.All(r =>
            !r.requiredItem.Any(i => i.type is ItemID.LunarBar or ItemID.SoulofSight or ItemID.LivingFireBlock)));
        check("Blaze rods cost twice as much as powder and merge into one Hellstone stack", new[] { ("ender_rod", 4), ("forbidden_axe", 4), ("infernal_shield", 6) }.All(pair =>
            Recipes(pair.Item1).Any(r => r.requiredItem.Count(i => i.type == ItemID.HellstoneBar) == 1 && r.requiredItem.Any(i => i.type == ItemID.HellstoneBar && i.stack == pair.Item2))));
        check("Phantom membranes use Flight souls while original feather ingredients remain separate", new[] { ("cursed_scroll", 2, 1), ("curse_transposer", 2, 0), ("escape_scroll", 4, 1), ("extradimensional_eye", 1, 0), ("mending_mixture", 1, 0) }.All(pair =>
            Recipes(pair.Item1).Any(r => !r.requiredItem.Any(i => i.type == ItemID.Ectoplasm) &&
                r.requiredItem.Any(i => i.type == ItemID.SoulofFlight && i.stack == pair.Item2) && r.requiredItem.Where(i => i.type == ItemID.Feather).Sum(i => i.stack) == pair.Item3)));
        Recipe ascension = Recipes("ascension_amulet").Single();
        check("Every personal amulet color can craft Ascension", ascension.acceptedGroups.Any(g => LegacyAmulets.Colors.All(id => RecipeGroup.recipeGroups[g].ValidItems.Contains(ImportCatalog.Type(id)))));
        Recipe cosmic = Recipes("cosmic_scroll").Single();
        player.inventory[0] = Imported("cosmic_heart");
        check("Cosmic Scroll consumes an ordinary Cosmic Heart without a blessing gate", Contains(cosmic, "cosmic_heart", 1) && !cosmic.Conditions.Any(c => c.Description.Key.EndsWith("BlessedHeart")) && !((ImportedItem)player.inventory[0].ModItem).Blessed);
        ((ImportedItem)player.inventory[0].ModItem).Blessed = true;
        check("Legacy blessed hearts retain saved data but are no longer mandatory recipe credentials", ((ImportedItem)ItemIO.Load(ItemIO.Save(player.inventory[0])).ModItem).Blessed && cosmic.Conditions.All(c => !c.Description.Key.EndsWith("BlessedHeart")));

        Item amulet = Imported("unwitnessed_amulet");
        bool bound = LegacyAmulets.Bind(amulet, player);
        var meta = amulet.GetGlobalItem<ItemMetadata>();
        check("Starting amulet binds to its player with one of seven random effects", bound && LegacyAmulets.Colors.Contains(((ImportedItem)amulet.ModItem).ImportId) && meta.BoundOwner == legacy.OwnerId && meta.BoundName == player.name && !LegacyAmulets.Bind(amulet, player));
        var restoredAmulet = ItemIO.Load(ItemIO.Save(amulet)).GetGlobalItem<ItemMetadata>();
        Item ascended = Imported("ascension_amulet"); LegacyAmulets.PreserveOwner(ascended, new[] { amulet });
        Item eldritch = Imported("eldritch_amulet"); LegacyAmulets.PreserveOwner(eldritch, new[] { ascended });
        check("Amulet owner survives item save and both upgrades", restoredAmulet.BoundOwner == legacy.OwnerId && eldritch.GetGlobalItem<ItemMetadata>().BoundOwner == legacy.OwnerId);

        var rules = LegacyAcquisition.Rules;
        AcquisitionNpcRule Drop(string id, string group = null) => rules.npc_rules.First(r => r.item == id && (group == null || r.group == group));
        NPC Npc(int type)
        {
            var npc = new NPC(); npc.SetDefaults(type); npc.Center = player.Center;
            npc.lastInteraction = player.whoAmI; return npc;
        }
        var bosses = new[] { ("eye_of_nebula", NPCID.CultistBoss), ("void_pearl", NPCID.MoonLordCore), ("ocean_stone", NPCID.DukeFishron), ("guardian_heart", NPCID.DukeFishron) };
        check("All four ordinary Boss relics have guaranteed native drop rules", bosses.All(b => Drop(b.Item1).denominator == 1 && LegacyAcquisition.CanDrop(Drop(b.Item1), Npc(b.Item2), player)));
        var flesh = Npc(NPCID.WallofFlesh); var moon = Npc(NPCID.MoonLordCore);
        legacy.BoundCurses = true;
        check("Permanent curse binding alone does not unlock conditional Boss drops", !LegacyAcquisition.CanDrop(Drop("evil_essence"), flesh, player) && !LegacyAcquisition.CanDrop(Drop("abyssal_heart"), moon, player));
        legacy.Equip((ImportedItem)Imported("cursed_ring").ModItem);
        check("Wearing Seven Curses unlocks flesh and Moon Lord rewards", LegacyAcquisition.CanDrop(Drop("evil_essence"), flesh, player) && LegacyAcquisition.CanDrop(Drop("nether_star"), flesh, player) && LegacyAcquisition.CanDrop(Drop("abyssal_heart"), moon, player));
        flesh.lastInteraction = 0;
        check("A different player's kill cannot borrow the wearer's qualification", !LegacyAcquisition.CanDrop(Drop("evil_essence"), flesh, player));
        flesh.lastInteraction = player.whoAmI;
        var nearby = new Player { active = true, whoAmI = 253 };
        var nativeInfo = new DropAttemptInfo { npc = flesh, player = nearby, rng = new UnifiedRandom(1) };
        check("Native loot checks the credited killer even when another player is closer", AcquisitionNpcDrops.CreateRule(Drop("evil_essence")).CanDrop(nativeInfo));
        int Count(string id) => Main.item.Where(i => i.active && i.type == ImportCatalog.Type(id)).Sum(i => i.stack);
        int[] before = new[] { "evil_essence", "nether_star", "abyssal_heart" }.Select(Count).ToArray();
        foreach (var (id, npc) in new[] { ("evil_essence", flesh), ("nether_star", flesh), ("abyssal_heart", moon) })
        {
            npc.GetGlobalNPC<LegacyGlobalNPC>().DoubleLoot = true;
            var rule = AcquisitionNpcDrops.CreateRule(Drop(id));
            var info = new DropAttemptInfo { npc = npc, player = player, rng = new UnifiedRandom(12) };
            if (rule.CanDrop(info)) rule.TryDroppingItem(info);
        }
        check("Native drops preserve fixed 10 essence and one star or heart even with Looting", Count("evil_essence") - before[0] == 10 && Count("nether_star") - before[1] == 1 && Count("abyssal_heart") - before[2] == 1);
        float underground = ((float)Main.worldSurface + Main.UnderworldLayer) / 2 * 16;
        player.Center = new Vector2(2500, underground); var enemy = Npc(NPCID.Zombie);
        bool undergroundDrop = LegacyAcquisition.CanDrop(Drop("earth_heart"), enemy, player);
        player.Center = new Vector2(2500, (Main.UnderworldLayer + 1) * 16);
        bool crossBoundary = LegacyAcquisition.CanDrop(Drop("earth_heart"), enemy, player);
        enemy.Center = player.Center;
        bool hellDrop = LegacyAcquisition.CanDrop(Drop("earth_heart"), enemy, player);
        player.Center = new Vector2(2500, ((float)Main.worldSurface - 10) * 16); enemy.Center = player.Center;
        check("Earth Heart cursed kills require both entities underground above Hell", undergroundDrop && !crossBoundary && !hellDrop && !LegacyAcquisition.CanDrop(Drop("earth_heart"), enemy, player) && Drop("earth_heart").denominator == 2000);
        check("All four pillars and pillar enemy heads are recognized without segmented bodies", new[] { NPCID.LunarTowerSolar, NPCID.LunarTowerVortex, NPCID.LunarTowerNebula, NPCID.LunarTowerStardust }.All(t => LegacyAcquisition.MatchesNpc("pillars", t)) && LegacyAcquisition.IsPillarEnemy(NPCID.SolarCrawltipedeHead) && LegacyAcquisition.IsPillarEnemy(NPCID.StardustWormHead) && !LegacyAcquisition.IsPillarEnemy(NPCID.SolarCrawltipedeBody) && !LegacyAcquisition.IsPillarEnemy(NPCID.StardustWormBody));
        check("Soul Dust recognizes native undead and excludes living enemies", new[] { NPCID.Zombie, NPCID.Mummy, NPCID.Vampire, NPCID.Ghost, NPCID.AngryBones }.All(t => LegacyAcquisition.CanDrop(Drop("soul_dust"), Npc(t), player)) && !LegacyAcquisition.CanDrop(Drop("soul_dust"), Npc(NPCID.BlueSlime), player));
        NPC tagged = Npc(NPCID.BlueSlime);
        bool previouslyUndead = Compatibility.HasNpcTag(tagged, "undead");
        using (Compatibility.PreserveNpcTags())
        {
            Compatibility.Call(new object[] { "RegisterNpcTag", tagged.type, "undead" });
            check("Third-party undead tags feed the Soul Dust drop rule", LegacyAcquisition.CanDrop(Drop("soul_dust"), tagged, player));
        }
        check("Diagnostic NPC tagging restores the original registry", Compatibility.HasNpcTag(tagged, "undead") == previouslyUndead);
        enemy.SpawnedFromStatue = true;
        bool statue = LegacyAcquisition.CanDrop(Drop("soul_dust"), enemy, player);
        enemy.SpawnedFromStatue = false; enemy.friendly = true;
        check("Acquisition rules exclude statues and friendly entities", !statue && !LegacyAcquisition.CanDrop(Drop("soul_dust"), enemy, player));

        int hell = Main.UnderworldLayer + 1, surface = (int)Main.worldSurface - 20;
        bool ChestMatch(string pool, int style, int y, bool dungeon = false) => LegacyAcquisition.MatchesChest(pool, TileID.Containers, style, y, dungeon);
        check("Hell loot matches Shadow Chests only at Hell height", ChestMatch("hell_shadow", 3, hell) && ChestMatch("hell_shadow", 4, hell) && !ChestMatch("hell_shadow", 1, hell) && !ChestMatch("hell_shadow", 3, surface));
        check("Dungeon gold and general gold chest pools remain distinct", ChestMatch("dungeon_gold", 2, surface, true) && !ChestMatch("dungeon_gold", 2, surface) && ChestMatch("gold", 1, hell) && !ChestMatch("gold", 3, hell));
        check("Sky and Overworld chest pools respect location and container type", ChestMatch("sky", 13, surface) && !ChestMatch("sky", 13, hell) && !ChestMatch("overworld", 0, hell) && !LegacyAcquisition.MatchesChest("overworld", TileID.Dirt, 0, surface, false));
        int chestIndex = Array.FindIndex(Main.chest, c => c == null);
        var chest = new Chest { x = 190, y = surface };
        Tile tile = Main.tile[chest.x, chest.y]; tile.HasTile = true; tile.TileType = TileID.Containers; tile.TileFrameX = 36; tile.TileFrameY = 0; tile.WallType = 0;
        void EmptyChest() { for (int i = 0; i < Chest.maxItems; i++) chest.item[i] = new Item(); chest.item[0] = new Item(ItemID.CopperBar, 7); }
        int hearts = 0, stones = 0, trials = 10000; var rng = new UnifiedRandom(220029);
        for (int n = 0; n < trials; n++)
        {
            EmptyChest(); LegacyAcquisition.FillChest(chest, rng);
            if (chest.item.Any(i => i.type == ImportCatalog.Type("earth_heart"))) hearts++;
            if (chest.item.Any(i => i.type == ImportCatalog.Type("void_stone"))) stones++;
        }
        check("Seeded chest integration produces the configured Earth Heart and Void Stone rates", hearts > 650 && hearts < 950 && stones > 400 && stones < 600 && chest.item[0].type == ItemID.CopperBar && chest.item[0].stack == 7);
        for (int i = 0; i < Chest.maxItems; i++) chest.item[i] = new Item(ItemID.CopperBar, i + 1);
        LegacyAcquisition.FillChest(chest, new UnifiedRandom(1));
        check("Full chests preserve every original stack", chest.item.Select((i, slot) => i.type == ItemID.CopperBar && i.stack == slot + 1).All(v => v));
        EmptyChest(); Main.chest[chestIndex] = chest;
        var world = ModContent.GetInstance<AcquisitionWorld>(); var flag = new TagCompound { ["chestLootSeededV1"] = true };
        world.LoadWorldData(flag); world.SeedChests(); var savedWorld = new TagCompound(); world.SaveWorldData(savedWorld);
        check("Already seeded worlds never refill existing chests", savedWorld.GetBool("chestLootSeededV1") && chest.item.Skip(1).All(i => i.IsAir));
        Reset(); legacy.FirstChestReward = legacy.FirstMoonLordReward = false; player.chest = chestIndex;
        int eyes = Count("enigmatic_eye"), fruits = Count("astral_fruit");
        bool eye = LegacyAcquisition.TryFirstChest(player), fruit = LegacyAcquisition.TryFirstMoonLord(player);
        check("First chest and first Moon Lord grant exactly one personal reward", eye && fruit && !LegacyAcquisition.TryFirstChest(player) && !LegacyAcquisition.TryFirstMoonLord(player) && Count("enigmatic_eye") == eyes + 1 && Count("astral_fruit") == fruits + 1);
        var savedPlayer = new TagCompound(); legacy.SaveData(savedPlayer);
        legacy.FirstChestReward = legacy.FirstMoonLordReward = false; legacy.LoadData(savedPlayer);
        world.ClearWorld();
        check("Personal first rewards survive character save and changing worlds", legacy.FirstChestReward && legacy.FirstMoonLordReward && !LegacyAcquisition.TryFirstChest(player) && !LegacyAcquisition.TryFirstMoonLord(player));
        using (var stream = new MemoryStream())
        {
            legacy.FirstChestReward = legacy.FirstMoonLordReward = false;
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) legacy.WriteState(writer);
            legacy.FirstChestReward = legacy.FirstMoonLordReward = true; stream.Position = 0;
            using var reader = new BinaryReader(stream); legacy.ReadState(reader);
            check("Stale network states cannot reset once-per-character rewards", legacy.FirstChestReward && legacy.FirstMoonLordReward);
        }
        Player independent = new() { active = true, whoAmI = 253 };
        check("A separate character retains independent first rewards", !independent.GetModPlayer<LegacyPlayer>().FirstChestReward && !independent.GetModPlayer<LegacyPlayer>().FirstMoonLordReward);
        Main.chest[chestIndex] = null; player.chest = -1;

        Reset(); legacy.AteAstralFruit = false;
        var slot = ModContent.GetInstance<AstralRingSlot>();
        bool accepts = slot.CanAcceptItem(new Item(ItemID.CloudinaBottle), AccessorySlotType.FunctionalSlot) && !slot.CanAcceptItem(Imported("etherium_helmet"), AccessorySlotType.FunctionalSlot);
        LegacyActions.ExecuteUtility(player, (ImportedItem)Imported("astral_fruit").ModItem, player.Center, false);
        bool canRepeat = LegacyActions.CanUse(player, (ImportedItem)Imported("astral_fruit").ModItem);
        LegacyActions.ExecuteUtility(player, (ImportedItem)Imported("astral_fruit").ModItem, player.Center, false);
        check("Astral Fruit unlocks one universal slot while repeat fruit still buffs", accepts && legacy.AteAstralFruit && canRepeat && player.HasBuff(ModContent.BuffType<RegenerationIV>()) && player.HasBuff(ModContent.BuffType<StrengthIV>()));
        Reset(); legacy.AteForbiddenFruit = false;
        Item grail = Imported("unholy_grail"); LegacyGrail.Drink(player);
        check("Reusable Grail applies its original pre-Fruit debuffs and native substitutes", !grail.consumable && grail.ModItem.CanUseItem(player) && player.HasBuff(ModContent.BuffType<Withering>()) && player.HasBuff(ModContent.BuffType<StrongPoison>()) && player.HasBuff(BuffID.Confused) && player.HasBuff(BuffID.Weak));
        Reset(); legacy.AteForbiddenFruit = false; LegacyGrail.EatForbiddenFruit(player);
        var forbiddenSaved = new TagCompound(); legacy.SaveData(forbiddenSaved); legacy.AteForbiddenFruit = false; legacy.LoadData(forbiddenSaved);
        check("Forbidden Fruit persists qualification and applies original immediate non-hunger penalties", legacy.AteForbiddenFruit && !LegacyActions.CanUse(player, (ImportedItem)Imported("forbidden_fruit").ModItem) && player.HasBuff(ModContent.BuffType<WitheringIV>()) && player.HasBuff(ModContent.BuffType<WeaknessIV>()) && player.HasBuff(ModContent.BuffType<SlownessIII>()));
        Reset(); LegacyGrail.Drink(player);
        check("Fruit without Seven Curses retains negative Grail effects without hunger substitute", player.HasBuff(ModContent.BuffType<Withering>()) && !player.HasBuff(BuffID.Weak) && !player.HasBuff(ModContent.BuffType<RegenerationIII>()));
        Reset(); legacy.BoundCurses = true; LegacyGrail.Drink(player);
        check("Fruit and Seven Curses switch Grail to its five positive effects", player.buffTime[player.FindBuffIndex(ModContent.BuffType<RegenerationIII>())] == 1500 && player.HasBuff(ModContent.BuffType<AbsorptionII>()) && player.HasBuff(ModContent.BuffType<ResistanceII>()) && player.HasBuff(ModContent.BuffType<LegacyStrength>()) && player.HasBuff(ModContent.BuffType<SwiftnessI>()) && !player.HasBuff(ModContent.BuffType<Withering>()));
        details.Add($"Acquisition chest sample: {hearts}/{trials} Earth Hearts, {stones}/{trials} Void Stones; original chest contents preserved.");
        details.Add("Personal server packet delivery and client slot UI still require a live multiplayer/client acceptance test.");
        Main.netMode = oldMode;
    }
}
