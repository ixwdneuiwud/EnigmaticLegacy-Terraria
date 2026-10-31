// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace EnigmaticLegacy.Systems;

// Brewing becomes native bottle-station crafting. Inputs absent from Terraria have explicit material/effect analogues.
public static class LegacyBrewing
{
    public static void AddFor(ImportedItem item)
    {
        string id = item.ImportId;
        if (id == "recall_potion") { Register(item, "recall", ItemID.BottledWater, ItemID.Fireblossom, ItemID.Amethyst); return; }
        if (id == "twisted_potion") { Register(item, "twisted", ImportCatalog.Type("recall_potion"), ImportCatalog.Type("twisted_heart")); return; }
        bool ultimate = id.StartsWith("ultimate_", StringComparison.Ordinal), common = id.StartsWith("common_", StringComparison.Ordinal);
        if ((!ultimate && !common) || item.Definition.Variant == "") return;
        string suffix = item.Definition.Delivery;
        string baseId = suffix == "" ? id : id[..^(suffix.Length + 1)];
        if (suffix == "splash") { Register(item, "splash", ImportCatalog.Type(baseId), ItemID.ExplosivePowder); return; }
        if (suffix == "lingering") { Register(item, "lingering", ImportCatalog.Type(baseId + "_splash"), ItemID.FragmentNebula); return; }
        int dust = ImportCatalog.Type("astral_dust");
        if (common)
        {
            switch (item.Definition.Variant)
            {
                case "haste": Register(item, "haste", ItemID.BottledWater, ItemID.Fireblossom, ItemID.CrystalShard); break;
                case "long_haste": Register(item, "long_haste", ImportCatalog.Type("common_haste"), ItemID.Wire); break;
                case "strong_haste": Register(item, "strong_haste", ImportCatalog.Type("common_haste"), ItemID.PixieDust); break;
                case "molten_heart": Register(item, "molten_heart", ItemID.ObsidianSkinPotion, ItemID.HellstoneBar); break;
                case "long_molten_heart": Register(item, "long_molten_heart", ImportCatalog.Type("common_molten_heart"), ItemID.Wire); break;
            }
            return;
        }
        switch (item.Definition.Variant)
        {
            case "haste":
                Register(item, "long_haste", ImportCatalog.Type("common_long_haste"), dust);
                Register(item, "strong_haste", ImportCatalog.Type("common_strong_haste"), dust); break;
            case "molten_heart": Register(item, "molten_heart", ImportCatalog.Type("common_long_molten_heart"), dust); break;
            case "night_vision": Register(item, "night_vision", ItemID.NightOwlPotion, dust); break;
            case "invisibility": Register(item, "invisibility", ItemID.InvisibilityPotion, dust); break;
            case "leaping": Register(item, "leaping", ItemID.FeatherfallPotion, dust); break;
            case "fire_resistance": Register(item, "fire_resistance", ItemID.ObsidianSkinPotion, dust); break;
            case "swiftness": Register(item, "swiftness", ItemID.SwiftnessPotion, dust); break;
            case "slowness": Register(item, "slowness", ItemID.SwiftnessPotion, ItemID.Deathweed, dust); break;
            case "turtle_master": Register(item, "turtle_master", ItemID.EndurancePotion, ItemID.Deathweed, dust); break;
            case "water_breathing": Register(item, "water_breathing", ItemID.GillsPotion, dust); break;
            case "healing": Register(item, "healing", ItemID.HealingPotion, dust); break;
            case "harming": Register(item, "harming", ItemID.HealingPotion, ItemID.Deathweed, dust); break;
            case "poison": Register(item, "poison", ItemID.BottledWater, ItemID.Stinger, dust); break;
            case "regeneration": Register(item, "regeneration", ItemID.RegenerationPotion, dust); break;
            case "strength": Register(item, "strength", ItemID.WrathPotion, dust); break;
            case "weakness": Register(item, "weakness", ItemID.BottledWater, ItemID.Deathweed, dust); break;
            case "slow_falling": Register(item, "slow_falling", ItemID.FeatherfallPotion, dust); break;
        }
    }
    private static void Register(ImportedItem item, string path, params int[] ingredients)
    {
        var recipe = item.CreateRecipe();
        foreach (int ingredient in ingredients) recipe.AddIngredient(ingredient);
        recipe.AddTile(TileID.Bottles);
        if (item.Definition.Gate != "") recipe.AddCondition(new Condition(Language.GetOrRegister("Mods.EnigmaticLegacy.Conditions.Cursed"), () => Main.LocalPlayer.GetModPlayer<LegacyPlayer>().Cursed));
        recipe.Register();
        ImportedRecipes.Record("adapted_brewing/" + item.ImportId + "/" + path, item.ImportId);
    }
}
