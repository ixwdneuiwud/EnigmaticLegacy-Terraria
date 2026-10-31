// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace EnigmaticLegacy.Content.Items;

public static class KnownBlockItems
{
    public static bool Configure(Item item, string id)
    {
        int tile = id switch {
            "big_lamp" => ModContent.TileType<OrdinaryLampTile>(), "big_shroomlamp" => ModContent.TileType<ShroomLampTile>(),
            "massive_lamp" => ModContent.TileType<EncasedLampTile>(), "massive_shroomlamp" => ModContent.TileType<EncasedShroomLampTile>(),
            "end_anchor" => ModContent.TileType<DimensionalAnchorTile>(), _ => -1 };
        if (tile < 0) return false;
        item.DefaultToPlaceableTile(tile); item.width = item.height = 32;
        if (id == "end_anchor") item.maxStack = 1;
        return true;
    }
}
public abstract class OriginalLampTile : ModTile
{
    protected abstract string SourceId { get; }
    protected virtual bool Solid => false;
    public override string Texture => "EnigmaticLegacy/Assets/Blocks/" + SourceId + "_tile";
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true; Main.tileLighted[Type] = true;
        Main.tileLavaDeath[Type] = false; Main.tileSolid[Type] = Solid;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.LavaDeath = false;
        if (!Solid)
        {
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, 1, 0);
            TileObjectData.addAlternate(0);
        }
        TileObjectData.addTile(Type);
        AddMapEntry(SourceId.Contains("shroom") ? new Color(230, 135, 80) : new Color(245, 216, 130));
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
        RegisterItemDrop(ImportCatalog.Type(SourceId));
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    { r = 1; g = SourceId.Contains("shroom") ? .65f : .9f; b = SourceId.Contains("shroom") ? .35f : .65f; }
}
public sealed class OrdinaryLampTile : OriginalLampTile { protected override string SourceId => "big_lamp"; }
public sealed class ShroomLampTile : OriginalLampTile { protected override string SourceId => "big_shroomlamp"; }
public sealed class EncasedLampTile : OriginalLampTile { protected override string SourceId => "massive_lamp"; protected override bool Solid => true; }
public sealed class EncasedShroomLampTile : OriginalLampTile { protected override string SourceId => "massive_shroomlamp"; protected override bool Solid => true; }

public sealed class DimensionalAnchorTile : ModTile
{
    public override string Texture => "EnigmaticLegacy/Assets/Blocks/end_anchor_tile";
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = Main.tileSolid[Type] = Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type); MinPick = 200; MineResist = 3;
        RegisterItemDrop(ImportCatalog.Type("end_anchor")); AddMapEntry(new Color(120, 60, 145));
    }
    public static int Charges(int x, int y) => WorldGen.InWorld(x,y,2) && Main.tile[x,y].HasTile && Main.tile[x,y].TileType == ModContent.TileType<DimensionalAnchorTile>() ? Math.Clamp(Main.tile[x,y].TileFrameX / 18, 0, 4) : 0;
    public static bool UseCharge(int x, int y)
    {
        int count = Charges(x,y); if (count == 0) return false;
        if (Main.rand.NextFloat() >= .35f) Main.tile[x,y].TileFrameX = (short)((count - 1) * 18);
        if (Main.netMode == NetmodeID.Server) NetMessage.SendTileSquare(-1,x,y,1);
        return true;
    }
    public override bool RightClick(int i, int j)
    {
        var player = Main.LocalPlayer;
        if (player.HeldItem.type == ItemID.Amethyst && Charges(i,j) < 4)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = Mod.GetPacket(); packet.Write((byte)11); packet.Write((short)i); packet.Write((short)j); packet.Write(false); packet.Send();
            }
            else Charge(player,i,j);
        }
        else if (Charges(i,j) > 0)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                var packet = Mod.GetPacket(); packet.Write((byte)11); packet.Write((short)i); packet.Write((short)j); packet.Write(true); packet.Send();
            }
            else Bind(player, i, j);
        }
        return true;
    }
    internal static void Charge(Player player, int x, int y)
    {
        if (player.HeldItem.type != ItemID.Amethyst || Charges(x,y) >= 4 || !Main.tile[x,y].HasTile ||
            Main.tile[x,y].TileType != ModContent.TileType<DimensionalAnchorTile>() || player.DistanceSQ(new Vector2(x*16+8,y*16+8)) > 160*160) return;
        Main.tile[x,y].TileFrameX += 18;
        if (player.difficulty != 3 && --player.HeldItem.stack <= 0) player.HeldItem.TurnToAir();
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1,x,y,1);
            NetMessage.SendData(MessageID.SyncEquipment,-1,-1,null,player.whoAmI,player.selectedItem);
        }
    }
    internal static void Bind(Player player, int x, int y)
    {
        if (Charges(x, y) == 0 || player.DistanceSQ(new Vector2(x * 16 + 8, y * 16 + 8)) > 160 * 160) return;
        var anchor = player.GetModPlayer<Players.LegacyAnchor>();
        anchor.Position = new Point(x, y); anchor.World = Main.ActiveWorldFileData.UniqueId.ToString();
        if (Main.netMode == NetmodeID.Server) anchor.SyncPlayer(player.whoAmI, -1, false);
        else Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, player.Center);
    }
    public override bool CanExplode(int i, int j) => false;
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    { float light=Charges(i,j)/4f; r=.65f*light; g=.15f*light; b=.8f*light; }
}
