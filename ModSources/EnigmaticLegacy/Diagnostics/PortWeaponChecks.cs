// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Projectiles;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortWeaponChecks
{
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        int previousMode = Main.netMode;
        Main.netMode = NetmodeID.SinglePlayer;
        player.ResetEffects(); legacy.ResetEffects();
        foreach (var item in player.inventory.Concat(player.armor)) item.TurnToAir();
        legacy.BoundCurses = true; legacy.PlayTicks = legacy.CursedTicks = 1000;
        player.selectedItem = 0; player.altFunctionUse = 0; player.statLife = player.statLifeMax2 = 500;
        var weapons = ImportCatalog.Items.Values.Where(d => d.Kind == "weapon" && LegacyWeapons.Implemented(d.Id)).Select(d => new Item(ImportCatalog.Type(d.Id))).ToArray();
        check("Ten approved base weapons use native attack styles and qualify correctly", weapons.Length == 10 && weapons.All(i => i.useStyle > 0 && i.ModItem.CanUseItem(player)));
        check("Every weapon projectile resolves its texture resource", weapons.Where(i => i.shoot > 0).All(i => {
            string texture = ContentSamples.ProjectilesByType[i.shoot].ModProjectile.Texture;
            return texture.StartsWith("EnigmaticLegacy/") && (ImportCatalog.Owner.FileExists(texture[16..] + ".png") || ImportCatalog.Owner.FileExists(texture[16..] + ".rawimg"));
        }));
        var vanilla = ContentSamples.ItemsByType.Values.Where(i => i.type < ItemID.Count).ToArray();
        var external = ContentSamples.ItemsByType.Values.Where(i => i.ModItem?.Mod != ImportCatalog.Owner).ToArray();
        Item Tool(string id) => new(ImportCatalog.Type(id));
        var pick = Tool("etherium_pickaxe"); var axe = Tool("etherium_axe"); var breaker = Tool("astral_breaker");
        check("Etherium tool power stays just above vanilla even with Calamity loaded", pick.pick == 231 && axe.axe * 5 == 180 && Tool("etherium_shovel").pick == pick.pick);
        check("Astral Breaker matches the highest loaded mining tools", breaker.pick == Math.Max(external.Max(i => i.pick), pick.pick) && breaker.axe == Math.Max(external.Max(i => i.axe), axe.axe));
        details.Add($"Tool powers: original vanilla 230 pick/175% axe; loaded vanilla {vanilla.Max(i => i.pick)}/{vanilla.Max(i => i.axe) * 5}%; external {external.Max(i => i.pick)}/{external.Max(i => i.axe) * 5}%; Etherium {pick.pick}/{axe.axe * 5}%; Astral Breaker {breaker.pick}/{breaker.axe * 5}%.");
        details.Add("External maximum pick tools: " + string.Join(", ", external.Where(i => i.pick == external.Max(x => x.pick)).Select(i => i.ModItem?.FullName ?? ItemID.Search.GetName(i.type))));
        details.Add("External maximum axe tools: " + string.Join(", ", external.Where(i => i.axe == external.Max(x => x.axe)).Select(i => i.ModItem?.FullName ?? ItemID.Search.GetName(i.type))));
        Point mine = player.Center.ToTileCoordinates(); mine.X += 12; mine.Y += 10;
        var tile = Main.tile[mine.X, mine.Y]; tile.HasTile = true; tile.TileType = TileID.Stone;
        player.inventory[0] = pick;
        for (int n = 0; n < 4 && Main.tile[mine.X, mine.Y].HasTile; n++) player.PickTile(mine.X, mine.Y, pick.pick);
        check("Etherium pick mines actual native tiles through Player.PickTile", !Main.tile[mine.X, mine.Y].HasTile);

        var nativeShoot = typeof(Player).GetMethod("ItemCheck_Shoot", BindingFlags.Instance | BindingFlags.NonPublic);
        var created = new List<int>();
        int npcIndex = Main.maxNPCs - 1; NPC savedNpc = Main.npc[npcIndex];
        var npc = new NPC { whoAmI = npcIndex }; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = npcIndex;
        npc.active = true; npc.Center = player.Center + new Vector2(120, 0); npc.lifeMax = npc.life = 100000; npc.defense = 0;
        Main.npc[npcIndex] = npc;
        Projectile Spawn(Item weapon)
        {
            int p = Projectile.NewProjectile(player.GetSource_ItemUse(weapon), npc.Center, Vector2.UnitX * weapon.shootSpeed,
                weapon.shoot, player.GetWeaponDamage(weapon), weapon.knockBack, player.whoAmI);
            created.Add(p); return Main.projectile[p];
        }
        try
        {
            var twist = Tool("the_twist"); var infinitum = Tool("the_infinitum"); var pan = Tool("eldritch_pan");
            check("An ungrown pan adds no weapon damage bonus", player.GetWeaponDamage(pan) == pan.damage);
            foreach (var weapon in new[] { twist, infinitum, pan })
            {
                player.inventory[0] = weapon;
                var attack = Spawn(weapon);
                var modifiers = new NPC.HitModifiers { DamageType = DamageClass.Melee };
                modifiers.SourceDamage *= 1.3f; modifiers.FinalDamage *= 0.8f;
                legacy.ModifyHitNPC(npc, ref modifiers);
                attack.ModProjectile.ModifyHitNPC(npc, ref modifiers);
                check($"{((ImportedItem)weapon.ModItem).ImportId} fully bypasses only the fourth curse on its own attack",
                    Math.Abs(modifiers.SourceDamage.ApplyTo(100) * modifiers.FinalDamage.ApplyTo(1) - 104) < 0.01f);
                attack.active = false;
            }
            // A future access provider may grant a book without applying the
            // seven curses. Removing an absent penalty must not double damage.
            Func<Player, string, bool> access = (p, gate) => p == player && gate == "cursed";
            BridgeExtensions.AccessProviders.Add(access);
            try {
                legacy.BoundCurses = false;
                var unpenalized = new NPC.HitModifiers { DamageType = DamageClass.Melee };
                LegacyWeapons.ModifyHit(player, "the_twist", npc, ref unpenalized);
                check("Alternate weapon access does not compensate an absent fourth curse", Math.Abs(unpenalized.FinalDamage.ApplyTo(100) - 100) < .01f);
            }
            finally { BridgeExtensions.AccessProviders.Remove(access); legacy.BoundCurses = true; }
            player.inventory[0] = twist;
            int before = Main.projectile.Count(p => p.active && p.owner == player.whoAmI && p.type == twist.shoot);
            nativeShoot.Invoke(player, new object[] { player.whoAmI, twist, player.GetWeaponDamage(twist) });
            var emitted = Main.projectile.Where(p => p.active && p.owner == player.whoAmI && p.type == twist.shoot).ToArray();
            check("Native item shooting emits the Twist projectile without consuming mana", emitted.Length == before + 1 && twist.mana == 0);
            foreach (var p in emitted) { created.Add(p.whoAmI); p.active = false; }
            player.altFunctionUse = 2;
            nativeShoot.Invoke(player, new object[] { player.whoAmI, twist, player.GetWeaponDamage(twist) });
            check("Right-click journal does not emit an attack", !Main.projectile.Any(p => p.active && p.owner == player.whoAmI && p.type == twist.shoot));
            player.altFunctionUse = 0;
            var bolt = Spawn(twist);
            // Switch away after launch: the projectile must retain its source effects.
            player.inventory[0] = new Item(ItemID.CopperShortsword);
            var hit = new NPC.HitModifiers { DamageType = DamageClass.Melee };
            npc.boss = true; legacy.ModifyHitNPC(npc, ref hit); bolt.ModProjectile.ModifyHitNPC(npc, ref hit);
            check("Twist source keeps boss bonus and curse exemption after switching items", Math.Abs(hit.SourceDamage.ApplyTo(100) * hit.FinalDamage.ApplyTo(1) - 400) < 0.01f);
            var unrelated = new NPC.HitModifiers { DamageType = DamageClass.Summon };
            player.inventory[0] = infinitum; legacy.ModifyHitNPC(npc, ref unrelated);
            check("Holding Infinitum cannot remove the curse from unrelated summon attacks", Math.Abs(unrelated.FinalDamage.ApplyTo(100) - 50) < 0.01f);
            npc.boss = false; npc.life = 100000;
            bolt.Center = npc.Center; bolt.Damage(); int first = npc.life; bolt.Damage();
            check("A piercing book bolt deals actual damage only once per target", first < 100000 && npc.life == first);
            bolt.active = false;
            npc.immune[player.whoAmI] = 0; Array.Clear(npc.buffType); Array.Clear(npc.buffTime);
            player.inventory[0] = infinitum; var star = Spawn(infinitum);
            player.statLife = 100; player.lifeSteal = 80;
            star.Damage();
            details.Add($"Infinitum native hit: life {npc.life}, player {player.statLife}, worthy {legacy.Worthy}, curse {legacy.Cursed}, ticks {legacy.CursedTicks}/{legacy.PlayTicks}, immunity {star.localNPCImmunity[npcIndex]}, damage {star.damage}, buffs {string.Join(',', npc.buffType)}, steal {player.lifeSteal}, owner {star.owner}/{Main.myPlayer}.");
            check("Native Infinitum hits apply converted debuffs and heal the owning player", npc.HasBuff(ModContent.BuffType<WitheringIV>()) && npc.HasBuff(ModContent.BuffType<WeaknessIV>()) && npc.HasBuff(ModContent.BuffType<LegacySlowness>()) && npc.HasBuff(BuffID.Confused) && player.statLife > 100);
            int minionLife = player.statLife; player.moonLeech = true;
            LegacyWeapons.OnHit(player, "the_infinitum", npc, 100); player.moonLeech = false;
            check("Weapon lifesteal respects native Moon Leech", player.statLife == minionLife);
            player.statLife = 100; player.lifeSteal = 3;
            LegacyWeapons.OnHit(player, "the_infinitum", npc, 10000);
            details.Add($"Infinitum budget check: life {player.statLife}, budget {player.lifeSteal}, worthy {legacy.Worthy}, gate {((ImportedItem)infinitum.ModItem).Definition.Gate}, dummy {npc.type}, immortal {npc.immortal}, friendly {npc.friendly}, statue {npc.SpawnedFromStatue}, leech {player.moonLeech}, mode {Main.netMode}.");
            check("Weapon lifesteal consumes at most the native lifesteal budget", player.statLife == 103 && player.lifeSteal == 0);
            var originalRandom = Main.rand;
            try
            {
                int prevented = 0, allowed = 0; bool probabilitiesMatch = true;
                var expectedRandom = new Terraria.Utilities.UnifiedRandom(42197);
                Main.rand = new Terraria.Utilities.UnifiedRandom(42197);
                for (int trial = 0; trial < 200; trial++)
                {
                    bool expected = expectedRandom.NextFloat() < .85f, sound = true, dust = true;
                    var reason = PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral("isolated Infinitum lethal check"));
                    player.statLife = 0;
                    bool death = legacy.PreKill(99999, 0, false, ref sound, ref dust, ref reason);
                    probabilitiesMatch &= death == !expected && (death || player.statLife == BridgeUnits.Health(1));
                    if (death) allowed++; else prevented++;
                }
                check("Infinitum 85-percent lethal protection exercises both exact probability outcomes without a cooldown", probabilitiesMatch && prevented > 0 && allowed > 0 && legacy.RevivalCooldown == 0);
                legacy.BoundCurses = false;
                bool unworthySound = true, unworthyDust = true;
                var unworthyReason = PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral("unqualified fixture"));
                check("Infinitum lethal protection requires original worthiness", legacy.PreKill(99999, 0, false, ref unworthySound, ref unworthyDust, ref unworthyReason));
                legacy.BoundCurses = true;
                // Exercise the real Hurt -> PlayerLoader.PreKill path on the
                // disposable diagnostics player, never the user's character.
                bool nativeSaved = false;
                for (int seed = 0; seed < 8 && !nativeSaved; seed++)
                {
                    Main.rand = new Terraria.Utilities.UnifiedRandom(seed);
                    player.dead = false; player.statLife = 10; player.immune = false; player.immuneTime = 0;
                    Array.Clear(player.hurtCooldowns);
                    player.Hurt(PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral("Infinitum native lethal fixture")), 99999, 0, dodgeable: false);
                    nativeSaved = !player.dead && player.statLife == BridgeUnits.Health(1);
                }
                check("Infinitum protects against a real native lethal Hurt event", nativeSaved);
                player.dead = false; player.statLife = 100;
            }
            finally { Main.rand = originalRandom; }
            npc.lifeRegen = 0; int dot = 0; npc.GetGlobalNPC<LegacyGlobalNPC>().UpdateLifeRegen(npc, ref dot);
            check("Wither IV harms NPCs at its own rank", npc.lifeRegen == -BridgeUnits.Health(4) * 2);
            star.active = false; Array.Clear(npc.buffType); Array.Clear(npc.buffTime); npc.immune[player.whoAmI] = 0;

            player.inventory[0] = pan; var thrown = Spawn(pan);
            // Mimic an inventory move that clones the item while a pan is in flight.
            player.inventory[0] = pan.Clone(); var moved = player.inventory[0]; pan.TurnToAir();
            int damageBefore = player.GetWeaponDamage(moved);
            npc.lifeMax = 100; npc.life = 1; thrown.Center = npc.Center; thrown.Damage();
            details.Add($"Pan native hit: NPC life {npc.life}, active {npc.active}, worthy {legacy.Worthy}, immunity {thrown.localNPCImmunity[npcIndex]}, damage {thrown.damage}, identity {moved.GetGlobalItem<ItemMetadata>().WeaponIdentity}, growth {moved.GetGlobalItem<ItemMetadata>().UniqueKills.Count}.");
            check("A thrown pan kill grows the originating item across inventory moves", moved.GetGlobalItem<ItemMetadata>().UniqueKills.Count == 1 && player.GetWeaponDamage(moved) > damageBefore);
            var state = ItemIO.Load(ItemIO.Save(moved));
            check("Pan identity and unique kills persist through native item saves", state.GetGlobalItem<ItemMetadata>().WeaponIdentity == moved.GetGlobalItem<ItemMetadata>().WeaponIdentity && state.GetGlobalItem<ItemMetadata>().UniqueKills.SetEquals(moved.GetGlobalItem<ItemMetadata>().UniqueKills));
            using (var stream = new MemoryStream())
            {
                using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true);
                moved.GetGlobalItem<ItemMetadata>().NetSend(moved, writer); stream.Position = 0;
                var received = Tool("eldritch_pan"); received.GetGlobalItem<ItemMetadata>().NetReceive(received, new BinaryReader(stream));
                check("Pan per-instance state survives item networking", received.GetGlobalItem<ItemMetadata>().UniqueKills.SetEquals(moved.GetGlobalItem<ItemMetadata>().UniqueKills));
            }
            npc.life = 0; npc.active = true;
            LegacyWeapons.OnHit(player, "eldritch_pan", npc, 10, moved);
            check("Repeated kills of one species do not add growth", moved.GetGlobalItem<ItemMetadata>().UniqueKills.Count == 1);
            npc.SetDefaults(NPCID.Zombie); npc.whoAmI = npcIndex; npc.life = 0; npc.SpawnedFromStatue = true;
            LegacyWeapons.OnHit(player, "eldritch_pan", npc, 10, moved); npc.SpawnedFromStatue = false; npc.realLife = 1;
            LegacyWeapons.OnHit(player, "eldritch_pan", npc, 10, moved);
            check("Statues and subordinate segments cannot farm pan growth", moved.GetGlobalItem<ItemMetadata>().UniqueKills.Count == 1);
            player.statDefense = new Player.DefenseStat(); moved.ModItem.HoldItem(player);
            check("Pan armor grows with its own saved species count", player.statDefense == BridgeUnits.Armor(4.5f));
            for (int n = 0; n < 100; n++) moved.GetGlobalItem<ItemMetadata>().UniqueKills.Add("Test/" + n);
            // Loading old or forged oversized state remains limited to 100 entries.
            moved = ItemIO.Load(ItemIO.Save(moved)); player.inventory[0] = moved;
            check("Pan saved growth is limited to one hundred species", moved.GetGlobalItem<ItemMetadata>().UniqueKills.Count == 100);
            thrown.active = false;

            npc.active = false; player.inventory[0] = moved;
            var returning = Spawn(moved); returning.Center = player.Center + new Vector2(100, -80);
            returning.velocity = Vector2.UnitX * 22; returning.tileCollide = false;
            Vector2 initialVelocity = returning.velocity;
            for (int n = 0; n < 70 && returning.active; n++) returning.Update(returning.whoAmI);
            check("The pan executes returning throw AI", !returning.active || returning.ai[0] != 0 || returning.velocity != initialVelocity);
            check("Pan growth increases damage, projectile capacity and throwing speed", moved.damage == 650 && LegacyWeapons.PanCapacity(moved) == 12 && LegacyWeapons.PanInterval(moved) == 3 && player.GetWeaponDamage(moved) == 2650);
            var fresh = Tool("eldritch_pan");
            check("Fresh pan keeps its initial damage, speed and capacity", fresh.damage == 650 && LegacyWeapons.PanInterval(fresh) == 8 && LegacyWeapons.PanCapacity(fresh) == 2);
            for (int n = 0; n <= 100; n += 10)
            {
                fresh.GetGlobalItem<ItemMetadata>().UniqueKills.Clear();
                for (int j = 0; j < n; j++) fresh.GetGlobalItem<ItemMetadata>().UniqueKills.Add("Growth/" + j);
                check("Pan ten-species capacity step " + n, LegacyWeapons.PanCapacity(fresh) == 2 + n / 10 && fresh.ModItem.UseSpeedMultiplier(player) >= 1);
            }
            legacy.BoundCurses = false;
            check("Cursed weapons and their lingering projectiles cannot bypass qualification", !twist.ModItem.CanUseItem(player) && !infinitum.ModItem.CanUseItem(player) && star.ModProjectile.CanHitNPC(npc) == false);
            legacy.BoundCurses = true;

            details.Add("Weapon balance: Twist 38/24 ticks, Infinitum 1960/8 ticks, Pan 650/8 ticks; source-specific boss multipliers, immunity and native lifesteal limits exercised. Stationary headless checks do not establish an all-boss DPS ranking.");
        }
        finally
        {
            foreach (int p in created) Main.projectile[p].active = false;
            Main.npc[npcIndex] = savedNpc; player.altFunctionUse = 0; legacy.BoundCurses = false;
            Main.netMode = previousMode;
            player.ResetEffects(); legacy.ResetEffects();
            foreach (var item in player.inventory.Concat(player.armor)) item.TurnToAir();
        }
    }
}
