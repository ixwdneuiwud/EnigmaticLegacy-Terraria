// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Players;

public sealed class LegacyPlayer : ModPlayer
{
    private readonly Dictionary<string, ImportedItem> equipped = new(StringComparer.Ordinal);
    private readonly HashSet<string> inventory = new(StringComparer.Ordinal);
    public bool BoundCurses, BoundDesolation, AteForbiddenFruit, AteAstralFruit, GrantedGifts;
    public bool FirstChestReward, FirstMoonLordReward;
    public bool MagnetEnabled = true, ShieldHeld, DrankIchor;
    public int LostSouls, SpellCooldown, RevivalCooldown, ShieldPower, EnrageCooldown, MirrorCooldown;
    public long PlayTicks, CursedTicks;
    public Vector2 LastDeath;
    public string LastDeathWorld = "";
    public string OwnerId = "";
    public List<Item> RetainedItems = new();
    private int lifeBeforeHurt;
    private int absorptionLevel;
    private int pendingStatusHealing, pendingAbsorption;
    private float statusHealingRemainder;
    public int AbsorptionShield { get; private set; }
    public ImportedDamageClass IncomingDamageType;
    private bool shieldBeforeHurt;
    private bool nebulaPower;
    private float regenScale = 1f;
    private int lavaHeat, coreTicks;
    private int lastStateSync;
    private int flightManaTicks, anchorScanTicks;
    private string flightAnchorRealm = "";
    private bool nearFlightAnchor;
    private Item flightWings;
    private readonly LegacyLight darknessLight = new();
    public bool Has(string id) => equipped.ContainsKey(id);
    public bool Carries(string id) => inventory.Contains(id) || Has(id);
    public bool Cursed => BoundCurses || Has("cursed_ring");
    public bool Worthy => Cursed && PlayTicks > 0 && (double)CursedTicks / PlayTicks >= 0.995;
    public bool FullEtherium => Has("etherium_helmet") && Has("etherium_chestplate") && Has("etherium_leggings") && Has("etherium_boots");
    public bool EtheriumShieldActive => FullEtherium && Player.statLife <= Player.statLifeMax2 * (Has("cosmic_scroll") ? 0.8f : 0.4f) || BridgeExtensions.ExtraShield(Player);
    public float MissingHealth => 1f - MathHelper.Clamp((float)Player.statLife / Math.Max(1, Player.statLifeMax2), 0, 1);
    public bool HasAmuletColor(string color) => Has("ascension_amulet") ||
        Has(color == "red" ? "enigmatic_amulet" : "enigmatic_amulet_" + color);
    public string ActiveSpellstone => equipped.Keys.FirstOrDefault(k => ImportCatalog.Get(k).Kind == "spellstone");
    public void Equip(ImportedItem item) => equipped.TryAdd(item.ImportId, item);
    public void Carry(string id) => inventory.Add(id);
    public bool CanUse(ImportedDefinition definition) => definition.Gate switch { "cursed" or "eldritch" => BridgeExtensions.HasAccess(Player, definition.Gate, "enigmaticlegacy:" + definition.Id), _ => true };

