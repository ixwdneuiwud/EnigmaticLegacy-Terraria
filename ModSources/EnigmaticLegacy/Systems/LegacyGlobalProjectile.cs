// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;
    private bool accelerated;
    public override void PostAI(Projectile projectile)
    {
        if (accelerated || projectile.owner != Main.myPlayer || !projectile.friendly || projectile.hostile || projectile.damage <= 0 ||
            projectile.minion || projectile.sentry || LegacySummons.IsSummon(projectile) || projectile.aiStyle != 1 &&
            !Compatibility.HasProjectileTag(projectile, "acceleratable")) return;
        Player owner = Main.player[projectile.owner];
        if (!owner.active || !owner.GetModPlayer<LegacyPlayer>().Has("angel_blessing")) return;
        projectile.velocity *= 1.75f; accelerated = true; projectile.netUpdate = true;
    }
    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers) return null;
        if (Main.player[projectile.owner].GetModPlayer<LegacyPlayer>().Carries("animal_guidebook") && NPCID.Sets.CountsAsCritter[target.type]) return false;
        return null;
    }
}
