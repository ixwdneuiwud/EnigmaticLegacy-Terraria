// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyGrave
{
    public string Id = Guid.NewGuid().ToString("N");
    public string Owner = "";
    public Vector2 Position;
    public int Souls;
    public List<Item> Items = new();
    public TagCompound Save() => new() { ["id"] = Id, ["owner"] = Owner, ["pos"] = Position, ["souls"] = Souls, ["items"] = Items.Select(ItemIO.Save).ToList() };
    public static LegacyGrave Load(TagCompound tag) => new()
    {
        Id = tag.ContainsKey("id") ? tag.GetString("id") : Guid.NewGuid().ToString("N"),
        Owner = tag.GetString("owner"), Position = tag.Get<Vector2>("pos"), Souls = tag.GetInt("souls"),
        Items = tag.GetList<TagCompound>("items").Select(ItemIO.Load).ToList()
    };
}
public sealed class LegacyWorld : ModSystem
{
    private static readonly List<LegacyGrave> Graves = new();
    private static bool visualsDirty;
    public static int GraveCount => Graves.Count;
    public override void ClearWorld() { Graves.Clear(); visualsDirty = false; LegacyNetwork.Reset(); }
    public override void SaveWorldData(TagCompound tag) => tag["legacyGraves"] = Graves.Select(g => g.Save()).ToList();
    public override void LoadWorldData(TagCompound tag)
    {
        Graves.Clear(); Graves.AddRange(tag.GetList<TagCompound>("legacyGraves").Select(LegacyGrave.Load));
    }
    private static LegacyGrave GraveFor(Player player, Vector2 position)
    {
        string owner = player.GetModPlayer<LegacyPlayer>().OwnerId;
        var existing = Graves.LastOrDefault(g => g.Souls == 0 && g.Owner == owner && Vector2.DistanceSquared(g.Position, position) < 64 * 64);
        if (existing != null) return existing;
        var grave = new LegacyGrave { Owner = owner, Position = position };
        Graves.Add(grave); visualsDirty = true; return grave;
    }
    public static void CaptureDeath(Player player)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (!legacy.Carries("cursed_stone") || !legacy.Cursed || !IsSeverancePool(player)) return;
        player.GetModPlayer<LegacyNarrator>().RingDestroyed = true;
        legacy.RemoveImportedItems("cursed_stone", "cursed_ring", "desolation_ring");
        legacy.BoundCurses = legacy.BoundDesolation = false;
    }
    internal static bool IsSeverancePool(Player player)
    {
        Point center = player.Center.ToTileCoordinates();
        if (LegacyRealms.At(player.Center) != LegacyRealms.Nether || !WorldGen.InWorld(center.X, center.Y, 8)) return false;
        bool Lava(int x, int y) => Main.tile[x, y].LiquidAmount >= 250 && Main.tile[x, y].LiquidType == LiquidID.Lava;
        if (!Lava(center.X, center.Y)) return false;
        int surface = center.Y;
        while (surface > 4 && Lava(center.X, surface - 1)) surface--;
        for (int x = center.X - 3; x <= center.X + 3; x++)
        {
            for (int y = surface; y < surface + 4; y++) if (!Lava(x, y)) return false;
            for (int y = surface - 2; y < surface; y++)
                if (Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType]) return false;
        }
        return true;
    }
    private static bool HasVesselAmulet(LegacyPlayer legacy) => legacy.Carries("enigmatic_amulet") || legacy.Carries("ascension_amulet") || legacy.Carries("eldritch_amulet") ||
        new[] { "aqua", "violet", "magenta", "green", "black", "blue" }.Any(c => legacy.Carries("enigmatic_amulet_" + c));
    public static bool CaptureDroppedItem(Item item, IEntitySource source)
    {
        if (source is not EntitySource_Death death || death.Entity is not Player player || player.whoAmI != Main.myPlayer) return false;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        bool escape = legacy.Has("escape_scroll");
        if (escape && item.ModItem is ImportedItem { ImportId: "escape_scroll" }) { item.TurnToAir(); return true; }
        Vector2 destination = escape ? LegacyActions.SpawnPosition(player) : player.Center;
        if (!HasVesselAmulet(legacy))
        {
            if (escape) item.Center = destination;
            return false;
        }
        // Runs only when vanilla actually creates a death drop. No inventory is cleared in PreKill, so another mod's revival cannot lose items.
        if (legacy.Has("eldritch_amulet"))
        {
            legacy.RetainedItems.Add(item.Clone());
        }
        else if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)2);
            packet.Write(escape); ItemIO.Send(item, packet, true, true); packet.Send();
        }
        else GraveFor(player, destination).Items.Add(item.Clone());
        visualsDirty = true;
        item.TurnToAir(); return true;
    }
    internal static void ReceiveDrop(BinaryReader reader, Player player)
    {
        bool escape = reader.ReadBoolean();
        Item item = ItemIO.Receive(reader, true, true);
        if (!player.active || item.IsAir || item.stack < 1 || item.stack > item.maxStack) return;
        var grave = GraveFor(player, escape ? LegacyActions.SpawnPosition(player) : player.Center);
        if (grave.Items.Count < 256) grave.Items.Add(item);
        visualsDirty = true;
    }
    public static void AddSoul(Player player, Vector2 position)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)3); packet.Write(false); packet.Send();
        }
        else CreateSoul(player, position);
        visualsDirty = true;
    }
    private static void CreateSoul(Player player, Vector2 position)
    {
        var grave = new LegacyGrave { Owner = player.GetModPlayer<LegacyPlayer>().OwnerId, Position = position, Souls = 1 };
        Graves.Add(grave); EnsureSoulItem(grave); visualsDirty = true;
    }
    private static void EnsureSoulItem(LegacyGrave grave)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || grave.Souls <= 0) return;
        foreach (Item current in Main.ActiveItems)
            if (current.ModItem is ImportedItem { ImportId: "soul_crystal" } soul && soul.BoundWorld == grave.Id) return;
        int index = Item.NewItem(new EntitySource_Misc("ELSoulCrystal"), grave.Position, ImportCatalog.Type("soul_crystal"), noBroadcast: true);
        if (index >= Main.maxItems) return;
        Item item = Main.item[index];
        var crystal = (ImportedItem)item.ModItem;
        crystal.BoundWorld = grave.Id; crystal.BoundPosition = grave.Position; crystal.Inscription = grave.Owner;
        item.velocity = Vector2.Zero; item.Center = grave.Position;
        if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index);
    }
    private static void RemoveSoulItem(string id)
    {
        foreach (Item item in Main.ActiveItems)
        {
            if (item.ModItem is not ImportedItem { ImportId: "soul_crystal" } soul || soul.BoundWorld != id) continue;
            int index = item.whoAmI; item.TurnToAir();
            if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index);
        }
    }
    internal static void ReceiveSoul(BinaryReader reader, Player player)
    {
        _ = reader.ReadBoolean();
        if (!player.active) return;
        CreateSoul(player, player.Center);
    }
    public static void ReclaimNearby(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || player.dead || player.statLife <= 0) return;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        foreach (var grave in Graves.ToArray())
        {
            if (grave.Owner != legacy.OwnerId || !LegacyRealms.Same(player.Center, grave.Position) || Vector2.DistanceSquared(player.Center, grave.Position) > 56 * 56) continue;
            // Remove ownership of the contents before returning them; pickup callbacks cannot reclaim the same grave twice.
            Graves.Remove(grave);
            RemoveSoulItem(grave.Id);
            visualsDirty = true;
            legacy.LostSouls = Math.Max(0, legacy.LostSouls - grave.Souls);
            foreach (var item in grave.Items) if (!item.IsAir) player.QuickSpawnItem(player.GetSource_Misc("ELVessel"), item, item.stack);
            LegacyNetwork.SendState(legacy);
        }
    }
    public override void NetSend(BinaryWriter writer) => WriteVisuals(writer);
    public override void NetReceive(BinaryReader reader) => ReadVisuals(reader);
    public static void WriteVisuals(BinaryWriter writer)
    {
        writer.Write(Graves.Count);
        foreach (var grave in Graves) { writer.Write(grave.Owner); writer.Write(grave.Position.X); writer.Write(grave.Position.Y); writer.Write((byte)grave.Souls); }
    }
    public static void ReadVisuals(BinaryReader reader)
    {
        Graves.Clear(); int count = reader.ReadInt32();
        if (count < 0 || count > 10000) throw new IOException("Invalid vessel count");
        for (int i = 0; i < count; i++) Graves.Add(new LegacyGrave { Owner = reader.ReadString(), Position = new Vector2(reader.ReadSingle(), reader.ReadSingle()), Souls = reader.ReadByte() });
    }
    public override void PostUpdateWorld()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            foreach (var grave in Graves) EnsureSoulItem(grave);
        if (!visualsDirty || Main.netMode != NetmodeID.Server) return;
        visualsDirty = false;
        var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)4); WriteVisuals(packet); packet.Send();
    }
    public override void PostDrawTiles()
    {
        if (Main.dedServ || Graves.Count == 0) return;
        var texture = ModContent.Request<Texture2D>("EnigmaticLegacy/Assets/Status/storage_crystal").Value;
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        foreach (var grave in Graves)
        {
            if (grave.Souls > 0 || grave.Items.Count == 0) continue;
            Vector2 screen = grave.Position - Main.screenPosition;
            if (screen.X < -64 || screen.Y < -64 || screen.X > Main.screenWidth + 64 || screen.Y > Main.screenHeight + 64) continue;
            Rectangle frame = new(0, 0, texture.Width, Math.Min(texture.Width, texture.Height));
            Main.spriteBatch.Draw(texture, screen + new Vector2(0, MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 4), frame, Color.White, 0, frame.Size() / 2, 1, SpriteEffects.None, 0);
        }
        Main.spriteBatch.End();
    }
}

public sealed class ImportedGlobalItem : GlobalItem
{
    public override void OnSpawn(Item item, IEntitySource source) => LegacyWorld.CaptureDroppedItem(item, source);
    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        if (item.ModItem is ImportedItem { ImportId: "soul_crystal" }) return;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (!legacy.MagnetEnabled || player.controlDown) return;
        if (legacy.Has("super_magnet_ring")) grabRange = Math.Max(grabRange, 256);
        else if (legacy.Has("magnet_ring")) grabRange = Math.Max(grabRange, 128);
    }
    public override bool GrabStyle(Item item, Player player)
    {
        if (item.ModItem is ImportedItem { ImportId: "soul_crystal" }) return false;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        if (!legacy.MagnetEnabled || player.controlDown) return false;
        if (legacy.Has("super_magnet_ring")) { item.Center = player.Center; item.velocity = Vector2.Zero; return true; }
        if (!legacy.Has("magnet_ring")) return false;
        item.velocity = Vector2.Lerp(item.velocity, (player.Center - item.Center).SafeNormalize(Vector2.Zero) * 12, 0.15f);
        return true;
    }
}
