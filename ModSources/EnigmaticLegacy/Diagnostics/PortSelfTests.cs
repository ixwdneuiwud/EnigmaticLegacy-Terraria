// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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

// Explicitly opt-in, isolated-profile integration checks. Never run in a normal game.
public sealed class PortSelfTests : ModSystem
{
    private bool ran;
    public override void OnWorldLoad()
    {
        if (ran || !Main.dedServ || !Environment.GetCommandLineArgs().Contains("-elselftest") ||
            !Path.GetFullPath(Main.SavePath).Contains("PortBuildProfile", StringComparison.OrdinalIgnoreCase)) return;
        ran = true;
        bool passed = true;
        bool registrationOnly = Environment.GetCommandLineArgs().Contains("-elregistrationtest");
        var report = Run((_, ok) => passed &= ok, registrationOnly);
        File.WriteAllText(Path.Combine(Main.SavePath, "elselftest.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"SELFTEST RESULT: {(passed ? "PASS" : "FAIL")}");
        Environment.Exit(passed ? 0 : 1);
    }
    public static object Run(Action<string, bool> externalCheck, bool registrationOnly = false)
    {
        if (!Main.dedServ && !DiagnosticAccess.Allowed(Main.LocalPlayer))
            throw new InvalidOperationException("Diagnostics require a disposable RelicValidation character and world");
        using var clientFixture = !Main.dedServ && !registrationOnly ? new ClientFixtureScope() : null;
        // Only the isolated headless fixture needs neutral keybind state. Client
        // diagnostics retain the real client initialization and installed hooks.
        if (Main.dedServ && !registrationOnly)
        {
            Terraria.GameInput.PlayerInput.Triggers.Initialize();
            // The isolated server has no fonts. Occupied entries let actual heal
            // and damage events skip client-only combat text for the whole run.
            for (int i = 0; i < Main.combatText.Length; i++) Main.combatText[i] = new CombatText { active = true };
        }
        var checks = new Dictionary<string, bool>();
        var details = new List<string>();
        void Check(string name, bool ok) { checks[name] = ok; externalCheck(name, ok); ImportCatalog.Owner.Logger.Info($"SELFTEST {(ok ? "PASS" : "FAIL")}: {name}"); }
        int oldMode = Main.netMode, oldPlayer = Main.myPlayer;
        const int index = 254;
        Player previous = Main.player[index];
        if (registrationOnly) RegistrationChecks(Check);
        else try
        {
            Main.netMode = NetmodeID.SinglePlayer; Main.myPlayer = index;
            var player = new Player { whoAmI = index, name = "EL isolated diagnostics", active = true,
                position = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16), statLife = 400, statLifeMax = 400, statLifeMax2 = 400 };
            Main.player[index] = player;
            player.ResetEffects();
            var legacy = player.GetModPlayer<LegacyPlayer>();
            Check("All imported IDs and textures resolve", ImportCatalog.Items.Values.All(d => ImportCatalog.TryType(d.Id, out _) && (d.Texture.StartsWith("Terraria/", StringComparison.Ordinal) || ImportCatalog.Owner.FileExists(d.Texture + ".png") || ImportCatalog.Owner.FileExists(d.Texture + ".rawimg"))));
            Check("182 stable item variants including five original non-redstone blocks", ImportCatalog.Items.Count == 182);
            var weapons = ImportCatalog.Items.Values.Where(d => d.Kind == "weapon").Select(d => new Item(ImportCatalog.Type(d.Id))).ToArray();
            Check("Only two base weapon attacks remain deferred", weapons.Count(i => !LegacyWeapons.Implemented(((ImportedItem)i.ModItem).ImportId)) == 2 &&
                weapons.Where(i => !LegacyWeapons.Implemented(((ImportedItem)i.ModItem).ImportId)).All(i => i.noMelee && i.shoot == 0 && i.useStyle == 0 && !i.ModItem.CanUseItem(player)));
            Check("Seven amulet colors have different source textures", ImportCatalog.Items.Values.Where(d => d.Id == "enigmatic_amulet" || d.Id.StartsWith("enigmatic_amulet_")).Select(d => d.Texture).Distinct().Count() == 7);
            Check("Weapons, armor and accessories can be reforged", ImportCatalog.Items.Values.Where(d => d.Kind is "weapon" or "armor" or "boots" or "accessory" or "spellstone").All(d => new Item(ImportCatalog.Type(d.Id)).Prefix(-3)));
            var wardedArmor = new Item(ImportCatalog.Type("etherium_helmet"));
            Check("Armor accepts native Warding prefix", wardedArmor.Prefix(PrefixID.Warding) && wardedArmor.prefix == PrefixID.Warding);
            Check("Armor cannot be placed in accessory slots", !wardedArmor.ModItem.CanEquipAccessory(player, 3, false));
            player.armor[0] = wardedArmor;
            player.ResetEffects();
            typeof(Player).GetMethod("UpdateEquips", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(player, new object[] { index });
            details.Add($"Warding armor defense after native UpdateEquips: {player.statDefense}");
            Check("Warding armor applies native prefix exactly once", player.statDefense == BridgeUnits.Armor(4) + 4);
            player.armor[0] = new Item(); player.ResetEffects(); legacy.ResetEffects();
            var brokenWeapon = new Item(ImportCatalog.Type("ender_slayer"));
            Check("Deferred weapon accepts native Broken prefix", brokenWeapon.Prefix(PrefixID.Broken) && brokenWeapon.prefix == PrefixID.Broken && !brokenWeapon.ModItem.CanUseItem(player));
            Check("Only overall negative prefixes count", LegacyPrefixes.IsNegative(PrefixID.Broken) && !LegacyPrefixes.IsNegative(PrefixID.Ruthless) && !LegacyPrefixes.IsNegative(PrefixID.Warding));
            var deferred = new[] { "redemption_potion", "astral_potato", "xp_scroll", "enchanter_pearl", "enchantment_transposer", "curse_transposer", "tattered_tome", "withered_tome", "corrupted_tome" };
            Check("No use or consumption of deferred mechanics", deferred.All(id => !LegacyActions.CanUse(player, (ImportedItem)new Item(ImportCatalog.Type(id)).ModItem)));
            Check("Mapped recipes registered", ImportedRecipes.RegisteredCount > 20);
            Check("Every nonempty potion variant has a native brewing recipe", ImportCatalog.Items.Values.Where(d => d.Variant != "" && (d.Id.StartsWith("ultimate_") || d.Id.StartsWith("common_"))).All(d => ImportedRecipes.RegisteredSources.ContainsValue(d.Id)));
            ((ImportedItem)new Item(ImportCatalog.Type("cursed_scroll")).ModItem).UpdateEquip(player);
            Check("Generic equipment hook cannot bypass curse requirement", !legacy.Has("cursed_scroll"));
            var cloneSource = new Item(ImportCatalog.Type("void_stone"));
            cloneSource.GetGlobalItem<ItemMetadata>().VoidFilters.Add("Terraria/2");
            var clone = cloneSource.Clone(); clone.GetGlobalItem<ItemMetadata>().VoidFilters.Add("Terraria/3");
            Check("Item state clones independently", cloneSource.GetGlobalItem<ItemMetadata>().VoidFilters.Count == 1);
            var saved = ItemIO.Load(ItemIO.Save(cloneSource));
            Check("Item state survives save/load", saved.GetGlobalItem<ItemMetadata>().VoidFilters.SetEquals(new[] { "Terraria/2" }));
            legacy.PlayTicks = 1000; legacy.CursedTicks = 1000; legacy.BoundCurses = true;
            TagCompound state = new(); legacy.SaveData(state); legacy.BoundCurses = false; legacy.LoadData(state);
            Check("Bound curse and worthiness survive save/load", legacy.BoundCurses && legacy.Worthy);
            legacy.BoundCurses = false;
            legacy.ResetEffects(); player.statDefense += 100;
            ((ImportedItem)new Item(ImportCatalog.Type("iron_ring")).ModItem).UpdateAccessory(player, false);
            legacy.PostUpdateEquips();
            Check("Defense adds to other equipment", player.statDefense == 100 + BridgeUnits.Armor(1));
            player.ResetEffects(); legacy.ResetEffects(); legacy.BoundCurses = true;
            player.selectedItem = 0; player.inventory[0] = brokenWeapon;
            var spare = new Item(ItemID.IronBroadsword); spare.Prefix(PrefixID.Broken); player.inventory[1] = spare;
            player.armor[10] = spare.Clone();
            ((ImportedItem)new Item(ImportCatalog.Type("cursed_scroll")).ModItem).UpdateAccessory(player, false);
            legacy.PostUpdateEquips();
            Check("Spare and vanity prefixes excluded from curse count", LegacyPrefixes.CurseCount(player) == 1);
            Check("Doubled scroll counts seven Ring curses and one negative prefix", LegacyPrefixes.TotalCurses(player) == 8 && Math.Abs(player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) - 164) < 0.01f && Math.Abs(player.pickSpeed - 1 / 2.12f) < 0.001f);
            player.lifeRegen = 100; legacy.UpdateLifeRegen();
            Check("Doubled scroll regeneration includes all eight active curses", player.lifeRegen == 164);
            legacy.BoundCurses = false; player.armor[10].TurnToAir();
            legacy.ResetEffects();
            var pearl = (ImportedItem)new Item(ImportCatalog.Type("void_pearl")).ModItem;
            pearl.UpdateAccessory(player, false); legacy.PostUpdateEquips();
            Check("Technical debuffs remain available", !player.buffImmune[BuffID.PotionSickness] && !player.buffImmune[BuffID.MoonLeech]);
            legacy.ResetEffects();
            foreach (var item in player.inventory) item.TurnToAir();
            player.inventory[10] = new Item(ImportCatalog.Type("enigmatic_amulet"));
            player.inventory[11] = new Item(ItemID.CopperBar, 7);
            var deathItem = new Item(ImportCatalog.Type("void_stone"));
            deathItem.GetGlobalItem<ItemMetadata>().VoidFilters.Add("Terraria/123");
            var deathArmor = new Item(ImportCatalog.Type("etherium_helmet")); deathArmor.Prefix(PrefixID.Warding);
            player.inventory[12] = deathItem; player.armor[0] = deathArmor;
            legacy.Carry("enigmatic_amulet");
            player.difficulty = 1;
            // DropItems does not invoke KillMe or character autosave. A real
            // client has no initialized server sockets, so retain its mode.
            Main.netMode = Main.dedServ ? NetmodeID.Server : NetmodeID.SinglePlayer;
            var drop = typeof(Player).GetMethod("DropItems", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (drop == null) throw new InvalidOperationException("Native Player.DropItems routine was not found");
            drop.Invoke(player, null);
            Check("Native mediumcore drops enter a vessel", LegacyWorld.GraveCount > 0);
            var world = ModContent.GetInstance<LegacyWorld>();
            TagCompound worldState = new(); world.SaveWorldData(worldState);
            var graves = worldState.GetList<TagCompound>("legacyGraves").Select(LegacyGrave.Load).ToList();
            Item[] contents = graves.SelectMany(g => g.Items).ToArray();
            Check("Native death drops preserve exact stacks and remove originals", contents.Where(i => i.type == ItemID.CopperBar).Sum(i => i.stack) == 7 && player.inventory.All(i => i.type != ItemID.CopperBar));
            Check("Native death drops preserve armor prefixes and custom item data", contents.Any(i => i.type == ImportCatalog.Type("etherium_helmet") && i.prefix == PrefixID.Warding) && contents.Any(i => i.type == ImportCatalog.Type("void_stone") && i.GetGlobalItem<ItemMetadata>().VoidFilters.Contains("Terraria/123")));
            using (var stream = new MemoryStream())
            {
                TagIO.ToStream(worldState, stream, false); stream.Position = 0;
                world.LoadWorldData(TagIO.FromStream(stream, false));
            }
            TagCompound reloaded = new(); world.SaveWorldData(reloaded);
            Check("Vessels survive binary world save/load", reloaded.GetList<TagCompound>("legacyGraves").Select(LegacyGrave.Load).SelectMany(g => g.Items).Sum(i => i.stack) == contents.Sum(i => i.stack));
            player.statLife = 400;
            LegacyWorld.ReclaimNearby(player);
            int CopperCount() => Main.item.Where(i => i.active && i.type == ItemID.CopperBar).Sum(i => i.stack) + player.inventory.Where(i => i.type == ItemID.CopperBar).Sum(i => i.stack);
            int returned = CopperCount(); LegacyWorld.ReclaimNearby(player);
            Check("Vessel return is exact and cannot be repeated", LegacyWorld.GraveCount == 0 && returned == 7 && CopperCount() == 7);
            legacy.LostSouls = 2; LegacyWorld.AddSoul(player, player.Center); LegacyWorld.ReclaimNearby(player);
            Check("Reclaiming a soul restores one lost-health layer", legacy.LostSouls == 1 && LegacyWorld.GraveCount == 0);
            Main.netMode = NetmodeID.SinglePlayer;
            player.ResetEffects(); legacy.ResetEffects();
            Array.Clear(player.buffImmune); // The earlier Void Pearl test populated native immunity flags.
            player.ClearBuff(ModContent.BuffType<Withering>()); player.AddBuff(ModContent.BuffType<Withering>(), 100);
            player.lifeRegen = 100; legacy.UpdateBadLifeRegen();
            Check("Withering harms players without scaling positive regeneration", player.lifeRegen == -BridgeUnits.Health(2) * 2);
            player.ClearBuff(ModContent.BuffType<Withering>()); player.AddBuff(ModContent.BuffType<StrongPoison>(), 36);
            player.statLife = 2; int poisonIndex = player.FindBuffIndex(ModContent.BuffType<StrongPoison>());
            Check("Strong poison can be applied after immunity is removed", poisonIndex >= 0);
            if (poisonIndex >= 0) ModContent.GetInstance<StrongPoison>().Update(player, ref poisonIndex);
            Check("Strong poison cannot kill the player", player.statLife == 1);
            player.ClearBuff(ModContent.BuffType<StrongPoison>()); player.statLife = 400;
            PortBehaviorChecks.Run(player, Check, details);
            SoulCrystalChecks.Run(player, Check);
            PortEffectChecks.Run(player, Check, details);
            PortStatusChecks.Run(player, Check, details);
            PortAdaptationChecks.Run(player, Check, details);
            PortPotionChecks.Run(player, Check, details);
            KnownGapChecks.Run(player, Check);
            PortAcquisitionChecks.Run(player, Check, details);
            PortWeaponChecks.Run(player, Check, details);
            if (Main.dedServ) PortWeaponBenchmarks.Run(player, Check);
            details.Add("Vanilla Player.DropItems exercised with a temporary player; client mode uses only the explicitly authorized disposable test world.");
            if (Environment.GetCommandLineArgs().Contains("-elrequirecalamity"))
                Check("Required Calamity actually loaded", ModLoader.HasMod("CalamityMod"));
            if (ModLoader.TryGetMod("CalamityMod", out var calamity))
            {
                Check("Calamity optional bridge initialized", Compatibility.CalamityLoaded);
                if (calamity.TryFind<ModNPC>("AquaticScourgeHead", out var aquatic))
                {
                    var npc = new NPC(); npc.SetDefaults(aquatic.Type);
                    Check("Calamity aquatic boss recognized", Compatibility.IsAquatic(npc) && BridgeUnits.IsBoss(npc));
                }
                if (calamity.TryFind<ModBuff>("BossEffects", out var bossBuff))
                    Check("Calamity boss control buff protected", Compatibility.ProtectedBuff(bossBuff.Type));
            }
        }
        catch (Exception ex) { Check("Unhandled integration exception", false); details.Add(ex.ToString()); ImportCatalog.Owner.Logger.Error("SELFTEST EXCEPTION", ex); }
        finally { Main.netMode = oldMode; Main.myPlayer = oldPlayer; Main.player[index] = previous; }
        object expansionDiagnostics = null;
        if (ModLoader.TryGetMod("RelicExpansions", out var expansions))
            try { expansionDiagnostics = expansions.Call("RunPortDiagnostics", (Action<string, bool>)Check, registrationOnly); }
            catch (Exception ex) { Check("Expansion diagnostics dispatch", false); details.Add(ex.ToString()); }
        string SourceId(Item item) => item.ModItem is ImportedItem imported ? imported.ImportId : (item.ModItem as MaterialBlockItem)?.SourceId;
        var nativeRecipes = Main.recipe.Take(Recipe.numRecipes).Where(r => r.createItem.ModItem is ImportedItem or MaterialBlockItem).Select(r => new {
            result = SourceId(r.createItem), count = r.createItem.stack,
            ingredients = r.requiredItem.Where(i => !i.IsAir).Select(i => new { id = ItemMetadata.ItemKey(i), importId = SourceId(i), count = i.stack }).ToArray(),
            stations = r.requiredTile.ToArray(),
            conditions = r.Conditions.Select(c => c.Description.Key).ToArray(),
            groups = r.acceptedGroups.Select(g => RecipeGroup.recipeGroups[g].ValidItems.Select(t => SourceId(new Item(t)) ?? ItemID.Search.GetName(t)).ToArray()).ToArray() }).ToArray();
        var weaponStats = ImportCatalog.Items.Values.Where(d => d.Kind == "weapon" && LegacyWeapons.Implemented(d.Id)).Select(d => {
            var i = new Item(ImportCatalog.Type(d.Id));
            return new { id = d.Id, i.damage, i.useTime, i.useAnimation, i.knockBack, i.pick, axePercent = i.axe * 5, i.shootSpeed, i.noMelee, i.tileBoost, projectile = i.shoot };
        }).ToArray();
        var report = new { modVersion = ImportCatalog.Owner.Version.ToString(), validationMode = registrationOnly ? "registration-only" : Main.dedServ ? "headless-behavior" : "client-behavior", checks, details, expansionDiagnostics, recipes = ImportedRecipes.RegisteredCount, registeredRecipes = ImportedRecipes.RegisteredSources, deferredRecipes = ImportedRecipes.Deferred, nativeRecipes, weaponStats, weaponBenchmarks = PortWeaponBenchmarks.Results,
            loadedMods = ModLoader.Mods.Select(m => new { m.Name, version = m.Version.ToString() }).ToArray(),
            calamity = Compatibility.CalamityLoaded, tModLoader = typeof(Mod).Assembly.GetName().Version.ToString() };
        return report;
    }
    private static void RegistrationChecks(Action<string, bool> check)
    {
        check("Registration: 182 base item variants and source textures resolve", ImportCatalog.Items.Count == 182 && ImportCatalog.Items.Values.All(d => ImportCatalog.TryType(d.Id, out _) && (d.Texture.StartsWith("Terraria/", StringComparison.Ordinal) || ImportCatalog.Owner.FileExists(d.Texture + ".png") || ImportCatalog.Owner.FileExists(d.Texture + ".rawimg"))));
        check("Registration: native and hidden base recipes register", ImportedRecipes.RegisteredCount > 100 && ImportedRecipes.Deferred.Count == 0);
        check("Registration: base equipment accepts native prefixes", ImportCatalog.Items.Values.Where(d => d.Kind is "weapon" or "armor" or "boots" or "accessory" or "spellstone").All(d => new Item(ImportCatalog.Type(d.Id)).Prefix(-3)));
        check("Registration: optional Calamity bridge matches the loaded mods", Compatibility.CalamityLoaded == ModLoader.HasMod("CalamityMod"));
        if (Environment.GetCommandLineArgs().Contains("-elrequirecalamity"))
            check("Registration: explicitly required Calamity is loaded", ModLoader.HasMod("CalamityMod"));
    }
}
