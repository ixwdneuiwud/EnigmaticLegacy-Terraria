// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content.Items;

// Stable source IDs also allow the expansion rituals to use these blocks without
// registering base-mod materials from an optional dependent mod.
public abstract class MaterialBlockItem : ModItem
{
    public abstract string SourceId { get; }
    public override string Name => SourceId;
    public override string Texture => "EnigmaticLegacy/Assets/Blocks/" + SourceId;
    public override void SetStaticDefaults()
    {
        if (SourceId == "etherium_block") Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 12));
    }
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(SourceId == "astral_block" ? ModContent.TileType<AstralBlockTile>() : ModContent.TileType<EtheriumBlockTile>());
        Item.width = Item.height = 16; Item.maxStack = 9999; Item.rare = ItemRarityID.Purple;
    }
    public override void AddRecipes() => ImportedRecipes.AddFor(this, SourceId, "");
}
public sealed class AstralDustBlock : MaterialBlockItem { public override string SourceId => "astral_block"; }
public sealed class EtheriumBlock : MaterialBlockItem { public override string SourceId => "etherium_block"; }

public sealed class AstralBlockTile : ModTile
{
    public override string Texture => "EnigmaticLegacy/Assets/Blocks/AstralTile";
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true; Main.tileBlockLight[Type] = true; Main.tileLighted[Type] = true;
        DustType = DustID.PurpleTorch; AddMapEntry(new Color(120, 60, 180)); RegisterItemDrop(ModContent.ItemType<AstralDustBlock>());
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = .25f; g = .08f; b = .4f; }
}
public sealed class EtheriumBlockTile : ModTile
{
    public override string Texture => "EnigmaticLegacy/Assets/Blocks/EtheriumTile";
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true; Main.tileBlockLight[Type] = true; Main.tileLighted[Type] = true;
        AnimationFrameHeight = 288; MinPick = 200; MineResist = 3;
        DustType = DustID.PurpleTorch; AddMapEntry(new Color(165, 115, 210)); RegisterItemDrop(ModContent.ItemType<EtheriumBlock>());
    }
    public override void AnimateTile(ref int frame, ref int frameCounter) { if (++frameCounter >= 6) { frameCounter = 0; frame = (frame + 1) % 12; } }
    public override bool CanExplode(int i, int j) => false;
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) { r = .3f; g = .15f; b = .4f; }
}
