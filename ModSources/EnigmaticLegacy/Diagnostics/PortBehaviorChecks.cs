// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Diagnostics;

internal static class PortBehaviorChecks
{
    private static ImportedItem Imported(string id) => (ImportedItem)new Item(ImportCatalog.Type(id)).ModItem;
    public static void Run(Player player, Action<string, bool> check, List<string> details)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        var world = ModContent.GetInstance<LegacyWorld>();
        var drop = typeof(Player).GetMethod("DropItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        void ClearEquipment()
        {
            legacy.ResetEffects();
            foreach (var item in player.inventory.Concat(player.armor)) item.TurnToAir();
        }
        List<LegacyGrave> Graves()
        {
            var state = new TagCompound(); world.SaveWorldData(state);
            return state.GetList<TagCompound>("legacyGraves").Select(LegacyGrave.Load).ToList();
        }
        int CopperCount() => Main.item.Where(i => i.active && i.type == ItemID.CopperBar).Sum(i => i.stack) + player.inventory.Where(i => i.type == ItemID.CopperBar).Sum(i => i.stack);
        ClearEquipment();
        legacy.PlayTicks = legacy.CursedTicks = 1000; legacy.BoundCurses = true;
        var amulet = Imported("eldritch_amulet"); player.armor[3] = amulet.Item; legacy.Equip(amulet);
        player.inventory[10] = new Item(ItemID.CopperBar, 13);
        Main.netMode = Main.dedServ ? NetmodeID.Server : NetmodeID.SinglePlayer; drop.Invoke(player, null);
        check("Abyssal amulet retains actual death drops", legacy.RetainedItems.Where(i => i.type == ItemID.CopperBar).Sum(i => i.stack) == 13 && LegacyWorld.GraveCount == 0);
        var playerState = new TagCompound(); legacy.SaveData(playerState); legacy.RetainedItems.Clear(); legacy.LoadData(playerState);
        int before = CopperCount(); legacy.OnRespawn(); int after = CopperCount(); legacy.OnRespawn();
        check("Retained drops persist and return once on respawn", after - before == 13 && CopperCount() == after && legacy.RetainedItems.Count == 0);

        ClearEquipment();
        var escape = Imported("escape_scroll"); player.armor[3] = escape.Item; legacy.Equip(escape);
        player.inventory[10] = Imported("enigmatic_amulet").Item; legacy.Carry("enigmatic_amulet");
        player.inventory[11] = new Item(ItemID.CopperBar, 17);
        player.position += new Vector2(1600, 0); player.statLife = 400;
        drop.Invoke(player, null); legacy.Kill(999, 0, false, PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral("isolated test")));
        var escaped = Graves();
        check("Escape scroll moves drops to spawn but leaves an independent soul at death", escaped.Count == 2 &&
            escaped.Any(g => Vector2.DistanceSquared(g.Position, LegacyActions.SpawnPosition(player)) < 1 && g.Items.Where(i => i.type == ItemID.CopperBar).Sum(i => i.stack) == 17) &&
            escaped.Any(g => Vector2.DistanceSquared(g.Position, player.Center) < 1 && g.Souls == 1 && g.Items.Count == 0) &&
            escaped.SelectMany(g => g.Items).All(i => i.type != ImportCatalog.Type("escape_scroll")) && !legacy.Has("escape_scroll"));
        Vector2 soulPosition = player.Center;
        player.position = LegacyActions.SpawnPosition(player); LegacyWorld.ReclaimNearby(player);
        player.Center = soulPosition; LegacyWorld.ReclaimNearby(player);

        Main.netMode = NetmodeID.SinglePlayer; ClearEquipment();
        const int x = 80, y = 80;
        void SetLiquid(int tx, int ty, byte amount, int type)
        {
            Tile tile = Main.tile[tx, ty]; tile.HasTile = false; tile.LiquidAmount = amount; tile.LiquidType = type;
        }
        for (int tx = x - 6; tx <= x + 6; tx++)
            for (int ty = y - 6; ty <= y + 6; ty++) SetLiquid(tx, ty, 0, LiquidID.Water);
        for (int tx = x; tx <= x + 5; tx++) SetLiquid(tx, y, 255, LiquidID.Water);
        SetLiquid(x, y - 1, 255, LiquidID.Honey);
        SetLiquid(x - 1, y, 255, LiquidID.Lava);
        SetLiquid(x, y + 1, 255, LiquidID.Shimmer);
        int absorbed = LegacyLiquids.AbsorbConnected(new Point(x, y), 4);
        check("Sponge respects connected radius and other liquids", absorbed == 5 && Main.tile[x + 5, y].LiquidAmount == 255 &&
            Main.tile[x, y - 1].LiquidAmount == 255 && Main.tile[x - 1, y].LiquidAmount == 255 && Main.tile[x, y + 1].LiquidAmount == 255);

