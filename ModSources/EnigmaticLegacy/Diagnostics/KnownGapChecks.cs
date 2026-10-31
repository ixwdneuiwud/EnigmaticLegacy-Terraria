// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Diagnostics;

internal static class KnownGapChecks
{
    public static void Run(Player p, Action<string,bool> check)
    {
        var legacy=p.GetModPlayer<LegacyPlayer>(); legacy.ResetEffects(); p.ResetEffects();
        p.Center=new Vector2(2300,(float)Main.rockLayer*16+1600); p.GetModPlayer<LegacyAim>().Direction=Vector2.UnitX;
        NPC Target(int index,int x,int y)
        {
            NPC npc=Main.npc[index]; npc.SetDefaults(NPCID.BlueSlime); npc.active=true;
            npc.Center=p.Center+new Vector2(x,y); Array.Clear(npc.buffType); Array.Clear(npc.buffTime); Array.Clear(npc.buffImmune);
            return npc;
        }
        int[] indices=Enumerable.Range(0,Main.maxNPCs).Where(i=>!Main.npc[i].active).Take(3).ToArray();
        if(indices.Length<3) { check("Gaze regression has three disposable target slots",false); return; }
        NPC first=Target(indices[0],100,0), second=Target(indices[1],200,32), behind=Target(indices[2],-100,0);
        for(int x=(int)p.Center.X/16-2;x<(int)p.Center.X/16+18;x++)
            for(int y=(int)p.Center.Y/16-5;y<(int)p.Center.Y/16+5;y++) { Tile clear = Main.tile[x,y]; clear.HasTile=false; }
        legacy.ApplyGaze();
        check("Eldritch gaze reaches multiple observed targets at rank II",first.HasBuff(ModContent.BuffType<WeaknessII>()) && second.HasBuff(ModContent.BuffType<SlownessII>()) && LegacyStatus.Level(first,LegacyEffect.Weakness)==2);
        check("Eldritch gaze excludes targets behind the viewing direction",!behind.HasBuff(ModContent.BuffType<WeaknessII>()));
        foreach(int index in indices) Main.npc[index].active=false;
        var research=p.GetModPlayer<LegacyResearch>(); research.Observe(ImportCatalog.Type("enigmatic_eye")); research.ReadPages.Add("enigmatic_eye:0");
        var state=new TagCompound(); research.SaveData(state); research.Discovered.Clear(); research.ReadPages.Clear(); research.LoadData(state);
        check("Original book imports every one of its 100 entries and 388 pages",LegacyBook.Entries.Length==100 && LegacyBook.Entries.Sum(n=>n["pages"].AsArray().Count)==388);
        check("Original book expands its source paragraph, percent and player-name macros", LegacyBook.Text(System.Text.Json.Nodes.JsonValue.Create("A<br2/>B<br/><percent/><playername><&5>C<clear/>")) == "A\n\nB\n%" + Main.LocalPlayer.name + "C");
        check("Original book discovery and read progress persist with the character",research.Discovered.Contains("enigmatic_eye") && research.ReadPages.Contains("enigmatic_eye:0"));
        check("All four original non-redstone lamps and Dimensional Anchor place native tiles",new[]{"big_lamp","massive_lamp","big_shroomlamp","massive_shroomlamp","end_anchor"}.All(id=>new Item(ImportCatalog.Type(id)).createTile>=TileID.Count));
        Point at=p.Center.ToTileCoordinates(); Tile tile=Main.tile[at.X,at.Y]; tile.HasTile=true; tile.TileType=(ushort)ModContent.TileType<DimensionalAnchorTile>(); tile.TileFrameX=54;
        p.inventory[0]=new Item(ItemID.Amethyst,2); p.selectedItem=0;
        DimensionalAnchorTile.Charge(p,at.X,at.Y);
        bool charged=DimensionalAnchorTile.Charges(at.X,at.Y)==4 && p.HeldItem.stack==1;
        DimensionalAnchorTile.Charge(p,at.X,at.Y);
        check("Dimensional Anchor stores four native tile-frame charges and consumes one mapped fuel",charged && p.HeldItem.stack==1 && DimensionalAnchorTile.Charges(at.X,at.Y)==4);
        var anchor=p.GetModPlayer<LegacyAnchor>(); anchor.Position=at; anchor.World=Main.ActiveWorldFileData.UniqueId.ToString();
        var anchorState=new TagCompound(); anchor.SaveData(anchorState); anchor.Position=default; anchor.World=""; anchor.LoadData(anchorState);
        check("Dimensional Anchor binding persists without replacing native bed data",anchor.Position==at && anchor.World==Main.ActiveWorldFileData.UniqueId.ToString());
        for (int x = at.X - 4; x <= at.X + 4; x++)
            for (int y = at.Y - 4; y < at.Y; y++) { Tile clear = Main.tile[x,y]; clear.HasTile = false; clear.LiquidAmount = 0; }
        p.Center += new Vector2(600, 0);
        check("Dimensional Anchor relocates into safe space after native spawn positioning",anchor.TryRespawn() && p.DistanceSQ(at.ToWorldCoordinates()) < 160 * 160);
        string world = anchor.World; anchor.World = "another-world"; Vector2 before = p.position;
        check("Dimensional Anchor cannot act in a different world",!anchor.TryRespawn() && p.position == before); anchor.World = world;
        tile.HasTile=false;
        var light=new LegacyLight(); light.Scan(p.Center,16); float dark=light.Brightness(p.Center);
        int shot=Projectile.NewProjectile(p.GetSource_Misc("DynamicLightFixture"),p.Center,Vector2.Zero,ProjectileID.WoodenArrowFriendly,0,0,p.whoAmI);
        Main.projectile[shot].light=1; light.Scan(p.Center,16);
        check("Darkness gameplay scan includes native dynamic projectile light",light.Brightness(p.Center)>dark+.1f);
        Main.projectile[shot].active=false;
        foreach (Item item in p.inventory) item.TurnToAir();
        Item stone = new(ImportCatalog.Type("void_stone")); p.inventory[0] = stone;
        var metadata = stone.GetGlobalItem<ItemMetadata>(); metadata.VoidFilters.Add("Terraria/" + ItemID.StoneBlock);
        p.inventory[1] = new Item(ItemID.StoneBlock, 4); p.inventory[2] = new Item(ItemID.StoneBlock, 8);
        metadata.VoidMode = 1; LegacyVoidConsumption.Consume(p, stone);
        check("Oblivion Stone Last Stack preserves the largest matching stack",p.inventory[1].IsAir && p.inventory[2].stack == 8);
        metadata.VoidMode = 2; LegacyVoidConsumption.Consume(p, stone);
        check("Oblivion Stone Excess Absorption waits for a full inventory",p.inventory[2].stack == 8);
        for (int n = 1; n < 50; n++) if (p.inventory[n].IsAir) p.inventory[n] = new Item(ItemID.DirtBlock);
        p.inventory[3] = new Item(ItemID.StoneBlock, 2); LegacyVoidConsumption.Consume(p, stone);
        check("Oblivion Stone Excess Absorption frees one smallest matching slot",p.inventory[3].IsAir && p.inventory[2].stack == 8);
        p.inventory[3] = new Item(ItemID.StoneBlock, 2) { favorited = true }; metadata.VoidMode = 0; LegacyVoidConsumption.Consume(p, stone);
        check("Oblivion Stone Total Absorption consumes inventory matches and protects favorites",p.inventory[2].IsAir && p.inventory[3].stack == 2);
        var metaState = new TagCompound(); metadata.VoidMode = 2; metadata.SaveData(stone, metaState); metadata.VoidMode = 0; metadata.LoadData(stone, metaState);
        check("Oblivion Stone mode and stable filter keys persist",metadata.VoidMode == 2 && metadata.VoidFilters.Contains("Terraria/" + ItemID.StoneBlock));
        foreach (Item item in p.inventory) item.TurnToAir(); legacy.ResetEffects();
    }
}

