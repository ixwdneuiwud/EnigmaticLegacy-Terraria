// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

// Associate drops with the actual miner, never a nearby bystander. Multiplayer
// tile drops are created on the server while it handles the native tile packet.
public sealed class LegacyMining : ModSystem
{
    private static MiningScope current;
    private static readonly HashSet<int> AdditionalOres = new();
    public static float DoubleOreChance(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        return (legacy.Cursed ? 0.1f : 0) + (legacy.Has("avarice_scroll") && legacy.Cursed ? 0.1f : 0) +
            (legacy.Has("the_cube") ? 0.1f : 0) + (legacy.Has("mining_charm") ? 0.1f : 0) + BridgeExtensions.Sum(BridgeExtensions.FortuneProviders, player);
    }
    public static bool IsOre(int type) => TileID.Sets.Ore[type] || AdditionalOres.Contains(type) ||
        type is TileID.Amethyst or TileID.Topaz or TileID.Sapphire or TileID.Emerald or TileID.Ruby or TileID.Diamond;
    public static void RegisterOre(int type) => AdditionalOres.Add(type);
    public override void Load()
    {
        On_Player.PickTile += PickTile;
        On_MessageBuffer.GetData += GetData;
    }
    public override void Unload() { current = null; AdditionalOres.Clear(); }
    private static void PickTile(On_Player.orig_PickTile orig, Player player, int x, int y, int power)
    {
        using var scope = Begin(player, x, y);
        orig(player, x, y, power);
    }
    private static void GetData(On_MessageBuffer.orig_GetData orig, MessageBuffer buffer, int start, int length, out int messageType)
    {
        using var scope = PacketScope(buffer, start, length);
        orig(buffer, start, length, out messageType);
    }
    internal static IDisposable PacketScope(MessageBuffer buffer, int start, int length)
    {
        if (Main.netMode != NetmodeID.Server || buffer.whoAmI < 0 || buffer.whoAmI >= Main.maxPlayers ||
            start < 0 || length < 9 || start > buffer.readBuffer.Length - 9 ||
            buffer.readBuffer[start] != MessageID.TileManipulation || buffer.readBuffer[start + 1] != 0) return null;
        int x = BinaryPrimitives.ReadInt16LittleEndian(buffer.readBuffer.AsSpan(start + 2, 2));
        int y = BinaryPrimitives.ReadInt16LittleEndian(buffer.readBuffer.AsSpan(start + 4, 2));
        return Begin(Main.player[buffer.whoAmI], x, y);
    }
    internal static IDisposable Begin(Player player, int x, int y)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || !WorldGen.InWorld(x, y, 1) ||
            !Main.tile[x, y].HasTile || !IsOre(Main.tile[x, y].TileType) || DoubleOreChance(player) <= 0) return null;
        return new MiningScope(player, new Point(x, y));
    }
    internal static bool ShouldDouble(IEntitySource source)
    {
        if (current == null || source is not EntitySource_TileBreak tile || tile.TileCoords != current.Point) return false;
        current.Success ??= Main.rand.NextFloat() < DoubleOreChance(current.Player);
        return current.Success.Value;
    }
    private sealed class MiningScope : IDisposable
    {
        private readonly MiningScope previous;
        internal readonly Player Player;
        internal readonly Point Point;
        internal bool? Success;
        internal MiningScope(Player player, Point point) { previous = current; Player = player; Point = point; current = this; }
        public void Dispose() => current = previous;
    }
}
