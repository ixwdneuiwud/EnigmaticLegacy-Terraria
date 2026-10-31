// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content;
using EnigmaticLegacy.Core;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

// Loot is rolled inside StrikeNPC, before OnHit callbacks. Scope classification
// to that strike so a later sword hit or a DoT cannot inherit an earlier blast.
public sealed class LegacyDamageContext : ModSystem
{
    [ThreadStatic] private static Projectile projectile;
    [ThreadStatic] private static NPC victim;
    [ThreadStatic] private static bool explosion;
    [ThreadStatic] private static Player attacker;
    public static bool IsExplosionDeath(NPC npc) => ReferenceEquals(victim, npc) && explosion;
    public static Projectile CurrentProjectile => projectile;
    public static Player CurrentAttacker=>attacker;
    public override void Load()
    {
        On_Projectile.Damage += ProjectileDamage;
        On_NPC.StrikeNPC_HitInfo_bool_bool += Strike;
        On_Player.ItemCheck+=ItemCheck;
        On_Player.ApplyDamageToNPC+=PlayerDamage;
    }
    private static void ProjectileDamage(On_Projectile.orig_Damage orig, Projectile shot)
    {
        Projectile previous = projectile;Player oldAttacker=attacker;
        try { projectile = shot;attacker=shot.friendly&&shot.owner>=0&&shot.owner<Main.maxPlayers?Main.player[shot.owner]:null;orig(shot); }
        finally { projectile = previous;attacker=oldAttacker; }
    }
    private static void ItemCheck(On_Player.orig_ItemCheck original,Player player)
    {Player old=attacker;try{attacker=player;original(player);}finally{attacker=old;}}
    private static void PlayerDamage(On_Player.orig_ApplyDamageToNPC original,Player player,NPC target,int damage,float knockback,int direction,bool crit,DamageClass type,bool variation)
    {Player old=attacker;try{attacker=player;original(player,target,damage,knockback,direction,crit,type,variation);}finally{attacker=old;}}
    private static int Strike(On_NPC.orig_StrikeNPC_HitInfo_bool_bool orig, NPC npc, NPC.HitInfo hit, bool fromNet, bool noPlayerInteraction)
    {
        NPC previous = victim; bool wasExplosion = explosion;
        try
        {
            victim = npc;
            explosion = projectile != null && Compatibility.HasProjectileTag(projectile, "explosion") ||
                hit.DamageType is ImportedDamageClass source && source.Traits.HasFlag(ImportedDamageTraits.Explosion);
            return orig(npc, hit, fromNet, noPlayerInteraction);
        }
        finally { victim = previous; explosion = wasExplosion; }
    }
}


