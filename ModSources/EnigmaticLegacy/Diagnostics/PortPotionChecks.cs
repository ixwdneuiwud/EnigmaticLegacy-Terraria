// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using EnigmaticLegacy.Content.Projectiles;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortPotionChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        int oldMode = Main.netMode; Main.netMode = NetmodeID.SinglePlayer;
        Vector2 position = player.Center;
        int target = NPC.NewNPC(new EntitySource_Misc("RelicPotionCheck"), 0, 0, NPCID.BlueSlime);
        if (target >= Main.maxNPCs) { check("Potion fixture has a target slot", false); return; }
        NPC npc = Main.npc[target];
        var effect = LegacyPotions.Effect(ImportCatalog.Get("ultimate_strength_splash"));
        Vector2 center = new(2400, (float)Main.worldSurface * 16);
        Projectile Shot(bool cloud = false, string potion = "ultimate_strength")
        {
            var shot = new Projectile(); shot.SetDefaults(ModContent.ProjectileType<LegacyPotionProjectile>());
            shot.active = true; shot.owner = player.whoAmI; shot.Center = center;
            shot.ai[0] = ImportCatalog.Type(potion + (cloud ? "_lingering" : "_splash")); shot.ai[1] = cloud ? 1 : 0;
            return shot;
        }
        void Target(float x, float y = 0)
        {
            npc.width = npc.height = 2; npc.Center = center + new Vector2(x, y);
            npc.active = true; npc.immortal = npc.dontTakeDamage = false;
            Array.Clear(npc.buffType); Array.Clear(npc.buffTime); Array.Clear(npc.buffImmune);
        }
        void Tick(Projectile shot, int count)
        { for (int i = 0; i < count && shot.active; i++) { shot.ModProjectile.AI(); shot.timeLeft--; } }
        int BuffTime() => npc.FindBuffIndex(effect.Buff) is int i && i >= 0 ? npc.buffTime[i] : 0;
        try
        {
            player.Center = center + new Vector2(200, 0);
            Target(32); Shot().ModProjectile.OnKill(0);
            check("Splash halfway from impact receives half duration", BuffTime() == effect.Ticks / 2);
            Target(0, 40); Shot().ModProjectile.OnKill(0);
            check("Splash honors the source's two-block vertical search bound", BuffTime() == 0);
            Target(12); npc.width = npc.height = 32; npc.Center = center + new Vector2(12, 0);
            var direct = Shot(); direct.timeLeft = 170; direct.ModProjectile.AI();
            check("Thrown potion collides with an NPC and grants full direct-hit duration", !direct.active && BuffTime() == effect.Ticks);
            Target(0); var cloud = Shot(true); cloud.ModProjectile.OnTileCollide(Vector2.Zero);
            check("Lingering cloud waits ten source ticks before applying", BuffTime() == 0);
            Tick(cloud, 31);
            check("Lingering cloud applies one quarter duration after the wait", BuffTime() == effect.Ticks / 4);
            Array.Clear(npc.buffType); Array.Clear(npc.buffTime); Tick(cloud, 30);
            check("Lingering cloud does not reapply within its target cooldown", BuffTime() == 0);
            Tick(cloud, 30);
            check("Lingering cloud reapplies after twenty source ticks", BuffTime() == effect.Ticks / 4);
            Target(200); cloud = Shot(true); cloud.ModProjectile.OnTileCollide(Vector2.Zero); Tick(cloud, 450);
            Target(40); Tick(cloud, 15);
            check("Lingering radius shrinks with age and excludes its former edge", BuffTime() == 0);
            Target(0); cloud = Shot(true, "ultimate_healing"); npc.lifeMax = 1000; npc.life = 200;
            cloud.ModProjectile.OnTileCollide(Vector2.Zero); Tick(cloud, 31);
            check("Lingering instant healing uses half strength rather than quarter duration scaling", npc.life == 200 + BridgeUnits.Health(8));
            Target(0); cloud = Shot(true); cloud.ModProjectile.OnTileCollide(Vector2.Zero);
            check("Lingering cloud starts with the native thirty-second source lifetime", cloud.timeLeft == BridgeUnits.Ticks(600));
        }
        finally { npc.active = false; player.Center = position; Main.netMode = oldMode; }
        details.Add("Potion delivery compared with original EnigmaticPotionEntity: radius, vertical bound, direct hit, cloud wait, cooldown, shrink and instant potency.");
    }
}
