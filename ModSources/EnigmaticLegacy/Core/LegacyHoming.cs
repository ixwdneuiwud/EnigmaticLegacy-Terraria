// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Microsoft.Xna.Framework;
using Terraria;

namespace EnigmaticLegacy.Core;

public static class LegacyHoming
{
    public static NPC Find(Projectile shot, float radius, bool spread = false)
    {
        NPC best = null; float score = float.MaxValue;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(shot) || !LegacyRealms.Same(npc.Center, shot.Center) ||
                npc.DistanceSQ(shot.Center) > radius * radius ||
                shot.usesLocalNPCImmunity && shot.localNPCImmunity[npc.whoAmI] != 0 ||
                shot.tileCollide && !Collision.CanHitLine(shot.Center, 1, 1, npc.Center, 1, 1)) continue;
            float value = npc.Distance(shot.Center);
            if (spread)
                foreach (Projectile sibling in Main.ActiveProjectiles)
                    if (sibling.whoAmI != shot.whoAmI && sibling.owner == shot.owner && sibling.type == shot.type &&
                        sibling.ai[0] >= 0 && (int)sibling.ai[2] == npc.whoAmI + 1) value += radius * 2;
            if (value < score) { best = npc; score = value; }
        }
        return best;
    }
    public static void Steer(Projectile shot, NPC target, float speed, float amount)
    {
        if (target == null) return;
        shot.velocity = Vector2.Lerp(shot.velocity, (target.Center - shot.Center).SafeNormalize(Vector2.UnitX) * speed, amount);
    }
}