        int poolY = Main.maxTilesY - 120;
        for (int tx = x - 4; tx <= x + 4; tx++)
            for (int ty = poolY - 3; ty <= poolY + 5; ty++)
            {
                SetLiquid(tx, ty, (byte)(ty >= poolY && ty < poolY + 4 ? 255 : 0), LiquidID.Lava);
            }
        player.Center = new Vector2(x * 16 + 8, poolY * 16 + 8);
        legacy.BoundCurses = legacy.BoundDesolation = true;
        var ring = Imported("cursed_ring"); player.armor[3] = ring.Item; legacy.Equip(ring);
        player.inventory[10] = Imported("cursed_stone").Item; legacy.Carry("cursed_stone");
        check("Severance requires a clear full-size Underworld lava pool", LegacyWorld.IsSeverancePool(player));
        LegacyWorld.CaptureDeath(player);
        check("Severance removes curse state and ritual items in the same tick", !legacy.Cursed && !legacy.BoundDesolation && player.armor[3].IsAir && player.inventory[10].IsAir);

        player.position = new Vector2(80 * 16, 64 * 16);
        for (int tx = 78; tx < 95; tx++) for (int ty = 58; ty < 75; ty++) SetLiquid(tx, ty, 0, LiquidID.Water);
        var npc = Main.npc[199]; npc.SetDefaults(NPCID.BlueSlime); npc.whoAmI = 199; npc.active = true;
        npc.Center = player.Center + new Vector2(48, 0);
        var eye = Imported("extradimensional_eye"); eye.BoundWorld = Main.worldID.ToString(); eye.BoundPosition = player.position + new Vector2(128, 0);
        Vector2 originalPlayer = player.position;
        bool transported = LegacyActions.TransportCreature(player, eye, npc.Center);
        check("Eye teleports a creature, consumes itself and leaves player in place", transported && eye.Item.IsAir && npc.position == eye.BoundPosition && player.position == originalPlayer);
        npc.boss = true; npc.Center = player.Center + new Vector2(48, 0);
        var protectedEye = Imported("extradimensional_eye"); protectedEye.BoundWorld = Main.worldID.ToString(); protectedEye.BoundPosition = player.position;
        check("Eye preserves boss AI and is not consumed on failure", !LegacyActions.TransportCreature(player, protectedEye, npc.Center) && !protectedEye.Item.IsAir);
        npc.active = false;

        ClearEquipment();
        foreach (string id in new[] { "etherium_helmet", "etherium_chestplate", "etherium_leggings", "etherium_boots" }) legacy.Equip(Imported(id));
        player.statLife = 160; player.statLifeMax2 = 400;
        check("Etherium shield activates at exactly 40 percent health", legacy.EtheriumShieldActive);
        player.statLife = 161;
        check("Etherium shield deactivates above threshold", !legacy.EtheriumShieldActive);
        legacy.Equip(Imported("cosmic_scroll")); player.statLife = 320;
        check("Cosmic scroll raises shield threshold to 80 percent", legacy.EtheriumShieldActive);

