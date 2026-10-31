// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using EnigmaticLegacy.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using EnigmaticLegacy.Content;

namespace EnigmaticLegacy.Core;

/// <summary>No compile-time or reflection dependency on Calamity. Other ports can extend these tags through Mod.Call.</summary>
public static class Compatibility
{
    private static readonly Dictionary<int, HashSet<string>> NpcTags = new();
    private static readonly Dictionary<int, HashSet<string>> ProjectileTags = new();
    private static readonly HashSet<int> ProtectedBuffs = new();
    public static bool CalamityLoaded { get; private set; }

    public static void Initialize(Mod mod)
    {
        foreach (var id in new[] { BuffID.PotionSickness, BuffID.ManaSickness, BuffID.ChaosState, BuffID.MoonLeech })
            ProtectedBuffs.Add(id);
        TagNpcs("undead", NPCID.Zombie, NPCID.ArmedZombie, NPCID.Skeleton, NPCID.AngryBones, NPCID.DarkCaster,
            NPCID.ArmoredSkeleton, NPCID.Mummy, NPCID.DarkMummy, NPCID.LightMummy, NPCID.Wraith, NPCID.Ghost,
            NPCID.UndeadMiner, NPCID.TheGroom, NPCID.TheBride, NPCID.PossessedArmor, NPCID.SkeletonArcher);
        for (int type = 0; type < NPCID.Sets.Zombies.Length; type++)
            if (NPCID.Sets.Zombies[type] || NPCID.Sets.Skeletons[type]) Tag(NpcTags, type, "undead");
        foreach (string name in new[] { "BloodMummy", "Vampire", "VampireBat", "DungeonSpirit", "RaggedCaster", "RaggedCasterOpenCoat",
            "Necromancer", "NecromancerArmored", "DiabolistRed", "DiabolistWhite", "DesertGhoul", "DesertGhoulCorruption",
            "DesertGhoulCrimson", "DesertGhoulHallow", "PirateGhost", "Reaper" })
            if (NPCID.Search.TryGetId(name, out int type)) Tag(NpcTags, type, "undead");
        TagNpcs("aquatic", NPCID.Shark, NPCID.Goldfish, NPCID.Piranha, NPCID.AnglerFish, NPCID.BlueJellyfish,
            NPCID.GreenJellyfish, NPCID.PinkJellyfish, NPCID.DukeFishron, NPCID.Arapaima, NPCID.SeaSnail, NPCID.Squid);
        foreach (int type in new[] { ProjectileID.Bomb, ProjectileID.StickyBomb, ProjectileID.BouncyBomb, ProjectileID.Dynamite, ProjectileID.StickyDynamite })
            Tag(ProjectileTags, type, "explosion");
        TagProjectiles("magic", "ShadowBeamHostile", "InfernoHostileBolt", "InfernoHostileBlast", "LostSoulHostile", "CursedFlameHostile", "GoldenShowerHostile", "CultistBossFireBall", "CultistBossIceMist", "CultistBossLightningOrbArc");
        TagProjectiles("fire", "Fireball", "Flames", "FlamesTrap", "GreekFire1", "GreekFire2", "GreekFire3", "InfernoHostileBolt", "InfernoHostileBlast", "CultistBossFireBall", "GeyserTrap");
        CalamityLoaded = ModLoader.TryGetMod("CalamityMod", out var calamity);
        if (!CalamityLoaded) return;
        // Names are optional lookups. A renamed NPC is simply left untagged; the mod never fails to load.
        foreach (string name in new[] { "AquaticScourgeHead", "AquaticScourgeBody", "AquaticScourgeBodyAlt", "AquaticScourgeTail",
            "DesertScourgeHead", "SulphurousSkater", "SulphurousFlounder", "AcidEel", "NuclearToad", "Trasher", "Skyfin",
            "ReaperShark", "EidolonWyrmHead", "EidolonWyrmBody", "EidolonWyrmTail", "GulperEelHead", "ColossalSquid",
            "Bloatfish", "Laserfish", "LuminousCorvina", "MirageJelly", "OarfishHead", "ToxicMinnow", "Riptide", "GhostBell" })
            if (calamity.TryFind<ModNPC>(name, out var npc)) Tag(NpcTags, npc.Type, "aquatic");
        foreach (string name in new[] { "RavagerHead", "RavagerBody", "RavagerClawLeft", "RavagerClawRight", "RavagerLegLeft", "RavagerLegRight",
            "AquaticScourgeHead", "AquaticScourgeBody", "AquaticScourgeBodyAlt", "AquaticScourgeTail",
            "DesertScourgeHead", "DesertScourgeBody", "DesertScourgeTail" })
            if (calamity.TryFind<ModNPC>(name, out var npc)) Tag(NpcTags, npc.Type, "boss");
        foreach (string name in new[] { "BossEffects", "BossZen" })
            if (calamity.TryFind<ModBuff>(name, out var buff)) ProtectedBuffs.Add(buff.Type);
        mod.Logger.Info($"Calamity compatibility active: {calamity.Version}; optional type tags, native damage hooks, protected control buffs.");
    }

