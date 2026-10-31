// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Projectiles;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public static class LegacyActions
{
    public static bool MaterialOnly(string id) => id is "infinimeal" or "mending_mixture" or "lore_inscriber" or "lore_fragment";
    public static bool IsConsumable(string id) => id is "recall_potion" or "twisted_potion" or "astral_fruit" or "ichor_bottle" or "forbidden_fruit"
        || (id.StartsWith("ultimate_", StringComparison.Ordinal) || id.StartsWith("common_", StringComparison.Ordinal)) && !id.Contains("potion", StringComparison.Ordinal);
    public static bool IsUsable(string id) => id is "the_acknowledgment" or "extradimensional_eye" or "twisted_mirror"
        or "recall_potion" or "twisted_potion" or "forbidden_fruit" or "redemption_potion" or "astral_fruit" or "astral_potato"
        or "tattered_tome" or "withered_tome" or "corrupted_tome" or "xp_scroll" or "mining_charm" or "enigmatic_eye" or "insignia"
        or "guardian_heart" or "void_stone" or "enchantment_transposer" or "curse_transposer" or "lore_inscriber" or "soul_compass"
        or "enchanter_pearl" or "ichor_bottle" or "unholy_grail" or "infinimeal" || id.StartsWith("ultimate_", StringComparison.Ordinal) || id.StartsWith("common_", StringComparison.Ordinal);

    public static bool CanUse(Player player, ImportedItem item)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        string id = item.ImportId;
        if (item.Definition.Kind == "weapon") return LegacyWeapons.CanUse(player, item);
        if (id == "forbidden_fruit") return !legacy.AteForbiddenFruit;
        if (id is "redemption_potion" or "astral_potato" or "xp_scroll" or "heaven_scroll" or "fabulous_scroll"
            or "enchanter_pearl" or "enchantment_transposer" or "curse_transposer" or "tattered_tome" or "withered_tome" or "corrupted_tome"
            or "ultimate_potion" or "ultimate_potion_splash" or "ultimate_potion_lingering" or "common_potion" or "common_potion_splash" or "common_potion_lingering") return false;
        if (id == "ichor_bottle") return !legacy.DrankIchor;
        if (id == "twisted_potion") return LegacyRealms.InCurrentWorld(legacy.LastDeathWorld);
        if (id == "twisted_mirror") return legacy.MirrorCooldown <= 0;
        if (id == "extradimensional_eye") return player.altFunctionUse == 2 ? item.BoundWorld == "" : LegacyRealms.Matches(item.BoundWorld, item.BoundPosition, player.Center);
        if (id == "guardian_heart") return legacy.EnrageCooldown <= 0;
        if (id == "enigmatic_eye") return !item.Tainted && item.EyeAwakeningTicks == 0;
        if (id is "insignia" or "lore_inscriber" or "mega_sponge" or "infinimeal" or "mending_mixture" or "lore_fragment") return false;
        if (id == "infinimeal") return false; // A world-use fertilizer; deliberately not a drink/healing potion.
        if (id is "storage_crystal" or "soul_crystal" or "mending_mixture" or "loot_generator" or "quote_player" or "relic_of_testing" or "animal_guidebook" or "hunter_guidebook" or "infernal_shield" or "cursed_stone") return false;
        return true;
    }
    private static Item Adjacent(Player player) => player.inventory[(player.selectedItem + 1) % 50];
    public static void Use(Player player, ImportedItem item)
    {
        if (item.ImportId is "the_acknowledgment") { LegacyJournal.Open(); return; }
        if (item.ImportId == "soul_compass")
        {
            var legacy = player.GetModPlayer<LegacyPlayer>();
            Main.NewText(BridgeUnits.Text("上次死亡：", "Last death: ") + LegacyRealms.Describe(legacy.LastDeathWorld, legacy.LastDeath) + " " + legacy.LastDeath.ToTileCoordinates()); return;
        }
        LegacyNetwork.Request(BridgeAction.Utility, Main.MouseWorld);
    }
    internal static void ExecuteUtility(Player player, ImportedItem item, Vector2 target, bool alternate)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        switch (item.ImportId)
        {
            case "recall_potion":
                SafeTeleport(player, SpawnPosition(player)); break;
            case "twisted_mirror":
                if (SafeTeleport(player, SpawnPosition(player))) legacy.MirrorCooldown = 600;
                break;
            case "extradimensional_eye":
                if (alternate && item.BoundWorld == "") { item.BoundPosition = player.position; item.BoundWorld = LegacyRealms.KeyAt(player.Center); }
                else if (!alternate && LegacyRealms.Matches(item.BoundWorld, item.BoundPosition, player.Center)) TransportCreature(player, item, target);
                break;
            case "twisted_potion":
                if (LegacyRealms.InCurrentWorld(legacy.LastDeathWorld) && SafeTeleport(player, legacy.LastDeath, LegacyRealms.StoredRealm(legacy.LastDeathWorld, legacy.LastDeath)))
                    player.statLife = 1;
                break;
            case "astral_fruit":
                legacy.AteAstralFruit = true;
                LegacyStatus.Apply(player, ModContent.BuffType<RegenerationIV>(), 9000);
                LegacyStatus.Apply(player, ModContent.BuffType<ResistanceIII>(), 9000);
                LegacyStatus.Apply(player, ModContent.BuffType<StrengthIV>(), 12000);
                LegacyStatus.Apply(player, ModContent.BuffType<LegacyFireResistance>(), 15000); break;
            case "unholy_grail": LegacyGrail.Drink(player); break;
            case "forbidden_fruit": LegacyGrail.EatForbiddenFruit(player); break;
            case "ichor_bottle":
                legacy.DrankIchor = true;
                LegacyStatus.Apply(player, ModContent.BuffType<RegenerationIII>(), 3600);
                LegacyStatus.Apply(player, ModContent.BuffType<AbsorptionV>(), 2400);
                LegacyStatus.Apply(player, ModContent.BuffType<ResistanceIII>(), 4800);
                LegacyStatus.Apply(player, ModContent.BuffType<LegacyFireResistance>(), 9000); break;
            case "mining_charm": item.Disabled = !item.Disabled; break;
            case "enigmatic_eye":
                if (item.Tainted || item.EyeAwakeningTicks > 0) break;
                item.EyeAwakeningTicks = 12;
                if (!Main.dedServ) SoundEngine.PlaySound(new SoundStyle("EnigmaticLegacy/Assets/Sounds/charged_on"), player.Center);
                player.GetModPlayer<LegacyNarrator>().Unlock(240);
                break;
            case "void_stone":
                if (player.controlDown) item.Disabled = !item.Disabled;
                else if (alternate) item.Item.GetGlobalItem<ItemMetadata>().VoidMode = (byte)((item.Item.GetGlobalItem<ItemMetadata>().VoidMode + 1) % 3);
                else if (!Adjacent(player).IsAir)
                {
                    string key = ItemMetadata.ItemKey(Adjacent(player));
                    var filters = item.Item.GetGlobalItem<ItemMetadata>().VoidFilters;
                    if (!filters.Remove(key) && filters.Count < 100) filters.Add(key);
                }
                break;
            case "guardian_heart": Enrage(player, target); break;
            default:
                if (item.Definition.Variant != "" && (item.ImportId.StartsWith("ultimate_", StringComparison.Ordinal) || item.ImportId.StartsWith("common_", StringComparison.Ordinal)))
                {
                    if (item.Definition.Delivery == "") LegacyPotions.Apply(player, item.Definition);
                    else
                    {
                        int p = Projectile.NewProjectile(player.GetSource_ItemUse(item.Item), player.Center,
                            (target - player.Center).SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<LegacyPotionProjectile>(), 0, 0, player.whoAmI,
                            item.Type, item.Definition.Delivery == "lingering" ? 1 : 0);
                        Main.projectile[p].netUpdate = true;
                    }
                }
                break;
        }
        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, player.whoAmI);
    }
    internal static bool TransportCreature(Player player, ImportedItem eye, Vector2 target)
    {
        // A utility interaction without weapon damage. Preserve boss/multipart AI and world boundaries.
        NPC npc = null;
        foreach (NPC candidate in Main.ActiveNPCs)
            if (!candidate.immortal && !BridgeUnits.IsBoss(candidate) && candidate.realLife < 0 &&
                candidate.DistanceSQ(player.Center) <= 96 * 96 && LegacyRealms.Same(player.Center, candidate.Center) && candidate.Hitbox.Contains(target.ToPoint())) { npc = candidate; break; }
        if (npc == null || !LegacyRealms.Matches(eye.BoundWorld, eye.BoundPosition, npc.Center)) return false;
        Vector2 position = eye.BoundPosition;
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || position.X < 32 || position.Y < 32 ||
            position.X + npc.width > Main.maxTilesX * 16 - 32 || position.Y + npc.height > Main.maxTilesY * 16 - 32 ||
            Collision.SolidCollision(position, npc.width, npc.height)) return false;
        npc.position = position; npc.velocity = Vector2.Zero; npc.netUpdate = true;
        if (--eye.Item.stack <= 0) eye.Item.TurnToAir();
        return true;
    }
    public static void ActivateSpellstone(Player player, Vector2 target)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        string spell = legacy.ActiveSpellstone;
        if (spell == null || legacy.SpellCooldown > 0) return;
        bool success = false;
        int cooldown = 0;
        switch (spell)
        {
            case "angel_blessing":
                player.velocity = (target - player.Center).SafeNormalize(new Vector2(player.direction, 0)) * 18;
                player.fallStart = (int)player.position.Y / 16;
                if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
                success = true; cooldown = 120; break;
            case "eye_of_nebula":
                NPC npc = FindTarget(player, target - player.Center, 512);
                if (npc != null)
                {
                    Vector2 behind = npc.Center + (npc.Center - player.Center).SafeNormalize(Vector2.UnitX) * (npc.width / 2 + 32) - player.Size / 2;
                    success = SafeTeleport(player, behind); if (success) legacy.EmpowerNebula();
                }
                cooldown = 180; break;
            case "ocean_stone":
                if (LegacyRealms.At(player.Center) != LegacyRealms.Nether && !Main.raining)
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient) { LegacyNetwork.RequestStorm(); break; }
                    if (!player.CheckMana(150, true, false)) break;
                    player.manaRegenDelay = Math.Max(player.manaRegenDelay, 120);
                    StartStorm();
                    success = true; cooldown = 1800;
                }
                break;
            case "the_cube": success = RandomTeleport(player); cooldown = 9600; break;
            case "enigmatic_item":
                var aim = (target - player.Center).SafeNormalize(new Vector2(player.direction, 0));
                int damage = (int)player.GetTotalDamage(DamageClass.Magic).ApplyTo(BridgeUnits.Health(8));
                Projectile.NewProjectile(player.GetSource_Misc("ELHeartOfCreation"), player.Center, aim * 12,
                    ModContent.ProjectileType<LegacySkull>(), damage, 3, player.whoAmI);
                success = true; cooldown = 9; break;
        }
        if (success) legacy.SpellCooldown = legacy.Has("cosmic_scroll") && spell != "enigmatic_item" ? spell == "angel_blessing" ? 45 : cooldown / 2 : cooldown;
    }
    internal static void StartStorm()
    {
        Main.StartRain(); Main.rainTime = Main.rand.Next(30000, 90000); Main.maxRaining = 0.9f;
        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.WorldData);
    }
    internal static void ReactToContact(Player player, int npcIndex, bool shield)
    {
        if (!player.active || npcIndex < 0 || npcIndex >= Main.maxNPCs) return;
        NPC npc = Main.npc[npcIndex];
        Rectangle area = player.Hitbox; area.Inflate(96, 96);
        if (!npc.active || !npc.Hitbox.Intersects(area)) return;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (shield && legacy.FullEtherium && npc.knockBackResist > 0)
        {
            npc.velocity += (npc.Center - player.Center).SafeNormalize(Vector2.UnitX) * 4.5f * npc.knockBackResist;
            npc.netUpdate = true;
        }
        if (legacy.Has("blazing_core"))
        {
            npc.AddBuff(BuffID.OnFire, 240);
            player.ApplyDamageToNPC(npc, BridgeUnits.Health(4), 0, player.direction, false);
        }
        if (legacy.Has("the_cube")) { var effect = LegacyRandomEffects.Negative(); if (effect.Buff != 0) npc.AddBuff(effect.Buff, effect.Ticks); }
    }
    internal static void ReflectContact(Player player, int npcIndex, int damage)
    {
        if (!player.active || player.dead || npcIndex < 0 || npcIndex >= Main.maxNPCs || !player.GetModPlayer<LegacyPlayer>().Has("the_cube")) return;
        NPC npc = Main.npc[npcIndex];
        if (!npc.active || npc.friendly || npc.immortal || npc.dontTakeDamage || npc.DistanceSQ(player.Center) > 160 * 160) return;
        damage = Math.Clamp(damage, 1, BridgeUnits.Health(player.GetModPlayer<LegacyPlayer>().Cursed ? 150 : 100));
        player.ApplyDamageToNPC(npc, damage, 0, player.direction, false);
    }
    public static bool SafeTeleport(Player player, Vector2 desired, string requiredRealm = null)
    {
        if (!float.IsFinite(desired.X) || !float.IsFinite(desired.Y)) return false;
        desired.X = MathHelper.Clamp(desired.X, 32, Main.maxTilesX * 16 - player.width - 32);
        desired.Y = MathHelper.Clamp(desired.Y, 32, Main.maxTilesY * 16 - player.height - 32);
        requiredRealm ??= LegacyRealms.At(desired + player.Size / 2);
        for (int offset = 0; offset <= 160; offset += 16)
        {
            var position = desired - new Vector2(0, offset);
            if (position.Y < 32 || LegacyRealms.At(position + player.Size / 2) != requiredRealm || Collision.SolidCollision(position, player.width, player.height)) continue;
            Point tile = (position + player.Size / 2).ToTileCoordinates();
            if (Main.tile[tile.X, tile.Y].LiquidAmount > 0 && Main.tile[tile.X, tile.Y].LiquidType == LiquidID.Lava) continue;
            player.Teleport(position, TeleportationStyleID.RodOfDiscord);
            player.velocity = Vector2.Zero; player.fallStart = (int)position.Y / 16;
            if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, position.X, position.Y, TeleportationStyleID.RodOfDiscord);
            return true;
        }
        return false;
    }
    public static bool TryNearbyTeleport(Player player, float range)
    {
        for (int attempt = 0; attempt < 30; attempt++)
            if (SafeTeleport(player, player.position + Main.rand.NextVector2Circular(range, range), LegacyRealms.At(player.Center))) return true;
        return false;
    }
    public static bool RandomTeleport(Player player)
    {
        // The Cube chooses another logical realm. Terraria's Underworld is the Minecraft Nether here.
        string destination = LegacyRealms.At(player.Center) == LegacyRealms.Nether ? LegacyRealms.Overworld : LegacyRealms.Nether;
        for (int i = 0; i < 100; i++)
        {
            int x = Main.rand.Next(100, Main.maxTilesX - 100);
            int y = destination == LegacyRealms.Nether ? Main.rand.Next(Main.UnderworldLayer + 4, Main.maxTilesY - 30) : Main.rand.Next(100, Math.Max(101, (int)Main.worldSurface));
            if (SafeTeleport(player, new Vector2(x, y) * 16, destination)) return true;
        }
        return false;
    }
    public static Vector2 SpawnPosition(Player player) => new((player.SpawnX >= 0 ? player.SpawnX : Main.spawnTileX) * 16, (player.SpawnY >= 0 ? player.SpawnY : Main.spawnTileY) * 16 - player.height);
    public static NPC FindTarget(Player player, Vector2 direction, float range, bool lineOfSight = false)
    {
        direction = direction.SafeNormalize(new Vector2(player.direction, 0));
        NPC best = null; float score = float.MaxValue;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy() || !LegacyRealms.Same(player.Center, npc.Center) || npc.DistanceSQ(player.Center) > range * range) continue;
            Vector2 delta = npc.Center - player.Center;
            float alignment = Vector2.Dot(delta.SafeNormalize(Vector2.Zero), direction);
            if (alignment < 0.75f || lineOfSight && !Collision.CanHitLine(player.position, player.width, player.height, npc.position, npc.width, npc.height)) continue;
            float candidate = delta.Length() * (2 - alignment);
            if (candidate < score) { score = candidate; best = npc; }
        }
        return best;
    }
    public static bool SourceInFront(Player player, PlayerDeathReason source)
    {
        if (source.SourceNPCIndex >= 0 && source.SourceNPCIndex < Main.maxNPCs) return Math.Sign(Main.npc[source.SourceNPCIndex].Center.X - player.Center.X) == player.direction;
        if (source.SourceProjectileLocalIndex >= 0 && source.SourceProjectileLocalIndex < Main.maxProjectiles) return Math.Sign(Main.projectile[source.SourceProjectileLocalIndex].Center.X - player.Center.X) == player.direction;
        return false;
    }
    public static void ReflectProjectile(Player player, int index)
    {
        if (index < 0 || index >= Main.maxProjectiles) return;
        Projectile projectile = Main.projectile[index];
        // Custom hostile AI may depend on owner/hostile flags. Keep its behaviour intact and use the dodge alone unless registered safe.
        if (!projectile.active || !Compatibility.CanReflect(projectile)) return;
        projectile.velocity *= -1; projectile.hostile = false; projectile.friendly = true; projectile.owner = player.whoAmI;
        projectile.netUpdate = true;
    }
    private static void Enrage(Player player, Vector2 target)
    {
        NPC npc = FindTarget(player, target - player.Center, 384);
        if (npc == null || BridgeUnits.IsBoss(npc) || npc.realLife >= 0) return;
        npc.GetGlobalNPC<LegacyGlobalNPC>().EnragedBy = player.whoAmI;
        npc.GetGlobalNPC<LegacyGlobalNPC>().EnrageTicks = 600;
        player.GetModPlayer<LegacyPlayer>().EnrageCooldown = 600;
    }
    public static int RandomDebuff() => LegacyRandomEffects.Negative().Buff;
    public static void OpenEnderStorage(Player player)
    {
        if (Main.dedServ || player.whoAmI != Main.myPlayer) return;
        player.chest = player.chest == -5 ? -1 : -5; Main.playerInventory = true; Recipe.FindRecipes();
        SoundEngine.PlaySound(SoundID.MenuOpen, player.Center);
    }
}