    public override void Initialize() => OwnerId = Guid.NewGuid().ToString("N");
    public override void ResetEffects()
    {
        equipped.Clear(); inventory.Clear(); regenScale = 1f; pendingStatusHealing = 0;
    }
    public override void UpdateDead()
    {
        equipped.Clear(); inventory.Clear(); ShieldHeld = false;
        AbsorptionShield = absorptionLevel = 0;
        statusHealingRemainder = 0;
        if (SpellCooldown > 0) SpellCooldown--;
        if (RevivalCooldown > 0) RevivalCooldown--;
    }
    public override void PostUpdateEquips()
    {
        if (Has("cursed_ring") && PortConfig.Current.PermanentCurses) BoundCurses = true;
        if (Has("desolation_ring") && PortConfig.Current.PermanentCurses) BoundDesolation = true;
        if (Has("iron_ring")) Player.statDefense += BridgeUnits.Armor(1);
        if (Has("eldritch_amulet")) Player.GetDamage(DamageClass.Melee).Base += BridgeUnits.Health(3);
        else if (HasAmuletColor("red")) Player.GetDamage(DamageClass.Melee).Base += BridgeUnits.Health(1.5f);
        if (HasAmuletColor("aqua") && (Player.controlLeft || Player.controlRight)) Player.moveSpeed += 0.15f;
        if (HasAmuletColor("magenta")) Player.gravity *= 0.75f;
        if (HasAmuletColor("green")) Player.pickSpeed /= 1.25f;
        if (HasAmuletColor("blue") && Player.wet) { Player.accRunSpeed *= 1.25f; Player.ignoreWater = true; }
        if (Has("mining_charm"))
        {
            Player.pickSpeed /= 1.3f; Player.blockRange += 2;
            if (!equipped["mining_charm"].Disabled) Player.nightVision = true;
        }
        if (Has("cursed_scroll") && Cursed)
        {
            int curses = LegacyPrefixes.TotalCurses(Player);
            Player.GetModPlayer<DamageChannelPlayer>().AddUniversal("enigmaticlegacy:living_hurt", DamageOperation.MultiplyBase, .08f * curses);
            Player.pickSpeed /= 1 + 0.14f * curses;
            regenScale *= 1 + 0.08f * curses;
        }
        if (Has("berserk_charm"))
        {
            Player.GetModPlayer<DamageChannelPlayer>().AddUniversal("enigmaticlegacy:living_hurt", DamageOperation.MultiplyBase, MissingHealth);
            Player.GetAttackSpeed(DamageClass.Generic) += MissingHealth;
            Player.moveSpeed += MissingHealth * 0.5f;
        }
        if (Has("golem_heart"))
        {
            bool naked = Player.armor[0].IsAir && Player.armor[1].IsAir && Player.armor[2].IsAir;
            Player.statDefense += BridgeUnits.Armor(naked ? 16 : 4);
            Player.noKnockback = true; Player.fireWalk = true;
        }
        if (Has("eye_of_nebula")) Player.GetDamage(DamageClass.Magic) *= 1.4f;
        if (Has("enigmatic_eye")) Player.blockRange += 3;
        if (Has("angel_blessing"))
        {
            Player.noFallDmg = true;
            Player.GetJumpState<AngelFlightJump>().Enable();
            if (SpellCooldown == 0) Player.GetJumpState<AngelFlightJump>().Available = true;
        }
        if (Has("blazing_core"))
        {
            ProtectFromFire();
            if (Player.lavaWet) Player.nightVision = true;
            bool exposed = Player.lavaWet && !Player.lavaImmune && !Player.HasBuff(BuffID.ObsidianSkin);
            if (exposed && lavaHeat < 30000) Player.lavaImmune = true;
            if (++coreTicks % 3 == 0) lavaHeat = Math.Clamp(lavaHeat + (exposed ? 100 : -200), 0, 32000);
            for (int i = 0; i < Player.buffType.Length; i++)
            {
                if (Player.buffType[i] == ModContent.BuffType<MoltenHeart>())
                {
                    if (coreTicks % 2 == 0 && Player.buffTime[i] > 0) Player.buffTime[i]++;
                    continue;
                }
                if (Player.buffTime[i] > 1 && !Compatibility.ProtectedBuff(Player.buffType[i]) &&
                    !Main.vanityPet[Player.buffType[i]] && !Main.lightPet[Player.buffType[i]])
                    Player.buffTime[i]--;
            }
        }
        else if (lavaHeat > 0) lavaHeat = Math.Max(0, lavaHeat - 67);
        if (Has("ocean_stone"))
        {
            Player.gills = true; Player.ignoreWater = true;
            if (Player.wet && !Player.honeyWet && !Player.lavaWet && !Player.shimmerWet)
            { Player.accRunSpeed *= 3; Player.maxRunSpeed *= 3; Player.GetJumpState(ExtraJump.Flipper).Enable(); }
            if (LegacyWater.EyesSubmerged(Player))
            {
                Player.nightVision = true;
                if (!Player.mount.Active) Player.gravity = 0;
            }
        }
        if (Has("void_pearl")) { Player.gills = true; BlockStatusEffects(true); regenScale *= 0.5f; }
        if (Has("the_cube"))
        {
            Player.moveSpeed += 0.35f; Player.pickSpeed /= 1.6f;
            Player.GetAttackSpeed(DamageClass.Generic) += 0.4f;
            Player.gills = true; Player.noFallDmg = true; Player.fireWalk = true;
            Player.lavaImmune = true; ProtectFromFire(); BlockStatusEffects(false);
            if (Player.wet) { Player.ignoreWater = true; Player.accRunSpeed *= 2; Player.GetJumpState(ExtraJump.Flipper).Enable(); }
        }
        if (Has("enigmatic_item"))
        {
            Player.gills = true; Player.noFallDmg = true; Player.lavaImmune = true; Player.fireWalk = true;
            ProtectFromFire(); BlockStatusEffects(false);
        }
        if (Carries("infernal_shield") && Cursed && !Player.lavaWet) ProtectFromFire();
        if (ShieldPower > 0) Player.GetDamage(DamageClass.Melee) += 0.2f;
        if (LostSouls > 0)
            Player.statLifeMax2 = Math.Max(BridgeUnits.Health(1), (int)(Player.statLifeMax2 * (1 - Math.Min(9, LostSouls) * 0.1f)));
        HandleFlight();
        LegacyStatus.PlayerStats(Player);
        if (pendingStatusHealing > 0 && Player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
        {
            if (Player.statLife < Player.statLifeMax2)
            {
                float healing = pendingStatusHealing * regenScale + statusHealingRemainder;
                int whole = Math.Max(0, (int)MathF.Floor(healing + 0.00001f));
                statusHealingRemainder = Math.Max(0, healing - whole);
                Player.Heal(whole);
                if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);
            }
            else statusHealingRemainder = 0;
            pendingStatusHealing = 0;
        }
    }
    internal void QueueStatusRegeneration(int amount) => pendingStatusHealing = Math.Max(pendingStatusHealing, amount);
    public override void PostUpdateBuffs()
    {
        UpdateAbsorption();
        if (LegacyStatus.Level(Player, LegacyEffect.Regeneration) == 0) statusHealingRemainder = 0;
    }
    public override void PostUpdateRunSpeeds()
    {
        float factor = Math.Max(0.1f, 1 - 0.15f * LegacyStatus.Level(Player, LegacyEffect.Slowness));
        Player.maxRunSpeed *= factor; Player.accRunSpeed *= factor; Player.runAcceleration *= factor;
    }
    internal void UpdateAbsorption()
    {
        int level = LegacyStatus.Level(Player, LegacyEffect.Absorption);
        if (level == 0) AbsorptionShield = 0;
        else if (level != absorptionLevel) AbsorptionShield = BridgeUnits.Health(4 * level);
        else AbsorptionShield = Math.Min(AbsorptionShield, BridgeUnits.Health(4 * level));
        absorptionLevel = level;
    }
    internal void AbsorbDamage(ref Player.HurtInfo info)
    {
        UpdateAbsorption();
        if (AbsorptionShield == 0 || info.Cancelled) return;
        if (AbsorptionShield >= info.Damage)
        {
            // Dodgeable hits reach ConsumableDodge after free/vanilla dodges.
            if (!info.Dodgeable) { AbsorptionShield -= info.Damage; info.Cancelled = true; }
            return;
        }
        pendingAbsorption = AbsorptionShield;
        info.Damage -= pendingAbsorption;
    }
    public override bool ConsumableDodge(Player.HurtInfo info)
    {
        if (pendingAbsorption != 0 || AbsorptionShield < info.Damage || info.Cancelled) return false;
        AbsorptionShield -= info.Damage;
        return true;
    }
    private void ProtectFromFire()
    {
        Player.buffImmune[BuffID.OnFire] = true; Player.buffImmune[BuffID.OnFire3] = true;
        Player.buffImmune[BuffID.Burning] = true;
    }
    private void BlockStatusEffects(bool includePositive)
    {
        for (int i = 1; i < BuffLoader.BuffCount; i++)
            if (!Compatibility.ProtectedBuff(i) && !Main.vanityPet[i] && !Main.lightPet[i] && (includePositive || Main.debuff[i]))
                Player.buffImmune[i] = true;
    }
    public override void UpdateLifeRegen()
    {
        if (Player.lifeRegen > 0) Player.lifeRegen = (int)(Player.lifeRegen * regenScale);
    }
    public override void UpdateBadLifeRegen()
    {
        int damagePerSecond = 0;
        if (Player.HasBuff(ModContent.BuffType<WitheringIV>())) damagePerSecond += BridgeUnits.Health(4);
        else if (Player.HasBuff(ModContent.BuffType<Withering>())) damagePerSecond += BridgeUnits.Health(2);
        else if (Player.HasBuff(ModContent.BuffType<LesserWithering>())) damagePerSecond += BridgeUnits.Health(1);
        if (Player.HasBuff(ModContent.BuffType<DevouringDarkness>())) damagePerSecond += BridgeUnits.Health(8);
        if (Player.HasBuff(ModContent.BuffType<StrongPoison>()) && Player.lifeRegen > 0) { Player.lifeRegen = 0; Player.lifeRegenTime = 0; }
        if (damagePerSecond <= 0) return;
        if (Player.lifeRegen > 0) Player.lifeRegen = 0;
        Player.lifeRegenTime = 0;
        Player.lifeRegen -= damagePerSecond * 2;
    }
    public override void ModifyLuck(ref float luck)
    {
        if (Has("golden_ring")) luck += 1;
        if (Has("the_cube")) luck += 1;
        if (Has("cosmic_scroll")) luck += 1;
    }
    public override void PostUpdate()
    {
        if (Player.dead) return;
        PlayTicks++;
        if (Cursed) CursedTicks++;
        if (SpellCooldown > 0) SpellCooldown--;
        if (RevivalCooldown > 0) RevivalCooldown--;
        if (EnrageCooldown > 0) EnrageCooldown--;
        if (MirrorCooldown > 0) MirrorCooldown--;
        if (ShieldPower > 0) ShieldPower--;
        if (Cursed)
        {
            Player.sleeping.StopSleeping(Player);
            if (!Player.wet && Player.HasBuff(BuffID.OnFire) && !Player.buffImmune[BuffID.OnFire]) Player.AddBuff(BuffID.OnFire, 120);
            if (Player.wet) Player.ClearBuff(BuffID.OnFire);
            Player.aggro += 400;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 30 == 0)
        {
            if (Has("void_pearl")) ApplyDarknessAura();
            LegacyWorld.ReclaimNearby(Player);
        }
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.GameUpdateCount % 15 == 0 && Has("eldritch_amulet") && Worthy) ApplyGaze();
        if (Player.whoAmI == Main.myPlayer && ++lastStateSync >= 120) { lastStateSync = 0; LegacyNetwork.SendState(this); }
    }
    private void HandleFlight()
    {
        bool free = Has("enigmatic_item") || Has("enigmatic_elytra");
        bool scroll = Has("heaven_scroll") || Has("fabulous_scroll");
        if (!free && !scroll) { flightManaTicks = anchorScanTicks = 0; nearFlightAnchor = false; flightAnchorRealm = ""; return; }
        string realm = LegacyRealms.At(Player.Center);
        if (scroll && (++anchorScanTicks >= 30 || anchorScanTicks == 1 || flightAnchorRealm != realm))
        {
            nearFlightAnchor = LegacyFlight.NearAnchor(Player);
            flightAnchorRealm = realm;
            anchorScanTicks = 1;
        }
        bool granted = free || scroll && (Has("fabulous_scroll") || nearFlightAnchor) && (Has("fabulous_scroll") && nearFlightAnchor || flightManaTicks > 0 || Player.statMana > 0);
        if (!free && scroll && granted && Player.whoAmI == Main.myPlayer && !Player.mount.Active && Player.controlJump && Player.velocity.Y != 0)
        {
            bool paid = !(Has("fabulous_scroll") && nearFlightAnchor);
            if (paid && flightManaTicks == 0)
            {
                int cost = Has("fabulous_scroll") ? 8 : 2;
                granted = Player.CheckMana(cost, true, false);
                if (granted) { flightManaTicks = 60; Player.manaRegenDelay = Math.Max(Player.manaRegenDelay, 90); }
            }
        }
        if (flightManaTicks > 0) flightManaTicks--;
        if (!granted) return;
        flightWings ??= new Item(ImportCatalog.Type("enigmatic_elytra"));
        LegacyFlight.Grant(Player, flightWings, true);
    }
    public override void PostUpdateMiscEffects()
    {
        // Express the bonus through vanilla regen inputs, retaining native delays,
        // current-mana scaling, potion bonuses and other mods' regeneration.
        float efficiency = (Cursed ? 0.2f : 0) + (Has("enchanter_pearl") ? 0.5f : 0);
        if (efficiency <= 0) return;
        int baseline = Player.statManaMax2 / 3 + 1 + Player.manaRegenBonus;
        if (Player.IsStandingStillForSpecialEffects || Player.grappling[0] >= 0 || Player.manaRegenBuff) baseline += Player.statManaMax2 / 3;
        if (Player.usedArcaneCrystal) baseline += Player.statManaMax2 / 50;
        Player.manaRegenBonus += (int)Math.Ceiling(Math.Max(0, baseline) * efficiency);
        Player.manaRegenDelayBonus += efficiency;
    }
    internal void ApplyDarknessAura()
    {
        darknessLight.Scan(Player.Center, 16);
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy() || !LegacyRealms.Same(Player.Center, npc.Center) || npc.DistanceSQ(Player.Center) > 256 * 256) continue;
            if (darknessLight.Brightness(npc.Center) >= 0.2f && !Compatibility.HasNpcTag(npc, "darkness_hunter")) continue;
            Player.ApplyDamageToNPC(npc, BridgeUnits.Health(4), 0, 0, false, ModContent.GetInstance<DarknessDamage>());
            npc.AddBuff(ModContent.BuffType<LesserWithering>(), 240);
            // The original also applies Hunger III. Hunger remains deferred, with no replacement debuff.
            npc.AddBuff(ModContent.BuffType<SlownessIII>(), 300);
        }
    }
    internal void ApplyGaze()
    {
        foreach (NPC target in Main.ActiveNPCs)
        {
            if (target.immortal || target.dontTakeDamage || !LegacyGaze.Observed(Player, target)) continue;
            target.AddBuff(ModContent.BuffType<SlownessII>(), 30);
            target.AddBuff(ModContent.BuffType<WeaknessII>(), 30);
        }
    }
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (EnigmaticLegacy.SpellstoneKey.JustPressed) LegacyNetwork.Request(BridgeAction.Spellstone, Main.MouseWorld);
        if (EnigmaticLegacy.EnderKey.JustPressed && (Has("ender_ring") || Cursed)) LegacyActions.OpenEnderStorage(Player);
        if (EnigmaticLegacy.MagnetKey.JustPressed) LegacyNetwork.Request(BridgeAction.ToggleMagnet, Player.Center);
        bool shield = EnigmaticLegacy.ShieldKey.Current && Carries("infernal_shield") && Cursed;
        if (ShieldHeld != shield) { ShieldHeld = shield; LegacyNetwork.Request(shield ? BridgeAction.ShieldUp : BridgeAction.ShieldDown, Player.Center); }
    }
    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (Has("void_pearl") && IncomingDamageType is IndirectMagicDamage)
        { Player.Heal(Math.Max(0, info.Damage)); return true; }
        bool creature = LegacyDamage.IsCreatureSource(info.DamageSource);
        if (Has("the_cube") && creature && info.Damage > BridgeUnits.Health(Cursed ? 150 : 100)) return true;
        if (Has("eye_of_nebula") && creature && Main.rand.NextFloat() < 0.15f && LegacyActions.TryNearbyTeleport(Player, 256)) return true;
        if (info.DamageSource.SourceProjectileLocalIndex >= 0)
        {
            int index = info.DamageSource.SourceProjectileLocalIndex;
            if (EtheriumShieldActive && index < Main.maxProjectiles && Compatibility.CanReflect(Main.projectile[index]))
            {
                LegacyActions.ReflectProjectile(Player, index);
                return true;
            }
            float chance = (Has("angel_blessing") ? 0.5f : 0) + (HasAmuletColor("violet") ? 0.15f : 0) + (Has("the_cube") ? 0.35f : 0);
            if (Main.rand.NextFloat() < chance)
            {
                LegacyActions.ReflectProjectile(Player, info.DamageSource.SourceProjectileLocalIndex);
                return true;
            }
        }
        if (ShieldHeld && Carries("infernal_shield") && Cursed && LegacyActions.SourceInFront(Player, info.DamageSource))
        {
            ShieldPower = 600; return true;
        }
        if (Has("the_cube") && info.DamageSource.SourceNPCIndex >= 0 && Main.rand.NextFloat() < 0.35f)
        {
            LegacyNetwork.ReflectContact(Player, info.DamageSource.SourceNPCIndex, info.Damage);
            return true;
        }
        return false;
    }
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        lifeBeforeHurt = Player.statLife;
        pendingAbsorption = 0;
        shieldBeforeHurt = EtheriumShieldActive;
        if (Cursed) { modifiers.FinalDamage *= 2; modifiers.ScalingArmorPenetration += 0.3f; modifiers.Knockback *= 2; }
        if (HasAmuletColor("magenta") && modifiers.DamageSource.SourceOtherIndex == 0) modifiers.SourceDamage.Flat -= BridgeUnits.Health(2);
        ImportedDamageTraits traits = IncomingDamageType?.Traits ?? ImportedDamageTraits.None;
        if (traits.HasFlag(ImportedDamageTraits.BypassesArmor)) modifiers.ScalingArmorPenetration += 1;
        if (!traits.HasFlag(ImportedDamageTraits.BypassesResistance)) modifiers.FinalDamage *= LegacyStatus.Resistance(Player);
        if (Has("angel_blessing") && (traits.HasFlag(ImportedDamageTraits.Wither) || traits.HasFlag(ImportedDamageTraits.Void))) modifiers.FinalDamage *= 2;
        if (AbsorptionShield > 0 || LegacyStatus.Level(Player, LegacyEffect.Absorption) > 0) modifiers.ModifyHurtInfo += AbsorbDamage;
        if (Has("berserk_charm")) modifiers.FinalDamage *= 1 - MissingHealth * 0.5f;
        if (EtheriumShieldActive) modifiers.FinalDamage *= 0.5f;
        if (Has("eye_of_nebula") && Player.wet) modifiers.FinalDamage *= 2;
        if (ShieldHeld && Carries("infernal_shield") && Cursed && !LegacyActions.SourceInFront(Player, modifiers.DamageSource)) modifiers.FinalDamage *= 1.5f;
    }
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Has("golem_heart")) modifiers.FinalDamage *= 0.75f;
        if (Compatibility.IsAquatic(npc))
        {
            if (Has("ocean_stone")) modifiers.FinalDamage *= 0.6f;
            if (Has("blazing_core")) modifiers.FinalDamage *= 2;
        }
    }
    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        bool magic = proj.DamageType.CountsAsClass(DamageClass.Magic) || Compatibility.HasProjectileTag(proj, "magic");
        if (Has("eye_of_nebula") && magic) modifiers.FinalDamage *= 0.35f;
        if (Has("golem_heart") && magic) modifiers.FinalDamage *= 2;
        if (Has("golem_heart") && Compatibility.HasProjectileTag(proj, "explosion") && Player.armor[0].IsAir && Player.armor[1].IsAir && Player.armor[2].IsAir)
            modifiers.FinalDamage *= 0.6f;
        if (Has("ocean_stone") && Compatibility.HasProjectileTag(proj, "fire")) modifiers.FinalDamage *= 2;
    }
    public override bool CanBeHitByProjectile(Projectile projectile) => !LegacyStatus.FireProtected(Player) || !Compatibility.HasProjectileTag(projectile, "fire");
    public override void OnHurt(Player.HurtInfo info)
    {
        if (pendingAbsorption > 0) { AbsorptionShield = Math.Max(0, AbsorptionShield - pendingAbsorption); pendingAbsorption = 0; }
        ShieldPower = 0;
        if (Player.whoAmI == Main.myPlayer && info.DamageSource.SourceNPCIndex >= 0 && info.DamageSource.SourceNPCIndex < Main.maxNPCs)
            LegacyNetwork.ContactReaction(Player, info.DamageSource.SourceNPCIndex, shieldBeforeHurt);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Has("monster_charm")) modifiers.FinalDamage *= Compatibility.HasNpcTag(target, "undead") ? 1.25f : 1.1f;
        if (Cursed) modifiers.FinalDamage *= 0.5f;
        if (nebulaPower) modifiers.FinalDamage *= 2.5f;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        nebulaPower = false;
        if (Has("void_pearl")) target.AddBuff(ModContent.BuffType<Withering>(), 300);
        float fraction = (Has("eldritch_amulet") ? 0.15f : HasAmuletColor("black") ? 0.1f : 0);
        LegacyWeapons.LifeSteal(Player, target, damageDone, fraction);
    }
    public override bool? CanHitNPCWithItem(Item item, NPC target) => Carries("animal_guidebook") && NPCID.Sets.CountsAsCritter[target.type] ? false : null;
    public override bool? CanHitNPCWithProj(Projectile proj, NPC target) => Carries("animal_guidebook") && NPCID.Sets.CountsAsCritter[target.type] ? false : null;

    public void EmpowerNebula() => nebulaPower = true;
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        // This is the source's HIGHEST-priority protection, before revival cooldowns.
        if (Has("the_cube") && lifeBeforeHurt >= 1.5f * PortConfig.Current.HealthAndDamageScale)
        { Player.statLife = BridgeUnits.Health(1); return false; }
        if (Has("enigmatic_item") || Carries("enigmatic_item")) { Player.statLife = BridgeUnits.Health(1); return false; }
        if (Has("void_pearl") && Main.rand.NextFloat() < 0.35f || Player.HeldItem.ModItem is ImportedItem held && held.ImportId == "the_infinitum" && Worthy && Main.rand.NextFloat() < 0.85f)
        { Player.statLife = BridgeUnits.Health(1); return false; }
        if (Has("cosmic_scroll") && RevivalCooldown <= 0)
        {
            RevivalCooldown = 600 * 60; Revive(0.5f); return false;
        }
        if (Has("the_cube"))
        {
            if (SpellCooldown <= 0 && LegacyActions.RandomTeleport(Player))
            {
                SpellCooldown = Has("cosmic_scroll") ? 4800 : 9600; Revive(0.3f); return false;
            }
        }
        return true;
    }
    private void Revive(float fraction)
    {
        Player.statLife = Math.Max(1, (int)(Player.statLifeMax2 * fraction));
        LegacyStatus.Apply(Player, ModContent.BuffType<RegenerationIII>(), 3600);
        LegacyStatus.Apply(Player, ModContent.BuffType<ResistanceII>(), 3600);
        LegacyStatus.Apply(Player, ModContent.BuffType<LegacyFireResistance>(), 3600);
        LegacyStatus.Apply(Player, ModContent.BuffType<LegacyStrength>(), 3600);
        Player.HealEffect(Player.statLife);
    }
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Player.whoAmI != Main.myPlayer) return;
        Player.GetModPlayer<LegacyNarrator>().DeathFromCreature = LegacyDamage.IsCreatureSource(damageSource);
        LegacyWorld.CaptureDeath(Player);
        LastDeath = Player.position; LastDeathWorld = LegacyRealms.KeyAt(Player.Center);
        if (Cursed && LostSouls < 9)
        {
            LostSouls++;
            LegacyWorld.AddSoul(Player, Player.Center);
        }
        if (Has("escape_scroll")) RemoveImportedItems("escape_scroll");
        LegacyNetwork.SendState(this);
    }
    public override void OnRespawn()
    {
        if (Player.whoAmI != Main.myPlayer) return;
        var items = RetainedItems.ToArray(); RetainedItems.Clear();
        foreach (var item in items) if (!item.IsAir) Player.QuickSpawnItem(Player.GetSource_Misc("ELRetainedInventory"), item, item.stack);
        LegacyNetwork.SendState(this);
    }
    internal void RemoveImportedItems(params string[] ids)
    {
        foreach (var item in Player.inventory.Concat(Player.armor).Concat(equipped.Values.Select(x => x.Item)).ToArray())
            if (item.ModItem is ImportedItem imported && ids.Contains(imported.ImportId)) item.TurnToAir();
        foreach (string id in ids) { equipped.Remove(id); inventory.Remove(id); }
    }
    public override void OnEnterWorld()
    {
        if (!GrantedGifts && PortConfig.Current.StartingGifts && Player.whoAmI == Main.myPlayer)
        {
            GrantedGifts = true;
            var amulet = new Item(ImportCatalog.Type("unwitnessed_amulet"));
            LegacyAmulets.Bind(amulet, Player);
            Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), amulet);
            Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), ImportCatalog.Type("the_acknowledgment"));
            Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), ImportCatalog.Type("cursed_ring"));
        }
        LegacyNetwork.SendState(this);
    }
    public override void SaveData(TagCompound tag)
    {
        tag["owner"] = OwnerId; tag["bound"] = BoundCurses; tag["desolation"] = BoundDesolation;
        tag["forbidden"] = AteForbiddenFruit; tag["astral"] = AteAstralFruit; tag["gifts"] = GrantedGifts;
        tag["souls"] = LostSouls; tag["ichor"] = DrankIchor;
        tag["firstChestReward"] = FirstChestReward; tag["firstMoonLordReward"] = FirstMoonLordReward;
        tag["playTicks"] = PlayTicks; tag["cursedTicks"] = CursedTicks;
        tag["death"] = LastDeath; tag["deathWorld"] = LastDeathWorld; tag["revival"] = RevivalCooldown;
        tag["spellCooldown"] = SpellCooldown;
        tag["retainedItems"] = RetainedItems.Select(ItemIO.Save).ToList(); tag["mirrorCooldown"] = MirrorCooldown;
    }
    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("owner")) OwnerId = tag.GetString("owner");
        BoundCurses = tag.GetBool("bound"); BoundDesolation = tag.GetBool("desolation");
        AteForbiddenFruit = tag.GetBool("forbidden"); AteAstralFruit = tag.GetBool("astral"); GrantedGifts = tag.GetBool("gifts");
        DrankIchor = tag.GetBool("ichor");
        FirstChestReward = tag.GetBool("firstChestReward"); FirstMoonLordReward = tag.GetBool("firstMoonLordReward");
        LostSouls = Math.Clamp(tag.GetInt("souls"), 0, 9); PlayTicks = Math.Max(0, tag.GetLong("playTicks")); CursedTicks = Math.Clamp(tag.GetLong("cursedTicks"), 0, PlayTicks);
        LastDeath = tag.Get<Vector2>("death"); LastDeathWorld = tag.GetString("deathWorld");
        RevivalCooldown = Math.Clamp(tag.GetInt("revival"), 0, 36000); SpellCooldown = Math.Clamp(tag.GetInt("spellCooldown"), 0, 9600);
        RetainedItems = tag.GetList<TagCompound>("retainedItems").Select(ItemIO.Load).ToList(); MirrorCooldown = Math.Clamp(tag.GetInt("mirrorCooldown"), 0, 600);
    }
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) => LegacyNetwork.SendState(this, toWho, fromWho);
    internal void WriteState(BinaryWriter writer)
    {
        writer.Write(OwnerId); writer.Write(BoundCurses); writer.Write(BoundDesolation); writer.Write(AteForbiddenFruit); writer.Write(AteAstralFruit);
        writer.Write(DrankIchor); writer.Write((byte)LostSouls); writer.Write(PlayTicks); writer.Write(CursedTicks);
        writer.Write(SpellCooldown); writer.Write(RevivalCooldown); writer.Write(MagnetEnabled); writer.Write(ShieldHeld);
        writer.Write(LastDeath.X); writer.Write(LastDeath.Y); writer.Write(LastDeathWorld); writer.Write(MirrorCooldown);
        writer.Write(FirstChestReward); writer.Write(FirstMoonLordReward);
    }
    internal void ReadState(BinaryReader reader)
    {
        string owner = reader.ReadString(); if (Guid.TryParse(owner, out _)) OwnerId = owner;
        BoundCurses = reader.ReadBoolean(); BoundDesolation = reader.ReadBoolean(); AteForbiddenFruit = reader.ReadBoolean(); AteAstralFruit = reader.ReadBoolean();
        DrankIchor = reader.ReadBoolean(); LostSouls = Math.Min(9, (int)reader.ReadByte());
        PlayTicks = Math.Max(0, reader.ReadInt64()); CursedTicks = Math.Clamp(reader.ReadInt64(), 0, PlayTicks);
        SpellCooldown = Math.Clamp(reader.ReadInt32(), 0, 9600); RevivalCooldown = Math.Clamp(reader.ReadInt32(), 0, 36000);
        MagnetEnabled = reader.ReadBoolean(); ShieldHeld = reader.ReadBoolean();
        LastDeath = new Vector2(reader.ReadSingle(), reader.ReadSingle()); LastDeathWorld = reader.ReadString(); MirrorCooldown = Math.Clamp(reader.ReadInt32(), 0, 600);
        // Once earned, stale client snapshots cannot reset a character reward.
        FirstChestReward |= reader.ReadBoolean(); FirstMoonLordReward |= reader.ReadBoolean();
    }
}
