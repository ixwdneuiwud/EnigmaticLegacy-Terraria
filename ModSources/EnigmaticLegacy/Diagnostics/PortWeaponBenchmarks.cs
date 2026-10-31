// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Diagnostics;

// Reproducible native projectile simulation, only called in the isolated test profile.
// No armor/accessories, infinite mana for the vanilla comparison, 4 s warm-up,
// then 10 s against one stationary 120 x 160 boss with 50 defense, 360 px away.
internal static class PortWeaponBenchmarks
{
    public static readonly Dictionary<string, double> Results = new();
    public static void Run(Player player, Action<string, bool> check)
    {
        Results.Clear();
        var shoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.Instance | BindingFlags.NonPublic);
        var clock = typeof(Main).GetField("_gameUpdateCount", BindingFlags.Static | BindingFlags.NonPublic);
        uint oldClock = (uint)Main.GameUpdateCount;
        Vector2 oldPosition = player.position;
        int oldMouseX = Main.mouseX, oldMouseY = Main.mouseY, oldMode = Main.netMode;
        Vector2 oldScreen = Main.screenPosition;
        var oldLight = Lighting.Mode;
        var oldRandom = Main.rand;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        int slot = Main.maxNPCs - 1; NPC saved = Main.npc[slot];
        var npcStates = Main.npc.Select(n => n.active).ToArray();
        foreach (var npc in Main.npc) npc.active = false;
        try
        {
            // Native beam AI skips client-only screen shaders in server mode.
            // Keep the artificial owner as Main.myPlayer for native hit authority.
            Main.netMode = NetmodeID.Server;
            // Vanilla Zenith adds light even on a dedicated server; select its
            // CPU lighting engine without creating any graphics device.
            Lighting.Mode = Terraria.Graphics.Light.LightMode.Color;
            player.position = new Vector2(Main.spawnTileX * 16, 80 * 16);
            for (int x = Main.spawnTileX - 30; x <= Main.spawnTileX + 60; x++)
                for (int y = 50; y <= 110; y++) Main.tile[x, y].ClearEverything();
            var testItems = new Dictionary<string, Item> {
                ["Zenith"] = new(ItemID.Zenith), ["LastPrism"] = new(ItemID.LastPrism),
                ["Meowmere"] = new(ItemID.Meowmere),
                ["StarWrath"] = new(ItemID.StarWrath),
                ["the_twist"] = new(ImportCatalog.Type("the_twist")),
                ["the_infinitum"] = new(ImportCatalog.Type("the_infinitum")),
                ["eldritch_pan_0"] = new(ImportCatalog.Type("eldritch_pan")),
                ["eldritch_pan_100"] = new(ImportCatalog.Type("eldritch_pan"))
            };
            uint elapsed = 0;
            foreach (var pair in testItems)
            {
                Main.rand = new Terraria.Utilities.UnifiedRandom(230016);
                foreach (var p in Main.projectile) if (p.owner == player.whoAmI) p.active = false;
                player.ResetEffects(); legacy.ResetEffects();
                var weapon = pair.Value;
                if (pair.Key.EndsWith("_100"))
                    for (int n = 0; n < 100; n++) weapon.GetGlobalItem<ItemMetadata>().UniqueKills.Add("Benchmark/" + n);
                legacy.BoundCurses = weapon.ModItem is ImportedItem;
                legacy.PlayTicks = legacy.CursedTicks = 1000;
                player.inventory[0] = weapon; player.selectedItem = 0; player.altFunctionUse = 0;
                player.direction = 1; player.velocity = Vector2.Zero; player.gravDir = 1; player.dead = false;
                player.channel = weapon.channel; player.controlUseItem = true;
                var target = new NPC(); target.SetDefaults(NPCID.BlueSlime); target.whoAmI = slot; target.active = true;
                target.width = 120; target.height = 160; target.Center = player.Center + new Vector2(360, 0);
                target.boss = true; target.lifeMax = target.life = 100000000; target.defense = 50;
                Main.npc[slot] = target;
                Main.screenPosition = player.Center - new Vector2(500, 300);
                Main.mouseX = (int)(target.Center.X - Main.screenPosition.X);
                Main.mouseY = (int)(target.Center.Y - Main.screenPosition.Y);
                int startedAt = target.life;
                for (int tick = 0; tick < 840; tick++)
                {
                    clock.SetValue(null, oldClock + elapsed + (uint)tick + 1);
                    player.statMana = player.statManaMax2 = 2000;
                    player.itemAnimationMax = weapon.useAnimation;
                    player.itemAnimation = weapon.channel ? 2 : weapon.useAnimation - tick % weapon.useAnimation;
                    player.itemTime = weapon.useTime;
                    for (int n = 0; n < target.immune.Length; n++) if (target.immune[n] > 0) target.immune[n]--;
                    Array.Clear(player.ownedProjectileCounts);
                    foreach (Projectile p in Main.projectile)
                        if (p.active && p.owner == player.whoAmI) player.ownedProjectileCounts[p.type]++;
                    if ((weapon.channel ? tick == 0 : tick % weapon.useTime == 0) &&
                        (weapon.ModItem?.CanUseItem(player) ?? true))
                        shoot.Invoke(player, new object[] { player.whoAmI, weapon, player.GetWeaponDamage(weapon) });
                    for (int p = 0; p < Main.maxProjectiles; p++)
                        if (Main.projectile[p].active && Main.projectile[p].owner == player.whoAmI) Main.projectile[p].Update(p);
                    if (tick == 239) startedAt = target.life;
                }
                Results[pair.Key] = (startedAt - target.life) / 10d;
                elapsed += 840;
            }
            check("Native weapon benchmark emits damage for every compared weapon", Results.Values.All(d => d > 0));
        }
        finally
        {
            foreach (var p in Main.projectile) if (p.owner == player.whoAmI) p.active = false;
            Main.npc[slot] = saved;
            for (int n = 0; n < Main.maxNPCs; n++) Main.npc[n].active = npcStates[n];
            clock.SetValue(null, oldClock);
            player.position = oldPosition; player.channel = player.controlUseItem = false;
            player.itemAnimation = player.itemTime = 0; Main.mouseX = oldMouseX; Main.mouseY = oldMouseY;
            Main.screenPosition = oldScreen; Main.netMode = oldMode;
            Lighting.Mode = oldLight;
            Main.rand = oldRandom;
            player.ResetEffects(); legacy.ResetEffects(); legacy.BoundCurses = false; player.inventory[0].TurnToAir();
        }
    }
}
