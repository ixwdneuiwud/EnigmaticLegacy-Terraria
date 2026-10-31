// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace EnigmaticLegacy.Diagnostics;

internal static class SoulCrystalChecks
{
    public static void Run(Player player, Action<string, bool> check)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        var world = ModContent.GetInstance<LegacyWorld>();
        world.ClearWorld();
        foreach (Item item in player.inventory.Concat(player.armor)) item.TurnToAir();
        legacy.ResetEffects(); legacy.BoundCurses = true; legacy.LostSouls = 0;
        player.statLife = player.statLifeMax = player.statLifeMax2 = 500;
        player.Center = new Vector2(1600, 1600);
        legacy.Kill(999, 0, false, PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral("isolated soul fixture")));
        var crystal = Main.item.FirstOrDefault(i => i.active && i.ModItem is ImportedItem { ImportId: "soul_crystal" } soul && soul.Inscription == legacy.OwnerId);
        check("Soul: death creates a separate original-texture item at the death position", crystal != null && Vector2.DistanceSquared(crystal.Center, player.Center) < 1 && legacy.LostSouls == 1);
        legacy.PostUpdateEquips();
        check("Soul: one unreclaimed death removes ten percent of maximum life", player.statLifeMax2 == 450);
        if (crystal != null)
        {
            float gravity = .3f, fall = 10;
            crystal.ModItem.Update(ref gravity, ref fall);
            check("Soul: crystal floats without gravity and resists lava", gravity == 0 && crystal.velocity == Vector2.Zero && ItemID.Sets.ItemNoGravity[crystal.type] && ItemID.Sets.IsLavaImmuneRegardlessOfRarity[crystal.type]);
            check("Soul: native inventory pickup cannot bypass soul ownership", !crystal.ModItem.CanPickup(player));
            check("Soul: native soul rendering has a registered animation, including single-frame source textures", Main.itemAnimations[crystal.type] != null && Main.itemAnimations[crystal.type].FrameCount > 0);
            if (!Main.dedServ)
            {
                var renderer = typeof(Main).GetMethod("DrawItem_GetBasics", BindingFlags.Instance | BindingFlags.NonPublic);
                object[] args = { crystal, crystal.whoAmI, null, new Rectangle(), new Rectangle() };
                renderer.Invoke(Main.instance, args);
                var texture = (Texture2D)args[2]; var frame = (Rectangle)args[3];
                check("Soul: actual native world rendering resolves its texture and frame", texture != null && frame.Width > 0 && frame.Height > 0 && frame.Bottom <= texture.Height);
            }
        }
        TagCompound saved = new(); world.SaveWorldData(saved); world.LoadWorldData(saved);
        world.PostUpdateWorld(); world.PostUpdateWorld();
        check("Soul: world reload preserves a single persistent crystal identity", LegacyWorld.GraveCount == 1 && Main.item.Count(i => i.active && i.ModItem is ImportedItem { ImportId: "soul_crystal" } s && s.Inscription == legacy.OwnerId) == 1);
        var stranger = new Player { whoAmI = 252, active = true, statLife = 100, position = player.position };
        stranger.GetModPlayer<LegacyPlayer>().LostSouls = 3;
        LegacyWorld.ReclaimNearby(stranger);
        check("Soul: another character cannot reclaim the crystal", LegacyWorld.GraveCount == 1 && legacy.LostSouls == 1 && stranger.GetModPlayer<LegacyPlayer>().LostSouls == 3);
        player.dead = true; LegacyWorld.ReclaimNearby(player); player.dead = false;
        check("Soul: a dead character cannot reclaim before respawning", legacy.LostSouls == 1 && LegacyWorld.GraveCount == 1);
        for (int n = 0; n < player.inventory.Length; n++) player.inventory[n] = new Item(ItemID.StoneBlock, 9999);
        LegacyWorld.ReclaimNearby(player);
        player.statLifeMax2 = 500; legacy.ResetEffects(); legacy.BoundCurses = true; legacy.PostUpdateEquips();
        check("Soul: approaching with a full inventory restores max life and consumes the crystal", legacy.LostSouls == 0 && player.statLifeMax2 == 500 && LegacyWorld.GraveCount == 0 && (crystal == null || !crystal.active || crystal.IsAir));
        LegacyWorld.ReclaimNearby(player);
        check("Soul: repeated pickup cannot add extra maximum life", legacy.LostSouls == 0 && LegacyWorld.GraveCount == 0 && player.statLifeMax2 == 500);
        legacy.LostSouls = 0;
        foreach (Item item in player.inventory) item.TurnToAir();
    }
}