    private static void TagNpcs(string tag, params int[] ids) { foreach (int id in ids) Tag(NpcTags, id, tag); }
    private static void TagProjectiles(string tag, params string[] names)
    {
        foreach (string name in names) if (ProjectileID.Search.TryGetId(name, out int type)) Tag(ProjectileTags, type, tag);
    }
    private static void Tag(Dictionary<int, HashSet<string>> registry, int type, string tag)
    {
        if (!registry.TryGetValue(type, out var tags)) registry[type] = tags = new(StringComparer.OrdinalIgnoreCase);
        tags.Add(tag);
    }
    public static bool HasNpcTag(NPC npc, string tag) => NpcTags.TryGetValue(npc.type, out var tags) && tags.Contains(tag);
    internal static IDisposable PreserveNpcTags() => new NpcTagScope();
    private sealed class NpcTagScope : IDisposable
    {
        private readonly Dictionary<int, HashSet<string>> saved = new();
        public NpcTagScope()
        {
            foreach (var (type, tags) in NpcTags)
                saved[type] = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);
        }
        public void Dispose()
        {
            NpcTags.Clear();
            foreach (var (type, tags) in saved) NpcTags[type] = tags;
        }
    }
    public static bool HasProjectileTag(Projectile projectile, string tag) =>
        tag == "explosion" && (ProjectileID.Sets.Explosive[projectile.type] || projectile.aiStyle == ProjAIStyleID.Explosive) ||
        DamageTypeHasTag(projectile.DamageType,tag) ||
        ProjectileTags.TryGetValue(projectile.type, out var tags) && tags.Contains(tag);
    private static bool DamageTypeHasTag(DamageClass damageClass,string tag)=>damageClass is ImportedDamageClass imported && tag switch {
        "fire"=>imported.Traits.HasFlag(ImportedDamageTraits.Fire),
        "magic"=>imported.Traits.HasFlag(ImportedDamageTraits.Magic),
        "wither"=>imported.Traits.HasFlag(ImportedDamageTraits.Wither),
        "void"=>imported.Traits.HasFlag(ImportedDamageTraits.Void),
        "explosion"=>imported.Traits.HasFlag(ImportedDamageTraits.Explosion),_=>false
    };
    public static bool IsAquatic(NPC npc) => HasNpcTag(npc, "aquatic") || (!npc.townNPC && npc.wet && npc.noGravity && !npc.boss);
    public static bool ProtectedBuff(int type) => ProtectedBuffs.Contains(type);
    public static bool CanReflect(Projectile projectile) => projectile.hostile && projectile.damage > 0 && projectile.velocity.LengthSquared() > 0.1f
        && (projectile.ModProjectile == null && projectile.aiStyle == 1 || HasProjectileTag(projectile, "reflectable")) && !HasProjectileTag(projectile, "unreflectable");

    public static object Call(object[] args)
    {
        if (args.Length == 0 || args[0] is not string command) throw new ArgumentException("Expected a bridge API command.");
        switch (command)
        {
            case "RegisterDynamicLight":
                if (args.Length != 2 || args[1] is not Func<IEnumerable<(Vector2 Position, Vector3 Color)>> light)
                    throw new ArgumentException("RegisterDynamicLight(Func<IEnumerable<(Vector2 Position, Vector3 Color)>> provider)");
                return LegacyDynamicLight.Register(light);
            case "RegisterNpcTag":
                if (args.Length != 3 || args[1] is not int npc || npc < 0 || npc >= NPCLoader.NPCCount || args[2] is not string nt)
                    throw new ArgumentException("RegisterNpcTag(int npcType, string tag)");
                Tag(NpcTags, npc, nt); return true;
            case "RegisterProjectileTag":
                if (args.Length != 3 || args[1] is not int projectile || projectile < 0 || projectile >= ProjectileLoader.ProjectileCount || args[2] is not string pt)
                    throw new ArgumentException("RegisterProjectileTag(int projectileType, string tag)");
                Tag(ProjectileTags, projectile, pt); return true;
            case "ProtectBuff":
                if (args.Length != 2 || args[1] is not int buff || buff < 0 || buff >= BuffLoader.BuffCount)
                    throw new ArgumentException("ProtectBuff(int buffType)");
                ProtectedBuffs.Add(buff); return true;
            case "GetItemType":
                return args.Length == 2 && args[1] is string id && ImportCatalog.TryType(id, out int type) ? type : 0;
            case "GetApiVersion": return 3;
            case "RegisterDamageRule":
                if (args.Length is < 3 or > 4 || args[1] is not string ruleId || args[2] is not DamageHitRule rule || args.Length == 4 && args[3] is not int)
                    throw new ArgumentException("RegisterDamageRule(string source, DamageHitRule rule, int priority = 0)");
                return DamageChannels.RegisterRule(ruleId, rule, args.Length == 4 ? (int)args[3] : 0);
            case "UnregisterDamageRule": return args.Length == 2 && args[1] is string removeRule && DamageChannels.UnregisterRule(removeRule);
            case "RegisterDamageTag":
                if (args.Length != 3 || args[1] is not string tagName || args[2] is not Func<DamageHitContext, bool> classify)
                    throw new ArgumentException("RegisterDamageTag(string tag, Func<DamageHitContext, bool> classifier)");
                return DamageChannels.RegisterTag(tagName, classify);
            case "UnregisterDamageTag": return args.Length == 2 && args[1] is string removeTag && DamageChannels.UnregisterTag(removeTag);
            case "AddUniversalDamage":
                if (args.Length != 5 || args[1] is not Player damagePlayer || args[2] is not string source || args[3] is not DamageOperation operation || args[4] is not float bonus)
                    throw new ArgumentException("AddUniversalDamage(Player player, string source, DamageOperation operation, float bonus)");
                damagePlayer.GetModPlayer<DamageChannelPlayer>().AddUniversal(source, operation, bonus); return true;
            case "RegisterUniversalAccessorySlot":
                if (args.Length != 3 || args[1] is not string slotName || args[2] is not bool isGeneral)
                    throw new ArgumentException("RegisterUniversalAccessorySlot(string slotFullName, bool universal)");
                return AccessoryChannels.Register(slotName, isGeneral);
            case "GetNativeAccessoryCapacity":
                if (args.Length != 2 || args[1] is not Player slotPlayer) throw new ArgumentException("GetNativeAccessoryCapacity(Player player)");
                return AccessoryChannels.NativeCapacity(slotPlayer);
            case "GetDamageClass":
                if (args.Length != 2 || args[1] is not string damageId) throw new ArgumentException("GetDamageClass(string id)");
                return LegacyDamage.Type(damageId);
            case "ApplyNpcDamage":
                if (args.Length != 5 || args[1] is not Player attacker || args[2] is not NPC victim || args[3] is not string sourceType || args[4] is not int amount || amount <= 0)
                    throw new ArgumentException("ApplyNpcDamage(Player owner, NPC victim, string damageType, int damage)");
                if (Main.netMode == NetmodeID.MultiplayerClient) return false;
                attacker.ApplyDamageToNPC(victim, amount, 0, 0, false, LegacyDamage.Type(sourceType)); return true;
            case "ApplyPlayerDamage":
                if (args.Length != 5 || args[1] is not Player hurtPlayer || args[2] is not PlayerDeathReason reason || args[3] is not string playerDamageType || args[4] is not int playerDamage || playerDamage <= 0)
                    throw new ArgumentException("ApplyPlayerDamage(Player victim, PlayerDeathReason reason, string damageType, int damage)");
                if (hurtPlayer.whoAmI != Main.myPlayer) return 0d;
                return LegacyDamage.Hurt(hurtPlayer, LegacyDamage.Type(playerDamageType), reason, playerDamage);
            case "IsPlayerSummon":
                if (args.Length != 2 || args[1] is not Projectile summon) throw new ArgumentException("IsPlayerSummon(Projectile projectile)");
                return LegacySummons.IsSummon(summon);
            case "RegisterFlightAnchor":
                if (args.Length != 2 || args[1] is not int tile || tile < 0 || tile >= TileLoader.TileCount) throw new ArgumentException("RegisterFlightAnchor(int tileType)");
                LegacyFlight.RegisterAnchor(tile); return true;
            case "RegisterFortuneTile":
                if (args.Length != 2 || args[1] is not int ore || ore < 0 || ore >= TileLoader.TileCount) throw new ArgumentException("RegisterFortuneTile(int tileType)");
                LegacyMining.RegisterOre(ore); return true;
            case "GetRealm":
                if (args.Length != 2 || args[1] is not Vector2 center) throw new ArgumentException("GetRealm(Vector2 worldCenter)");
                return LegacyRealms.At(center);
            case "RegisterCursePrefix":
                if (args.Length != 3 || args[1] is not int prefix || args[2] is not bool negative)
                    throw new ArgumentException("RegisterCursePrefix(int prefixType, bool negative)");
                LegacyPrefixes.Register(prefix, negative); return true;
            default: throw new ArgumentException($"Unknown bridge API command: {command}");
        }
    }
    public static void Unload() { NpcTags.Clear(); ProjectileTags.Clear(); ProtectedBuffs.Clear(); CalamityLoaded = false; }
}
