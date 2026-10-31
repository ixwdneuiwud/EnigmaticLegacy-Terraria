// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Microsoft.Xna.Framework;
using Terraria;
using EnigmaticLegacy.Players;

namespace EnigmaticLegacy.Core;

public static class LegacyGaze
{
    public static bool Pvp(Player owner, Player victim) => owner != victim && owner.active && victim.active &&
        !owner.dead && !victim.dead && owner.hostile && victim.hostile && (owner.team == 0 || owner.team != victim.team);
    public static bool Observed(Player player, Entity target)
    {
        if (!LegacyRealms.Same(player.Center, target.Center)) return false;
        Vector2 direction = player.GetModPlayer<LegacyAim>().Direction;
        if (direction.LengthSquared() < .5f) direction = new Vector2(player.direction, 0);
        Vector2 delta = target.Center - player.Center;
        float distance = Vector2.Dot(delta, direction);
        if (distance <= 0 || distance > 2048) return false;
        float across = System.MathF.Abs(delta.X * direction.Y - delta.Y * direction.X);
        return across <= 48 + System.Math.Max(target.width, target.height) / 2f &&
            Collision.CanHitLine(player.position, player.width, player.height, target.position, target.width, target.height);
    }
}
