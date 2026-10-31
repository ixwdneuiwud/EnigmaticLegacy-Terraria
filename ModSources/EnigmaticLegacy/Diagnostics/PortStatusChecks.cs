// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortStatusChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        int oldMode = Main.netMode; Main.netMode = NetmodeID.SinglePlayer;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        void Reset(params string[] ids)
        {
            foreach (var item in player.inventory.Concat(player.armor)) item.TurnToAir();
            Array.Clear(player.buffType); Array.Clear(player.buffTime); Array.Clear(player.buffImmune);
            player.ResetEffects(); legacy.ResetEffects(); legacy.BoundCurses = false;
            legacy.PostUpdateBuffs();
            legacy.RevivalCooldown = legacy.SpellCooldown = 0;
            legacy.LostSouls = 0;
            player.statLife = player.statLifeMax = player.statLifeMax2 = 400;
            foreach (string id in ids) legacy.Equip((ImportedItem)new Item(ImportCatalog.Type(id)).ModItem);
        }
        bool Buff(string name, int ticks) => Enumerable.Range(0, player.buffType.Length).Any(i =>
            player.buffTime[i] == ticks && BuffLoader.GetBuff(player.buffType[i])?.Name == name);
        Reset(); player.statLife = 100;
        int regen = ModContent.BuffType<StrongRegeneration>(); player.AddBuff(regen, 75);
        int index = player.FindBuffIndex(regen); ModContent.GetInstance<StrongRegeneration>().Update(player, ref index);
        legacy.PostUpdateEquips();
        check("Regeneration II heals on its original 1.25 second pulse", player.statLife == 100 + BridgeUnits.Health(1));
        Reset(); player.statLife = player.statLifeMax2 = 100; player.AddBuff(regen, 75);
        index = player.FindBuffIndex(regen); ModContent.GetInstance<StrongRegeneration>().Update(player, ref index);
        player.statLifeMax2 = 400; legacy.PostUpdateEquips();
        check("Regeneration uses maximum life after equipment rather than the earlier base maximum", player.statLife == 100 + BridgeUnits.Health(1));
        Reset(); player.AddBuff(ModContent.BuffType<LegacyStrength>(), 4800); legacy.PostUpdateEquips();
        check("Strength II adds six source points to melee without affecting magic", player.GetDamage(DamageClass.Melee).Base == BridgeUnits.Health(6) && Math.Abs(player.GetTotalDamage(DamageClass.Magic).ApplyTo(100) - 100) < 0.01f);
        Reset(); player.statLife = 100; player.AddBuff(regen, 75); player.AddBuff(ModContent.BuffType<RegenerationIV>(), 72);
        index = player.FindBuffIndex(regen); ModContent.GetInstance<StrongRegeneration>().Update(player, ref index);
        index = player.FindBuffIndex(ModContent.BuffType<RegenerationIV>()); ModContent.GetInstance<RegenerationIV>().Update(player, ref index);
        legacy.PostUpdateEquips();
        check("Only the strongest regeneration rank heals each pulse", player.statLife == 100 + BridgeUnits.Health(1));
        Reset(); player.AddBuff(ModContent.BuffType<LegacyStrength>(), 4000); player.AddBuff(ModContent.BuffType<StrengthIV>(), 1200);
        legacy.PostUpdateEquips();
        check("Strength IV replaces the II bonus and preserves the weaker duration", player.GetDamage(DamageClass.Melee).Base == BridgeUnits.Health(12) && player.buffTime[player.FindBuffIndex(ModContent.BuffType<LegacyStrength>())] == 4000);
        Reset("cursed_scroll"); legacy.BoundCurses = true; player.statLife = 100;
        var broken = new Item(ItemID.IronBroadsword); broken.Prefix(PrefixID.Broken); player.inventory[0] = broken; player.selectedItem = 0;
        player.AddBuff(regen, 75);
        for (int pulse = 0; pulse < 5; pulse++)
        {
            player.ResetEffects(); legacy.ResetEffects();
            player.statLifeMax = player.statLifeMax2 = 400; // Temporary character has no consumed Life Crystals.
            index = player.FindBuffIndex(regen); ModContent.GetInstance<StrongRegeneration>().Update(player, ref index);
            if (pulse == 0) details.Add($"Regeneration pulse input: rank {LegacyStatus.Level(player, LegacyEffect.Regeneration)}, time {player.buffTime[index]}, owner {player.whoAmI}/{Main.myPlayer}, mode {Main.netMode}, dead {player.dead}, life {player.statLife}/{player.statLifeMax2}.");
            legacy.Equip((ImportedItem)new Item(ImportCatalog.Type("cursed_scroll")).ModItem); legacy.PostUpdateEquips();
        }
        check("Regeneration pulses retain fractional doubled curse bonuses after equipment updates", player.statLife == 100 + (int)MathF.Round(BridgeUnits.Health(1) * 5 * 1.64f));
        details.Add($"Ranked regeneration curse test: life {player.statLife}; negative prefixes {LegacyPrefixes.CurseCount(player)}, total active curses {LegacyPrefixes.TotalCurses(player)}.");

        var npc = Main.npc[197]; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = 197; npc.active = true;
        Array.Clear(npc.buffImmune); npc.life = npc.lifeMax = 1000;
        npc.AddBuff(regen, 75); index = npc.FindBuffIndex(regen); npc.life = 500;
        ModContent.GetInstance<StrongRegeneration>().Update(npc, ref index);
        check("Splash regeneration actually heals NPCs", npc.life == 500 + BridgeUnits.Health(1));
        npc.AddBuff(ModContent.BuffType<LegacyStrength>(), 4800);
        var state = npc.GetGlobalNPC<LegacyGlobalNPC>();
        var hurt = new Player.HurtModifiers(); state.ModifyHitPlayer(npc, player, ref hurt);
        check("NPC Strength II affects contact source damage without rewriting base damage", hurt.SourceDamage.Flat == BridgeUnits.Health(6));
        npc.AddBuff(ModContent.BuffType<LegacySlowness>(), 3600);
        npc.velocity = new Vector2(10, 0); state.PostAI(npc);
        bool slowed = Math.Abs(npc.velocity.X - 4) < 0.001f;
        for (int i = 0; i < 10; i++) { state.PreAI(npc); state.PostAI(npc); }
        check("NPC Slowness IV slows movement without multiplying away velocity every frame", slowed && Math.Abs(npc.velocity.X - 4) < 0.001f);
        npc.boss = true; state.PreAI(npc); npc.velocity = new Vector2(10, 0); state.PostAI(npc);
        check("Status movement adjustments preserve boss AI velocity", npc.velocity.X == 10);
        npc.boss = false; npc.realLife = 0; state.PreAI(npc); npc.velocity = new Vector2(10, 0); state.PostAI(npc);
        check("Status movement adjustments preserve multi-segment AI velocity", npc.velocity.X == 10);
        npc.realLife = -1; Array.Clear(npc.buffType); Array.Clear(npc.buffTime);
        npc.AddBuff(ModContent.BuffType<LegacyTurtleMaster>(), 2400);
        var incoming = new NPC.HitModifiers(); state.ModifyIncomingHit(npc, ref incoming);
        check("NPC Turtle Master applies resistance IV through native hit modifiers", Math.Abs(incoming.FinalDamage.ApplyTo(100) - 20) < 0.01f);
        Array.Clear(npc.buffType); Array.Clear(npc.buffTime); npc.lavaImmune = false;
        npc.AddBuff(ModContent.BuffType<MoltenHeart>(), 100); npc.AddBuff(BuffID.OnFire, 100);
        state.ResetEffects(npc);
        check("NPC Molten Heart grants temporary lava immunity and clears ordinary fire", npc.lavaImmune && !npc.HasBuff(BuffID.OnFire));
        Array.Clear(npc.buffType); Array.Clear(npc.buffTime); state.ResetEffects(npc);
        check("NPC fire protection expires without leaving permanent lava immunity", !npc.lavaImmune);
        npc.friendly = true; npc.life = 500;
        LegacyPotions.Apply(npc, player, ImportCatalog.Get("ultimate_regeneration"));
        index = npc.FindBuffIndex(regen); if (index >= 0) ModContent.GetInstance<StrongRegeneration>().Update(npc, ref index);
        check("Helpful splash potions can heal eligible friendly NPCs", index >= 0 && npc.life == 500 + BridgeUnits.Health(1));
        Main.netMode = NetmodeID.MultiplayerClient; int syncedLife = npc.life;
        if (index >= 0) ModContent.GetInstance<StrongRegeneration>().Update(npc, ref index);
        check("NPC regeneration does not run again on multiplayer clients", npc.life == syncedLife);
        Main.netMode = NetmodeID.SinglePlayer;
        npc.active = false;

        Reset();
        LegacyActions.ExecuteUtility(player, (ImportedItem)new Item(ImportCatalog.Type("astral_fruit")).ModItem, player.Center, false);
        check("Astral fruit grants original IV regeneration IV strength and III resistance", legacy.AteAstralFruit && Buff("RegenerationIV", 9000) && Buff("StrengthIV", 12000) && Buff("ResistanceIII", 9000));
        Reset();
        LegacyActions.ExecuteUtility(player, (ImportedItem)new Item(ImportCatalog.Type("ichor_bottle")).ModItem, player.Center, false);
        check("Ichor bottle grants original III regeneration V absorption and III resistance", legacy.DrankIchor && Buff("RegenerationIII", 3600) && Buff("AbsorptionV", 2400) && Buff("ResistanceIII", 4800));
        check("Absorption V starts with twenty converted source health", legacy.AbsorptionShield == BridgeUnits.Health(20));
        var absorbed = new Player.HurtInfo { Damage = 40, Dodgeable = true, DamageSource = PlayerDeathReason.ByOther(0) };
        legacy.AbsorbDamage(ref absorbed);
        check("Absorption does not spend health before free dodges resolve", !absorbed.Cancelled && legacy.AbsorptionShield == BridgeUnits.Health(20));
        check("A fully absorbed hit consumes only its final damage", legacy.ConsumableDodge(absorbed) && legacy.AbsorptionShield == BridgeUnits.Health(20) - 40);
        LegacyStatus.Apply(player, ModContent.BuffType<AbsorptionV>(), 4800);
        check("Refreshing absorption duration does not refill spent temporary health", legacy.AbsorptionShield == BridgeUnits.Health(20) - 40);
        hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        absorbed = new Player.HurtInfo { Damage = 100, Dodgeable = true, DamageSource = PlayerDeathReason.ByOther(0) };
        legacy.AbsorbDamage(ref absorbed);
        bool unchangedUntilHit = legacy.AbsorptionShield == BridgeUnits.Health(20) - 40 && absorbed.Damage == 100 - (BridgeUnits.Health(20) - 40);
        legacy.OnHurt(absorbed);
        check("Partial absorption spends temporary health only when the hit lands", unchangedUntilHit && legacy.AbsorptionShield == 0 && player.statLife == 400);
        player.ClearBuff(ModContent.BuffType<AbsorptionV>()); legacy.PostUpdateBuffs();
        LegacyStatus.Apply(player, ModContent.BuffType<AbsorptionV>(), 100);
        player.ClearBuff(ModContent.BuffType<AbsorptionV>()); legacy.PostUpdateBuffs();
        check("Expired absorption leaves no temporary health behind", legacy.AbsorptionShield == 0);
        Reset(); player.AddBuff(ModContent.BuffType<ResistanceII>(), 100); player.AddBuff(ModContent.BuffType<ResistanceIII>(), 100);
        hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        check("Resistance III replaces II instead of stacking two reductions", Math.Abs(hurt.FinalDamage.ApplyTo(100) - 40) < 0.01f);
        Reset(); LegacyStatus.Apply(player, ModContent.BuffType<AbsorptionV>(), 100);
        LegacyStatus.Apply(player, ModContent.BuffType<ResistanceIII>(), 100);
        hurt = new Player.HurtModifiers { DamageSource = PlayerDeathReason.ByOther(0), Dodgeable = true };
        legacy.ModifyHurt(ref hurt); absorbed = hurt.ToHurtInfo(200, 0, 0, 0, false);
        int remaining = legacy.AbsorptionShield;
        check("Absorption runs after native resistance in the final damage pipeline", !absorbed.Cancelled && Math.Abs(absorbed.Damage - 80) <= 1 && legacy.ConsumableDodge(absorbed) && legacy.AbsorptionShield == remaining - absorbed.Damage);
        Reset("the_cube"); legacy.SpellCooldown = 100;
        player.statLife = BridgeUnits.Health(1.4f); hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        bool sound = true, dust = true; var reason = PlayerDeathReason.ByOther(0);
        check("Cube death threshold does not round 1.4 source health up to 1.5", legacy.PreKill(999, 0, false, ref sound, ref dust, ref reason));
        player.statLife = (int)Math.Ceiling(PortConfig.Current.HealthAndDamageScale * 1.5f);
        hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        check("Cube high-health protection leaves one source health", !legacy.PreKill(999, 0, false, ref sound, ref dust, ref reason) && player.statLife == BridgeUnits.Health(1));
        Reset("the_cube", "cosmic_scroll"); player.statLife = BridgeUnits.Health(2);
        hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        check("Cube high priority protection does not spend the cosmic revival cooldown", !legacy.PreKill(999, 0, false, ref sound, ref dust, ref reason) && legacy.RevivalCooldown == 0 && player.statLife == BridgeUnits.Health(1));
        Reset("cosmic_scroll");
        player.immune = false; player.immuneTime = 0;
        check("Cosmic revival grants the source levels for exactly 60 seconds", !legacy.PreKill(999, 0, false, ref sound, ref dust, ref reason) && player.statLife == 200 && Buff("RegenerationIII", 3600) && Buff("ResistanceII", 3600) && Buff("LegacyStrength", 3600) && Buff("LegacyFireResistance", 3600));
        check("Revival leaves native hurt immunity timing without adding two seconds", !player.immune && player.immuneTime == 0);
        Reset("the_cube"); player.statLife = BridgeUnits.Health(1);
        hurt = new Player.HurtModifiers(); legacy.ModifyHurt(ref hurt);
        string realm = LegacyRealms.At(player.Center); int localPlayer = Main.myPlayer;
        Main.netMode = Main.dedServ ? NetmodeID.Server : NetmodeID.SinglePlayer; Main.myPlayer = Main.maxPlayers;
        try
        {
            check("Cube low-health revival teleports restores thirty percent and grants source buffs", !legacy.PreKill(999, 0, false, ref sound, ref dust, ref reason) && player.statLife == 120 && legacy.SpellCooldown == 9600 && LegacyRealms.At(player.Center) != realm && Buff("RegenerationIII", 3600) && Buff("ResistanceII", 3600) && Buff("LegacyStrength", 3600) && Buff("LegacyFireResistance", 3600));
        }
        finally { Main.myPlayer = localPlayer; Main.netMode = NetmodeID.SinglePlayer; }
        Reset();
        Main.netMode = oldMode;
        details.Add("Status regressions compare source levels, pulse timing, NPC contact damage, stable movement and revival event priority against 2.30.0.");
    }
}
