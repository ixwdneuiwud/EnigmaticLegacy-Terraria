// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Projectiles;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

// Native weapon stats, hit hooks and source attribution are shared by future ports.
public static class LegacyWeapons
{
    public static bool Implemented(string id) => id is "etherium_pickaxe" or "etherium_axe" or "etherium_shovel"
        or "etherium_sword" or "etherium_scythe" or "astral_breaker" or "the_twist" or "the_infinitum" or "eldritch_pan" or "forbidden_axe";
    public static bool Journal(string id) => id is "the_twist" or "the_infinitum";
    public static bool Qualified(Player player, string id)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        return Implemented(id) && legacy.CanUse(ImportCatalog.Get(id)) && (id != "eldritch_pan" || legacy.Worthy);
    }
    public static bool CanUse(Player player, ImportedItem item) => Qualified(player, item.ImportId) &&
        (item.ImportId != "eldritch_pan" || player.ownedProjectileCounts[ModContent.ProjectileType<LegacyPan>()] < PanCapacity(item.Item));

    public static int PanGrowth(Item item) => Math.Clamp(item.GetGlobalItem<ItemMetadata>().UniqueKills.Count, 0, 100);
    public static int PanCapacity(Item item) => 2 + PanGrowth(item) / 10;
    public static int PanInterval(Item item) => 8 - PanGrowth(item) / 20;
    public static float PanDamageBonus(Item item) => PanGrowth(item) * 20f;

    // Terraria 1.4.4.9 maxima. Other mods may nerf/buff vanilla samples;
    // Etherium must retain the requested original-game baseline.
    public const int VanillaPick = 230;
    public const int VanillaAxe = 35; // native axe fields store one fifth of the %.
    public static int ExternalPick { get; internal set; } = VanillaPick;
    public static int ExternalAxe { get; internal set; } = VanillaAxe;
    public static void SetToolPower(Item item, string id)
    {
        if (id is "etherium_pickaxe" or "etherium_shovel") item.pick = VanillaPick + 1;
        if (id == "etherium_axe") item.axe = VanillaAxe + 1;
        if (id == "astral_breaker") { item.pick = Math.Max(ExternalPick, VanillaPick + 1); item.axe = Math.Max(ExternalAxe, VanillaAxe + 1); }
    }
    public static void Defaults(Item item, string id)
    {
        if (!Implemented(id)) return;
        int native = id switch {
            "etherium_pickaxe" or "astral_breaker" => ItemID.SolarFlarePickaxe,
            "etherium_axe" => ItemID.LunarHamaxeSolar,
            "etherium_shovel" => ItemID.ShroomiteDiggingClaw,
            "etherium_sword" => ItemID.InfluxWaver,
            "etherium_scythe" => ItemID.DeathSickle,
            "eldritch_pan" => ItemID.PaladinsHammer,
            "forbidden_axe" => ItemID.BloodLustCluster,
            _ => ItemID.MagicDagger
        };
        item.CloneDefaults(native);
        item.DamageType = DamageClass.Melee;
        item.mana = 0; item.consumable = false; item.maxStack = 1;
        item.pick = item.axe = item.hammer = 0;
        item.shoot = 0; item.shootSpeed = 0; item.noMelee = item.noUseGraphic = false;
        item.useStyle = ItemUseStyleID.Swing; item.autoReuse = true;
        item.damage = id switch {
            "the_twist" => 38,
            "the_infinitum" => 1960,
            "eldritch_pan" => 650,
            "forbidden_axe" => 48,
            "etherium_sword" => 140,
            "etherium_scythe" => 135,
            "astral_breaker" => 95,
            _ => 80
        };
        item.useTime = item.useAnimation = id switch {
            "the_twist" => 24,
            "forbidden_axe" => 26,
            "the_infinitum" or "eldritch_pan" => 8,
            "etherium_sword" => 18,
            "etherium_scythe" => 20,
            "astral_breaker" => 16,
            _ => 20
        };
        item.knockBack = id switch { "eldritch_pan" => 9, "the_twist" or "the_infinitum" => 2, _ => 6 };
        item.UseSound = SoundID.Item1;
        item.rare = id is "the_infinitum" or "eldritch_pan" ? ItemRarityID.Red :
            id == "the_twist" ? ItemRarityID.LightRed : ItemRarityID.Cyan;
        item.value = Item.buyPrice(gold: id is "the_infinitum" or "eldritch_pan" ? 20 : 5);
        item.width = item.height = 40;
        if (id == "forbidden_axe") item.axe = 22;
        SetToolPower(item, id);
        if (item.pick > 0 || item.axe > 0) { item.useTime = id == "astral_breaker" ? 4 : 6; item.tileBoost = 3; }
        if (id is "etherium_sword" or "the_twist" or "the_infinitum")
        {
            item.shoot = id == "etherium_sword" ? ModContent.ProjectileType<LegacyEtheriumBolt>() :
                id == "the_twist" ? ModContent.ProjectileType<LegacyTwistBolt>() : ModContent.ProjectileType<LegacyInfinitumBolt>();
            item.shootSpeed = id == "the_twist" ? 15 : 24;
            if (id != "etherium_sword") { item.noMelee = true; item.useStyle = ItemUseStyleID.Shoot; item.UseSound = SoundID.Item8; }
        }
        if (id == "etherium_scythe") { item.shoot = ModContent.ProjectileType<LegacyEtheriumScythe>(); item.shootSpeed = 12; }
        if (id == "eldritch_pan")
        {
            item.shoot = ModContent.ProjectileType<LegacyPan>(); item.shootSpeed = 22;
            item.noMelee = item.noUseGraphic = true;
        }
    }
    public static void ModifyHit(Player player, string id, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!Qualified(player, id)) return;
        // Compensate only this attack's fourth-curse multiplier, never all damage
        // while holding a book/pan (which would empower unrelated minions).
        if (player.GetModPlayer<LegacyPlayer>().Cursed && id is ("the_twist" or "the_infinitum" or "eldritch_pan")) modifiers.FinalDamage *= 2;
        if (BridgeUnits.IsBoss(target)) modifiers.SourceDamage *= id == "the_twist" ? 4 : id == "the_infinitum" ? 3 : 1;
        if (id is "the_twist" or "the_infinitum") modifiers.Knockback *= id == "the_twist" ? 4 : 3;
    }
    public static bool EligibleKill(NPC target) => !target.immortal && !target.SpawnedFromStatue &&
        target.type != NPCID.TargetDummy && target.lifeMax > 0 &&
        (target.realLife < 0 || target.realLife == target.whoAmI);
    public static void OnHit(Player player, string id, NPC target, int damageDone, Item sourceItem = null)
    {
        if (Main.netMode == NetmodeID.Server || player.whoAmI != Main.myPlayer || !Qualified(player, id)) return;
        if (id == "the_infinitum")
        {
            // MC durations are ticks at 20 Hz. Hunger/mining penalties remain omitted.
            target.AddBuff(ModContent.BuffType<WitheringIV>(), 480);
            target.AddBuff(ModContent.BuffType<WeaknessIV>(), 900);
            target.AddBuff(ModContent.BuffType<LegacySlowness>(), 900);
            // Terraria NPCs have no visual blindness; native confusion substitutes
            // both nausea and blindness, taking the longer duration.
            target.AddBuff(BuffID.Confused, 1500);
        }
        LifeSteal(player, target, damageDone, id == "the_infinitum" ? 0.1f : id == "eldritch_pan" ? 0.15f : 0);
        if (id != "eldritch_pan" || target.life > 0 || !EligibleKill(target) || sourceItem?.ModItem is not ImportedItem { ImportId: "eldritch_pan" }) return;
        var data = sourceItem.GetGlobalItem<ItemMetadata>();
        string key = target.ModNPC?.FullName ?? "Terraria/" + NPCID.Search.GetName(target.type);
        if (data.UniqueKills.Count >= 100 || !data.UniqueKills.Add(key)) return;
        int slot = Array.IndexOf(player.inventory, sourceItem);
        if (slot >= 0 && Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
    }
    public static void LifeSteal(Player player, NPC target, int damageDone, float fraction)
    {
        if (Main.netMode == NetmodeID.Server || player.whoAmI != Main.myPlayer || fraction <= 0 ||
            target.type == NPCID.TargetDummy || target.lifeMax <= 5 || target.immortal || target.friendly || target.SpawnedFromStatue ||
            player.moonLeech || player.lifeSteal <= 0) return;
        int heal = Math.Min((int)(Math.Min(damageDone, target.lifeMax) * fraction), (int)player.lifeSteal);
        heal = Math.Min(heal, player.statLifeMax2 - player.statLife);
        if (heal > 0) { player.lifeSteal -= heal; player.Heal(heal); }
    }
}

public sealed class LegacyWeaponSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        var external = ContentSamples.ItemsByType.Values.Where(i => i.ModItem?.Mod != Mod).ToArray();
        LegacyWeapons.ExternalPick = external.Max(i => i.pick);
        LegacyWeapons.ExternalAxe = external.Max(i => i.axe);
        foreach (var item in ContentSamples.ItemsByType.Values)
            if (item.ModItem is ImportedItem imported) LegacyWeapons.SetToolPower(item, imported.ImportId);
    }
    public override void Unload() { LegacyWeapons.ExternalPick = LegacyWeapons.VanillaPick; LegacyWeapons.ExternalAxe = LegacyWeapons.VanillaAxe; }
}
