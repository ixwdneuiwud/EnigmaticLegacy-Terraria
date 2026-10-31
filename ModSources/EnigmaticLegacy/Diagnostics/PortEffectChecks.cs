// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EnigmaticLegacy.Diagnostics;

internal static class PortEffectChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        void Reset(params string[] ids)
        {
            foreach (var item in player.inventory.Concat(player.armor)) item.TurnToAir();
            Array.Clear(player.buffType); Array.Clear(player.buffTime); Array.Clear(player.buffImmune);
            player.ResetEffects(); legacy.ResetEffects();
            legacy.BoundCurses = false; player.lavaWet = player.wet = false;
            player.statLife = player.statLifeMax2 = 400;
            foreach (string id in ids) legacy.Equip((ImportedItem)new Item(ImportCatalog.Type(id)).ModItem);
        }
        Reset("golden_ring"); legacy.PostUpdateEquips();
        check("Golden ring does not grant unrelated coin pickup range", !player.goldRing);
        Reset("angel_blessing"); float jump = player.jumpSpeedBoost; legacy.PostUpdateEquips();
        check("Angel blessing does not add an undocumented jump boost", player.noFallDmg && player.jumpSpeedBoost == jump);
        Reset("eldritch_amulet"); float gravity = player.gravity; float speed = player.moveSpeed;
        legacy.PostUpdateEquips();
        check("Eldritch amulet retains its own attack bonus without extra color powers", player.GetDamage(DamageClass.Melee).Base == BridgeUnits.Health(3) && player.gravity == gravity && player.moveSpeed == speed && !legacy.HasAmuletColor("violet") && !legacy.HasAmuletColor("green"));
        Reset("etherium_helmet", "etherium_boots"); legacy.PostUpdateEquips();
        check("Etherium armor does not grant unrelated night vision or fall immunity", !player.nightVision && !player.noFallDmg);
        Reset("blazing_core");
        int molten = ModContent.BuffType<MoltenHeart>(); player.AddBuff(molten, 100);
        // Native buff ticking removes one tick; core restores one on every other tick.
        int restored = 0;
        for (int i = 0; i < 6; i++)
        {
            int index = player.FindBuffIndex(molten); int old = player.buffTime[index];
            legacy.PostUpdateEquips(); restored += player.buffTime[index] - old;
        }
        check("Core extends existing Molten Heart by slowing its timer", restored == 3);
        Reset("blazing_core"); player.lavaWet = true;
        bool protectedPeriod = true;
        for (int i = 0; i < 900; i++) { player.lavaImmune = false; legacy.PostUpdateEquips(); protectedPeriod &= player.lavaImmune; }
        player.lavaImmune = false; legacy.PostUpdateEquips();
        check("Core heat reaches its limit after 15 seconds of unprotected lava exposure", protectedPeriod && !player.lavaImmune);
        player.lavaWet = false;
        for (int i = 0; i < 480; i++) { player.lavaImmune = false; legacy.PostUpdateEquips(); }
        player.lavaWet = true; player.lavaImmune = false; legacy.PostUpdateEquips();
        check("Core heat recovers while out of lava", player.lavaImmune);
        Reset("blazing_core");
        LegacyPotions.Apply(player, ImportCatalog.Get("common_molten_heart"));
        check("Core does not multiply the initial Molten Heart duration twice", player.buffTime[player.FindBuffIndex(molten)] == LegacyPotions.Effect(ImportCatalog.Get("common_molten_heart")).Ticks);
        Reset("the_cube");
        check("Cube does not cancel excessive environmental damage", !legacy.FreeDodge(new Player.HurtInfo { Damage = BridgeUnits.Health(200), DamageSource = PlayerDeathReason.ByOther(0) }));
        var npc = Main.npc[198]; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = 198; npc.active = true;
        check("Cube cancels excessive damage from a creature", legacy.FreeDodge(new Player.HurtInfo { Damage = BridgeUnits.Health(200), DamageSource = PlayerDeathReason.ByNPC(198) }));
        npc.active = false;
        Reset("eye_of_nebula");
        bool environmentalDodge = false;
        for (int i = 0; i < 80; i++) environmentalDodge |= legacy.FreeDodge(new Player.HurtInfo { Damage = 10, DamageSource = PlayerDeathReason.ByOther(0) });
        check("Nebula eye does not dodge environmental damage", !environmentalDodge);
        int x = 100, y = (int)Main.rockLayer + 60;
        for (int tx = x - 35; tx <= x + 35; tx++)
            for (int ty = y - 35; ty <= y + 35; ty++)
            {
                Tile tile = Main.tile[tx, ty]; tile.HasTile = false; tile.WallType = WallID.DirtUnsafe; tile.LiquidAmount = 0;
            }
        var light = new LegacyLight(); Vector2 testPoint = new Vector2(x * 16 + 8, y * 16 + 8);
        light.Scan(testPoint, 16); float dark = light.Brightness(testPoint);
        Tile torchTile = Main.tile[x + 2, y]; torchTile.HasTile = true; torchTile.TileType = TileID.Torches;
        torchTile.TileFrameX = torchTile.TileFrameY = 0;
        light.Scan(testPoint, 16); float torch = light.Brightness(testPoint);
        check("Darkness aura recognizes torch-lit caves on dedicated servers", dark < 0.2f && torch >= 0.2f);
        details.Add($"Native gameplay light: unlit cave {dark:F3}, nearby torch {torch:F3}.");
        torchTile.HasTile = false;
        Reset("void_pearl"); player.Center = testPoint;
        npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = 198; npc.active = true;
        npc.Center = testPoint; npc.life = npc.lifeMax = 1000;
        npc.buffImmune[ModContent.BuffType<LesserWithering>()] = false;
        npc.buffImmune[ModContent.BuffType<SlownessIII>()] = false;
        legacy.ApplyDarknessAura();
        details.Add($"Darkness aura target: {npc.life} life, buffs {string.Join(',', npc.buffType.Where(type => type > 0))}.");
        check("Darkness aura applies source Slowness III and leaves Hunger empty", npc.HasBuff(ModContent.BuffType<LesserWithering>()) && npc.HasBuff(ModContent.BuffType<SlownessIII>()) &&
            npc.buffType.Where(type => type > 0).All(type => type == ModContent.BuffType<LesserWithering>() || type == ModContent.BuffType<Withering>() || type == ModContent.BuffType<SlownessIII>()));
        npc.active = false;
        Reset();
        details.Add("Effects compared against Enigmatic Legacy 2.30.0 item classes and EnigmaticEventHandler; environment and creature damage tested separately.");
    }
}