        var prefixArmor = Imported("etherium_helmet").Item; prefixArmor.Prefix(PrefixID.Warding); player.armor[0] = prefixArmor;
        Compatibility.Call(new object[] { "RegisterCursePrefix", (int)PrefixID.Warding, true });
        check("Other mods can register a negative prefix", LegacyPrefixes.CurseCount(player) == 1);
        Compatibility.Call(new object[] { "RegisterCursePrefix", (int)PrefixID.Warding, false });
        check("Prefix registration can be reversed", LegacyPrefixes.CurseCount(player) == 0);
        using (var stream = new MemoryStream())
        {
            legacy.BoundCurses = true; legacy.LostSouls = 3; legacy.MirrorCooldown = 400;
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) legacy.WriteState(writer);
            legacy.BoundCurses = false; legacy.LostSouls = 0; legacy.MirrorCooldown = 0;
            stream.Position = 0; using var reader = new BinaryReader(stream); legacy.ReadState(reader);
            check("Player state packet roundtrip preserves persistent state", legacy.BoundCurses && legacy.LostSouls == 3 && legacy.MirrorCooldown == 400);
        }
        ClearEquipment();
        float boundary = Main.UnderworldLayer * 16f;
        Vector2 above = new(1600, boundary - 1), below = new(1600, boundary + 1);
        check("Height distinguishes Overworld and Nether at the native boundary", LegacyRealms.At(above) == LegacyRealms.Overworld && LegacyRealms.At(below) == LegacyRealms.Nether);
        player.Center = above; string binding = LegacyRealms.KeyAt(player.Center);
        player.Center = below;
        check("Descending and ascending update logical realm immediately", !LegacyRealms.Matches(binding, above, player.Center) && LegacyRealms.At(player.Center) == LegacyRealms.Nether);
        var realmEye = Imported("extradimensional_eye"); realmEye.BoundWorld = binding; realmEye.BoundPosition = above - player.Size / 2;
        check("Eye cannot cross the Overworld Nether boundary", !LegacyActions.CanUse(player, realmEye));
        var savedEye = (ImportedItem)ItemIO.Load(ItemIO.Save(realmEye.Item)).ModItem;
        check("Bound realm survives item persistence", savedEye.BoundWorld == binding && !LegacyRealms.Matches(savedEye.BoundWorld, savedEye.BoundPosition, below));
        player.Center = above;
        check("Ascending restores access to same-realm bindings", LegacyActions.CanUse(player, realmEye));
        for (int tx = 97; tx <= 105; tx++) for (int ty = Main.UnderworldLayer; ty <= Main.UnderworldLayer + 10; ty++) SetLiquid(tx, ty, 0, LiquidID.Water);
        legacy.LastDeath = new Vector2(1600, boundary + 40); legacy.LastDeathWorld = LegacyRealms.KeyAt(legacy.LastDeath + player.Size / 2);
        var deathPotion = Imported("twisted_potion");
        check("Death potion retains original cross-realm permission", LegacyActions.CanUse(player, deathPotion));
        // Native teleport uses whoAmI == Main.myPlayer for camera/lighting work,
        // even in server mode. A dedicated server has no local player.
        Main.netMode = Main.dedServ ? NetmodeID.Server : NetmodeID.SinglePlayer;
        int localPlayer = Main.myPlayer;
        Main.myPlayer = Main.maxPlayers;
        try
        {
            LegacyActions.ExecuteUtility(player, deathPotion, player.Center, false);
            check("Death potion returns to recorded Nether and one health", LegacyRealms.At(player.Center) == LegacyRealms.Nether && player.position == legacy.LastDeath && player.statLife == 1);
            check("Cube travels from Nether to Overworld", LegacyActions.RandomTeleport(player) && LegacyRealms.At(player.Center) == LegacyRealms.Overworld);
            check("Cube travels from Overworld to Nether", LegacyActions.RandomTeleport(player) && LegacyRealms.At(player.Center) == LegacyRealms.Nether);
        }
        finally { Main.myPlayer = localPlayer; }
        player.statLife = 400; player.Center = below;
        LegacyWorld.AddSoul(player, player.Center); int soulsBefore = legacy.LostSouls;
        player.Center = above; LegacyWorld.ReclaimNearby(player);
        check("Nearby soul cannot be reclaimed from the other logical realm", LegacyWorld.GraveCount == 1 && legacy.LostSouls == soulsBefore);
        player.Center = below; LegacyWorld.ReclaimNearby(player);
        check("Returning to the soul realm enables reclamation", LegacyWorld.GraveCount == 0 && legacy.LostSouls == soulsBefore - 1);
        check("Other ports can query height-based realm through Mod Call", (string)Compatibility.Call(new object[] { "GetRealm", below }) == LegacyRealms.Nether);
        details.Add("Death inventory, binary world persistence, ritual, liquid and creature interaction checks ran only in the disposable test world.");
        Main.netMode = NetmodeID.SinglePlayer;
    }
}
