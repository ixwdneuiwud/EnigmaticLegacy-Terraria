// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

public static class ImportedRecipes
{
    private static JsonArray source;
    private static JsonObject replacements;
    public static readonly Dictionary<string, string> Deferred = new();
    public static readonly Dictionary<string, string> RegisteredSources = new();
    public static int RegisteredCount { get; private set; }
    public static void Record(string sourceName, string itemId) { RegisteredSources[sourceName] = itemId; RegisteredCount++; }
    public static void Reset() { source = null; replacements = null; Deferred.Clear(); RegisteredSources.Clear(); RegisteredCount = 0; }
    public static void AddFor(ImportedItem item) => AddFor(item, item.ImportId, item.Definition.Gate);
    public static void AddFor(ModItem item, string importId, string gate)
    {
        if (importId == "mega_sponge") return; // Explicitly an unobtainable easter egg.
        if (source == null)
        {
            source = JsonNode.Parse(item.Mod.GetFileBytes("Data/recipes.json")).AsArray();
            foreach (var hidden in JsonNode.Parse(item.Mod.GetFileBytes("Data/hidden-recipes.json")).AsArray())
                source.Add(hidden.DeepClone());
        }
        replacements ??= JsonNode.Parse(item.Mod.GetFileBytes("Data/vanilla-map.json")).AsObject();
        foreach (JsonObject entry in source.OfType<JsonObject>())
        {
            JsonNode result = entry["result"];
            string resultId = result is JsonObject obj ? (string)obj["item"] : result is JsonValue value ? value.GetValue<string>() : "";
            if (resultId != "enigmaticlegacy:" + importId) continue;
            string recipeType = (string)entry["type"] ?? "";
            if (recipeType is not ("minecraft:crafting_shaped" or "minecraft:crafting_shapeless" or "minecraft:smelting" or "minecraft:blasting" or "enigmaticlegacy:crafting_shaped_cursed" or "enigmaticlegacy:crafting_shaped_hidden" or "enigmaticlegacy:shapeless_no_return_craft")) continue;
            var ingredients = new Dictionary<int, int>();
            bool valid = true;
            string reason = "";
            if (entry["pattern"] is JsonArray pattern && entry["key"] is JsonObject keys)
                foreach (var symbol in string.Concat(pattern.Select(p => (string)p)).Where(c => c != ' ').GroupBy(c => c))
                    valid &= AddIngredient(keys[symbol.Key.ToString()], symbol.Count(), ingredients, ref reason);
            else if (entry["ingredients"] is JsonArray shapeless)
                foreach (var ingredient in shapeless) valid &= AddIngredient(ingredient, 1, ingredients, ref reason);
            else if (entry["ingredient"] is JsonNode single) valid &= AddIngredient(single, 1, ingredients, ref reason);
            else valid = false;
            if (!valid || ingredients.Count == 0)
            { Deferred[(string)entry["sourceFile"] ?? importId] = reason; continue; }
            var recipe = item.CreateRecipe(result is JsonObject ? (int?)result["count"] ?? 1 : 1);
            foreach (var ingredient in ingredients)
                if (ingredient.Key == ImportCatalog.Type("enigmatic_amulet")) recipe.AddRecipeGroup("EnigmaticLegacy:PersonalAmulet", ingredient.Value);
                else if (VanillaGroup(ingredient.Key) is string group) recipe.AddRecipeGroup("EnigmaticLegacy:" + group, ingredient.Value);
                else recipe.AddIngredient(ingredient.Key, ingredient.Value);
            recipe.AddTile(recipeType is "minecraft:smelting" or "minecraft:blasting" ?
                importId == "etherium_ingot" ? TileID.AdamantiteForge : TileID.Furnaces : TileID.WorkBenches);
            if (gate != "" || recipeType == "enigmaticlegacy:crafting_shaped_cursed")
                recipe.AddCondition(new Condition(Language.GetOrRegister("Mods.EnigmaticLegacy.Conditions.Cursed", () => "Bearer of the Seven Curses"),
                    () => BridgeExtensions.HasAccess(Main.LocalPlayer, "cursed", "enigmaticlegacy:" + importId)));
            // The user maps the blessed heart to the ordinary Cosmic Heart;
            // the original heart ingredient already supplies the requirement.
            if (importId is "ascension_amulet" or "eldritch_amulet")
                recipe.AddOnCraftCallback((r, output, consumed, destination) => LegacyAmulets.PreserveOwner(output, consumed));
            recipe.Register(); Record((string)entry["sourceFile"] ?? importId, importId);
        }
    }
    private static string VanillaGroup(int type) => type switch
    {
        ItemID.IronBar => "IronBar", ItemID.IronOre => "IronOre", ItemID.GoldBar => "GoldBar", ItemID.GoldOre => "GoldOre",
        ItemID.GoldBroadsword => "GoldSword", ItemID.DemoniteBar => "EvilBar", _ => null
    };
    private static bool AddIngredient(JsonNode ingredient, int count, Dictionary<int, int> output, ref string reason)
    {
        if (ingredient is JsonArray alternatives)
        {
            foreach (var alternative in alternatives)
                if (Resolve(alternative, out int type, out int units)) { output[type] = checked(output.GetValueOrDefault(type) + count * units); return true; }
        }
        else if (Resolve(ingredient, out int type, out int units)) { output[type] = checked(output.GetValueOrDefault(type) + count * units); return true; }
        reason = ingredient?.ToJsonString() ?? "unsupported ingredient";
        return false;
    }
    private static bool Resolve(JsonNode ingredient, out int type, out int units)
    {
        type = 0; units = 1;
        if (ingredient is not JsonObject obj) return false;
        string id = (string)obj["item"] ?? (string)obj["tag"] ?? "";
        if (id == "minecraft:nether_star") return ImportCatalog.TryType("nether_star", out type);
        if (id.StartsWith("enigmaticlegacy:", StringComparison.Ordinal)) return ImportCatalog.TryType(id[16..], out type);
        if (!replacements.TryGetPropertyValue(id, out JsonNode replacement)) return false;
        string vanilla;
        if (replacement is JsonObject conversion)
        {
            vanilla = (string)conversion["item"];
            units = (int?)conversion["count"] ?? 1;
        }
        else vanilla = (string)replacement;
        return units > 0 && vanilla != null && ItemID.Search.TryGetId(vanilla, out type);
    }
}
