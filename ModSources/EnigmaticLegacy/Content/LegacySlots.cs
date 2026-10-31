// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace EnigmaticLegacy.Content;

public abstract class LegacySlot : ModAccessorySlot, IRelicAccessorySlot
{
    protected abstract string Category { get; }
    public string AccessoryCategory => Category == "any" ? "curio" : Category;
    public override Vector2? CustomLocation => RelicSlotGrid.Location(this);
    public override bool DrawVanitySlot => !VanityItem.IsAir;
    public override bool DrawDyeSlot => !DyeItem.IsAir;
    public override bool IsVisibleWhenNotEnabled() => !FunctionalItem.IsAir || !VanityItem.IsAir || !DyeItem.IsAir;
    public override void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) => RelicSlotGrid.DrawGlyph(FunctionalTexture, context, item, position, isHovered);
    public override string FunctionalTexture => "EnigmaticLegacy/Assets/Slots/" + (Category == "any" ? "curio" : Category);
    public override string VanityTexture => FunctionalTexture;
    public override string DyeTexture => "EnigmaticLegacy/Assets/Slots/cosmetic";
    public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => context == AccessorySlotType.FunctionalSlot && AccessoryChannels.AcceptsUniversalItem(checkItem) &&
        (Category == "any" || BridgeExtensions.Category(checkItem) == Category);
    public override void OnMouseHover(AccessorySlotType context) => Main.hoverItemName = BridgeUnits.Text("神秘遗物：", "Enigmatic Legacy: ") + BridgeUnits.Text(Category switch { "spellstone" => "术石", "ring" => "戒指", "charm" => "护符", "scroll" => "卷轴", "boots" => "以太靴", _ => Category }, Category);
}
public sealed class SpellstoneSlot : LegacySlot
{
    protected override string Category => "spellstone";
    public override bool IsEnabled() => PortConfig.Current.DedicatedSpellstoneSlot;
}
public sealed class AstralRingSlot : LegacySlot
{
    // Preserve the saved slot identity; the fruit now unlocks a universal slot.
    protected override string Category => "any";
    public override bool CanAcceptItem(Item checkItem, AccessorySlotType context) => context == AccessorySlotType.FunctionalSlot && AccessoryChannels.AcceptsUniversalItem(checkItem);
    public override void OnMouseHover(AccessorySlotType context) => Main.hoverItemName = BridgeUnits.Text("天体果实：通用饰品栏", "Astral Fruit: accessory slot");
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().AteAstralFruit;
}
public sealed class IchorCharmSlot : LegacySlot
{
    protected override string Category => "charm";
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().DrankIchor;
}
public sealed class InsigniaCharmSlot : LegacySlot
{
    protected override string Category => "charm";
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().Has("insignia");
}
public sealed class CosmicScrollSlot : LegacySlot
{
    protected override string Category => "scroll";
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().Has("cosmic_scroll");
}
public sealed class EyeCharmSlot : LegacySlot
{
    protected override string Category => "charm";
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().Has("enigmatic_eye");
}
public sealed class EnchanterCharmSlot : LegacySlot
{
    protected override string Category => "charm";
    public override bool IsEnabled() => Player.GetModPlayer<LegacyPlayer>().Has("enchanter_pearl") && Player.GetModPlayer<LegacyPlayer>().Cursed;
}
public sealed class EtheriumBootSlot : LegacySlot
{
    protected override string Category => "boots";
    public override bool IsEnabled() => true;
}
