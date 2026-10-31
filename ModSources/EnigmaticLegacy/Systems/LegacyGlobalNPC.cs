// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Content;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public int EnragedBy = -1, EnrageTicks;
    public bool DoubleLoot;
    private int strikeDelay;
    private float previousMovementFactor = 1;
    private bool previousVerticalMovement;
    private float verticalBeforeAI;
    private bool restoredFire, priorLavaImmunity;
    public override void ResetEffects(NPC npc)
    {
        if (restoredFire) { npc.lavaImmune = priorLavaImmunity; restoredFire = false; }
        if (LegacyStatus.FireProtected(npc))
        {
            // Clear before vanilla's buff loop so slot order cannot re-enable fire.
            for (int i = npc.buffType.Length - 1; i >= 0; i--)
                if (npc.buffType[i] is BuffID.OnFire or BuffID.OnFire3) { npc.DelBuff(i); npc.netUpdate = true; }
            ProtectFire(npc);
        }
    }
    internal void ProtectFire(NPC npc)
    {
        if (!restoredFire) { priorLavaImmunity = npc.lavaImmune; restoredFire = true; }
        npc.lavaImmune = true; npc.onFire = npc.onFire3 = false;
    }
    public override bool PreAI(NPC npc)
    {
        if (previousMovementFactor != 1)
        {
            npc.velocity.X /= previousMovementFactor;
            if (previousVerticalMovement) npc.velocity.Y /= previousMovementFactor;
            previousMovementFactor = 1;
        }
        verticalBeforeAI = npc.velocity.Y;
        return true;
    }
    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        ImportedDamageTraits traits = (modifiers.DamageType as ImportedDamageClass)?.Traits ?? ImportedDamageTraits.None;
        traits|=BridgeExtensions.AttackTraits(LegacyDamageContext.CurrentAttacker,npc);
        if (traits.HasFlag(ImportedDamageTraits.BypassesArmor)) modifiers.DefenseEffectiveness *= 0;
        if (!traits.HasFlag(ImportedDamageTraits.BypassesResistance)) modifiers.FinalDamage *= LegacyStatus.Resistance(npc);
    }
    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile) => LegacyStatus.FireProtected(npc) && Compatibility.HasProjectileTag(projectile, "fire") ? false : null;
    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (npc.HasBuff(ModContent.BuffType<WitheringIV>()) || npc.HasBuff(ModContent.BuffType<Withering>()) || npc.HasBuff(ModContent.BuffType<LesserWithering>()))
        {
            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= BridgeUnits.Health(npc.HasBuff(ModContent.BuffType<WitheringIV>()) ? 4 : npc.HasBuff(ModContent.BuffType<Withering>()) ? 2 : 1) * 2; damage = Math.Max(damage, BridgeUnits.Health(1));
        }
        if (npc.HasBuff(ModContent.BuffType<DevouringDarkness>()))
        {
            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= BridgeUnits.Health(8) * 2; damage = Math.Max(damage, BridgeUnits.Health(4));
        }
        if (npc.HasBuff(ModContent.BuffType<StrongPoison>()) && npc.lifeRegen > 0) npc.lifeRegen = 0;
    }
    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.SourceDamage.Flat += ContactBonus(npc);
    }
    public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage.Flat += ContactBonus(npc);
    private static int ContactBonus(NPC npc)
    {
        int strength = LegacyStatus.Level(npc, LegacyEffect.Strength), weakness = LegacyStatus.Level(npc, LegacyEffect.Weakness);
        return (strength > 0 ? BridgeUnits.Health(3 * strength) : 0) - (weakness > 0 ? BridgeUnits.Health(4 * weakness) : 0);
    }
    public override void PostAI(NPC npc)
    {
        // Restore the previous multiplier before the next AI pass. Never replace an AI
        // routine or edit its timers/base damage, and preserve boss/segment movement.
        if (!BridgeUnits.IsBoss(npc) && npc.realLife < 0)
        {
            float factor = LegacyStatus.MovementFactor(npc);
            npc.velocity.X *= factor;
            if (npc.noGravity) npc.velocity.Y *= factor;
            previousMovementFactor = factor; previousVerticalMovement = npc.noGravity;
            if (!npc.noGravity && npc.velocity.Y < 0 && verticalBeforeAI >= 0)
                npc.velocity.Y -= 1.5f * LegacyStatus.Level(npc, LegacyEffect.Leaping);
            if (!npc.noGravity && npc.velocity.Y > 2 && LegacyStatus.Level(npc, LegacyEffect.SlowFalling) > 0) npc.velocity.Y = 2;
        }
        if (EnrageTicks <= 0 || Main.netMode == NetmodeID.MultiplayerClient || BridgeUnits.IsBoss(npc) || npc.realLife >= 0) return;
        EnrageTicks--;
        if (++strikeDelay < 30) return;
        strikeDelay = 0;
        foreach (NPC other in Main.ActiveNPCs)
        {
            if (other.whoAmI == npc.whoAmI || !other.CanBeChasedBy() || BridgeUnits.IsBoss(other) || other.realLife >= 0 || other.DistanceSQ(npc.Center) > 192 * 192) continue;
            npc.velocity += (other.Center - npc.Center).SafeNormalize(Vector2.Zero) * 3;
            if (other.DistanceSQ(npc.Center) < 64 * 64 && EnragedBy >= 0 && EnragedBy < Main.maxPlayers)
                Main.player[EnragedBy].ApplyDamageToNPC(other, Math.Max(1, npc.damage), 2, Math.Sign(other.Center.X - npc.Center.X), false);
            break;
        }
    }
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || npc.friendly || npc.SpawnedFromStatue || npc.lifeMax <= 5 || npc.realLife >= 0 || npc.lastInteraction >= Main.maxPlayers) return;
        Player player = Main.player[npc.lastInteraction];
        if (!player.active) return;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (legacy.Has("the_cube")) { var effect = LegacyRandomEffects.Positive(); LegacyStatus.Apply(player, effect.Buff, effect.Ticks); }
        if (legacy.Has("avarice_scroll") && legacy.Cursed && Main.rand.NextFloat() < 0.15f)
            Item.NewItem(npc.GetSource_Loot(), npc.Hitbox, ItemID.GoldCoin);
    }
    public override bool PreKill(NPC npc)
    {
        DoubleLoot = false;
        if (Main.netMode != NetmodeID.MultiplayerClient && LegacyEconomy.Eligible(npc) && npc.lastInteraction >= 0 && npc.lastInteraction < Main.maxPlayers)
        {
            Player player = Main.player[npc.lastInteraction];
            if (player.active && !player.dead) DoubleLoot = Main.rand.NextFloat() < LegacyEconomy.DoubleLootChance(player);
        }
        return true;
    }
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        bool active = false;
        foreach (Player wearer in Main.ActivePlayers)
            if (!wearer.dead && (wearer.GetModPlayer<LegacyPlayer>().BoundDesolation || wearer.GetModPlayer<LegacyPlayer>().Has("desolation_ring"))) { active = true; break; }
        if (!active) return;
        // Applies to the native spawn scheduler, including modded spawn pools.
        spawnRate = (int)Math.Min(int.MaxValue, Math.Max(1L, (long)spawnRate * 2));
        maxSpawns = maxSpawns <= 0 ? 0 : (int)(((long)maxSpawns + 1) / 2);
    }
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (!LegacyEconomy.Avarice(Main.LocalPlayer)) return;
        for (int i = 0; i < items.Length; i++)
        {
            Item item = items[i];
            if (item == null || item.IsAir || item.buyOnce) continue;
            items[i] = item = item.Clone();
            int price = item.shopCustomPrice ?? item.value;
            if (price > 0) item.shopCustomPrice = Math.Max(1, (int)Math.Floor(price * 0.65));
        }
    }
}
