// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Core;

public static class LegacySummons
{
    public static bool IsSummon(Projectile projectile) => projectile.active && projectile.friendly &&
        (projectile.minion || projectile.sentry || ProjectileID.Sets.MinionShot[projectile.type] ||
         ProjectileID.Sets.SentryShot[projectile.type] || Compatibility.HasProjectileTag(projectile, "summon")) &&
        !Compatibility.HasProjectileTag(projectile, "decorative_pet");
}
