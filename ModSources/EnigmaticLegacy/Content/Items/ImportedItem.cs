// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Content.Items;

public abstract class ImportedItem : ModItem
{
    public abstract string ImportId { get; }
    public ImportedDefinition Definition => ImportCatalog.Get(Mod, ImportId);
    public override string Texture => Definition.Texture.StartsWith("Terraria/", StringComparison.Ordinal) ? Definition.Texture : "EnigmaticLegacy/" + Definition.Texture;
    public bool Disabled;
    public bool Tainted;
    public bool Blessed;
    public int EyeAwakeningTicks;
    public Vector2 BoundPosition;
    public string BoundWorld = "";
    public string Inscription = "";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = Definition.Kind == "material" ? 25 : 1;
        if (ImportId == "soul_crystal")
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
            ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            // The native world renderer dereferences itemAnimations for every
            // soul, even when its original texture contains a single frame.
            if (Definition.Frames <= 1) Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 1));
        }
        if (Definition.Frames > 1)
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(Definition.FrameTicks, Definition.Frames));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }
        if (ImportId == "enigmatic_elytra")
            ArmorIDs.Wing.Sets.Stats[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings)] = new WingStats(180, 9f, 2.5f);
    }
    public override void Load()
    {
        if (ImportId == "enigmatic_elytra") EquipLoader.AddEquipTexture(Mod, "EnigmaticLegacy/Assets/Armor/EnigmaticWings", EquipType.Wings, this);
        if (Definition.Kind == "armor")
        {
            EquipType type = ImportId.EndsWith("helmet") ? EquipType.Head : ImportId.EndsWith("chestplate") ? EquipType.Body : EquipType.Legs;
            EquipLoader.AddEquipTexture(Mod, "EnigmaticLegacy/Assets/Armor/etherium_" + type, type, this);
        }
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.maxStack = Definition.Kind == "material" || LegacyActions.MaterialOnly(ImportId) ? 9999 : 1;
        Item.value = Item.buyPrice(silver: 20);
        Item.rare = Definition.Gate == "eldritch" ? ItemRarityID.Red : Definition.Gate == "cursed" ? ItemRarityID.Purple : ItemRarityID.Orange;
        // Armor uses the native accessory prefix pool but remains restricted to armor slots.
        Item.accessory = Definition.Accessory || Definition.Kind == "armor";
        if (ImportId == "enigmatic_elytra") Item.wingSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings);
        if (Definition.Kind == "armor")
        {
            if (ImportId == "etherium_helmet") { Item.headSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head); Item.defense = BridgeUnits.Armor(4); }
            if (ImportId == "etherium_chestplate") { Item.bodySlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body); Item.defense = BridgeUnits.Armor(9); }
            if (ImportId == "etherium_leggings") { Item.legSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs); Item.defense = BridgeUnits.Armor(7); }
        }
        if (Definition.Kind == "boots") Item.defense = BridgeUnits.Armor(4);
        if (Definition.Kind == "weapon")
        {
            // Unspecified weapons retain prefix data; approved designs replace it below.
            Item.noMelee = true; Item.DamageType = DamageClass.Melee;
            Item.damage = BridgeUnits.Health(ImportId switch {
                "eldritch_pan" => 31, "the_infinitum" or "the_judgement" => 16,
                "etherium_axe" or "etherium_scythe" => 12, "forbidden_axe" or "etherium_sword" => 10,
                "astral_breaker" or "the_twist" => 9, "etherium_shovel" => 7, "etherium_pickaxe" => 6, _ => 8 });
            Item.useTime = Item.useAnimation = 30; Item.knockBack = 5;
            LegacyWeapons.Defaults(Item, ImportId);
        }
        if (Definition.Kind != "weapon" && !LegacyActions.MaterialOnly(ImportId) && (Definition.Kind == "utility" || LegacyActions.IsUsable(ImportId)))
        {
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = Item.useAnimation = 24;
            Item.UseSound = SoundID.Item4;
            Item.consumable = LegacyActions.IsConsumable(ImportId);
            if (Item.consumable) Item.maxStack = 999;
        }
        if (ImportId is "unholy_grail" or "forbidden_fruit")
        {
            Item.useTime = Item.useAnimation = 96;
            Item.useStyle = ImportId == "unholy_grail" ? ItemUseStyleID.DrinkLiquid : ItemUseStyleID.EatFood;
            Item.UseSound = ImportId == "unholy_grail" ? SoundID.Item3 : SoundID.Item2;
        }
        KnownBlockItems.Configure(Item, ImportId);
    }
    public override void UpdateAccessory(Player player, bool hideVisual) { if ((ImportId != "enigmatic_eye" || Tainted) && player.GetModPlayer<LegacyPlayer>().CanUse(Definition)) player.GetModPlayer<LegacyPlayer>().Equip(this); }
    public override void UpdateEquip(Player player) { var legacy = player.GetModPlayer<LegacyPlayer>(); if ((ImportId != "enigmatic_eye" || Tainted) && legacy.CanUse(Definition)) legacy.Equip(this); }
    public override void UpdateInventory(Player player)
    {
        if (player.whoAmI == Main.myPlayer && Main.GameUpdateCount % 120 == 0) player.GetModPlayer<LegacyResearch>().Observe(Type);
        if (ImportId == "enigmatic_eye" && EyeAwakeningTicks > 0 && --EyeAwakeningTicks == 0)
        {
            Tainted = true;
            if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                for (int n = 0; n < player.inventory.Length; n++)
                    if (ReferenceEquals(player.inventory[n], Item)) NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, n);
        }
        if (ImportId == "unwitnessed_amulet" && player.whoAmI == Main.myPlayer) { LegacyAmulets.Bind(Item, player); return; }
        player.GetModPlayer<LegacyPlayer>().Carry(ImportId);
        if (ImportId == "earth_heart" && player.GetModPlayer<LegacyPlayer>().Cursed) Tainted = true;
        if (ImportId == "enigmatic_eye" && Tainted) player.GetModPlayer<LegacyNarrator>().Unlock(180);
    }
    public override bool CanEquipAccessory(Player player, int slot, bool modded) => Definition.Kind != "armor" && (ImportId != "enigmatic_eye" || Tainted) && player.GetModPlayer<LegacyPlayer>().CanUse(Definition);
    public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
    {
        if (ImportId != "enigmatic_elytra") return;
        speed = player.controlJump ? 18 : 9;
        acceleration *= player.controlJump ? 2f : 1;
    }
    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        if (ImportId != "enigmatic_elytra") return;
        ascentWhenFalling = 0.85f; ascentWhenRising = 0.15f; maxCanAscendMultiplier = 1f; maxAscentMultiplier = 3f; constantAscend = 0.135f;
    }
    public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
    {
        if (equippedItem.ModItem is not ImportedItem a || incomingItem.ModItem is not ImportedItem b) return true;
        if (a.ImportId == b.ImportId) return false;
        if (!PortConfig.Current.AllowMultipleSpellstones && a.Definition.Kind == "spellstone" && b.Definition.Kind == "spellstone") return false;
        return !(IsAmulet(a.ImportId) && IsAmulet(b.ImportId));
    }
    private static bool IsAmulet(string id) => id.StartsWith("enigmatic_amulet", StringComparison.Ordinal) || id is "ascension_amulet" or "eldritch_amulet";
    public override bool CanUseItem(Player player) => player.GetModPlayer<LegacyPlayer>().CanUse(Definition) &&
        (Definition.Kind == "weapon" ? LegacyWeapons.CanUse(player, this) : LegacyActions.CanUse(player, this));
    public override bool AltFunctionUse(Player player) => LegacyWeapons.Journal(ImportId) || Definition.Kind != "weapon" && LegacyActions.IsUsable(ImportId);
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse == 2 && LegacyWeapons.Journal(ImportId)) return false;
        if (ImportId != "eldritch_pan") return true;
        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        return false;
    }
    public override float UseSpeedMultiplier(Player player) => ImportId == "eldritch_pan" ? 8f / LegacyWeapons.PanInterval(Item) : 1;
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer && player.altFunctionUse == 2 && LegacyWeapons.Journal(ImportId)) LegacyJournal.Open();
        if (player.whoAmI == Main.myPlayer && Definition.Kind != "weapon" && (LegacyActions.IsUsable(ImportId) || Definition.Kind == "utility")) LegacyActions.Use(player, this);
        return true;
    }
    public override void HoldItem(Player player)
    {
        player.GetModPlayer<LegacyPlayer>().Carry(ImportId);
        if (ImportId == "eldritch_pan" && LegacyWeapons.Qualified(player, ImportId))
            player.statDefense += BridgeUnits.Armor(4 + Item.GetGlobalItem<ItemMetadata>().UniqueKills.Count * 0.5f);
    }
    public override bool? CanHitNPC(Player player, NPC target) => Definition.Kind == "weapon" &&
        (!LegacyWeapons.Qualified(player, ImportId) || Item.noMelee || player.altFunctionUse == 2) ? false : null;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        if (ImportId == "eldritch_pan" && LegacyWeapons.Qualified(player, ImportId))
        {
            damage.Base += LegacyWeapons.PanDamageBonus(Item);
        }
    }
    public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) => LegacyWeapons.ModifyHit(player, ImportId, target, ref modifiers);
    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) => LegacyWeapons.OnHit(player, ImportId, target, damageDone, Item);
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (ImportId != "enigmatic_eye" || !Tainted && EyeAwakeningTicks == 0) return true;
        Texture2D texture = ModContent.Request<Texture2D>(LegacyEyeVisual.Texture(this)).Value;
        Rectangle source = LegacyEyeVisual.Frame(this, texture.Width, texture.Height, (long)(Main.GlobalTimeWrappedHourly * 60));
        spriteBatch.Draw(texture, position, source, drawColor, 0, source.Size() / 2, scale, SpriteEffects.None, 0);
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (ImportId != "enigmatic_eye" || !Tainted && EyeAwakeningTicks == 0) return true;
        Texture2D texture = ModContent.Request<Texture2D>(LegacyEyeVisual.Texture(this)).Value;
        Rectangle source = LegacyEyeVisual.Frame(this, texture.Width, texture.Height, (long)(Main.GlobalTimeWrappedHourly * 60));
        spriteBatch.Draw(texture, Item.Center - Main.screenPosition, source, lightColor, rotation, source.Size() / 2, scale, SpriteEffects.None, 0);
        return false;
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var progress = new List<TooltipLine>();
        if (ImportId == "eldritch_pan")
        {
            int growth = LegacyWeapons.PanGrowth(Item);
            progress.Add(new(Mod, "GrowthProgress", BridgeUnits.Text($"成长进度：{growth}/100 种生物", $"Growth: {growth}/100 creature species")));
            progress.Add(new(Mod, "GrowthDamage", BridgeUnits.Text($"成长伤害加成：+{LegacyWeapons.PanDamageBonus(Item):0}", $"Growth damage bonus: +{LegacyWeapons.PanDamageBonus(Item):0}")));
            progress.Add(new(Mod, "GrowthCapacity", BridgeUnits.Text($"同时投掷物上限：{LegacyWeapons.PanCapacity(Item)}", $"Maximum active throws: {LegacyWeapons.PanCapacity(Item)}")));
        }
        RelicTooltips.Append(Mod, Item, ImportId, tooltips, LegacyWeapons.Implemented(ImportId), progress);
    }
    public override bool CanPickup(Player player) => ImportId != "soul_crystal";
    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (ImportId != "soul_crystal" || BoundWorld.Length == 0) return;
        gravity = 0; maxFallSpeed = 0; Item.velocity = Vector2.Zero;
        Item.Center = BoundPosition + new Vector2(0, MathF.Sin(Main.GlobalTimeWrappedHourly * 2) * 4);
    }
    public override void AddRecipes() { ImportedRecipes.AddFor(this); LegacyBrewing.AddFor(this); }
    public override void SaveData(TagCompound tag)
    {
        tag["disabled"] = Disabled; tag["tainted"] = Tainted; tag["bound"] = BoundPosition; tag["world"] = BoundWorld; tag["inscription"] = Inscription;
        tag["blessed"] = Blessed;
        if (EyeAwakeningTicks > 0) tag["eyeAwakening"] = EyeAwakeningTicks;
    }
    public override void LoadData(TagCompound tag)
    {
        Disabled = tag.GetBool("disabled"); Tainted = tag.GetBool("tainted"); BoundPosition = tag.Get<Vector2>("bound"); BoundWorld = tag.GetString("world"); Inscription = tag.GetString("inscription");
        Blessed = tag.GetBool("blessed");
        EyeAwakeningTicks = Math.Clamp(tag.GetInt("eyeAwakening"), 0, 12);
    }
    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Disabled); writer.Write(Tainted); writer.Write(BoundPosition.X); writer.Write(BoundPosition.Y); writer.Write(BoundWorld); writer.Write(Inscription);
        writer.Write(Blessed);
        writer.Write((byte)EyeAwakeningTicks);
    }
    public override void NetReceive(BinaryReader reader)
    {
        Disabled = reader.ReadBoolean(); Tainted = reader.ReadBoolean();
        BoundPosition = new Vector2(reader.ReadSingle(), reader.ReadSingle()); BoundWorld = reader.ReadString(); Inscription = reader.ReadString();
        Blessed = reader.ReadBoolean();
        EyeAwakeningTicks = Math.Clamp((int)reader.ReadByte(), 0, 12);
    }
}
