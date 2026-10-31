// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public sealed class ImportedDefinition
{
    public string Id = "";
    public string SourceClass = "";
    public string Name = "";
    public string EnglishName = "";
    public string Kind = "";
    public string Gate = "";
    public string Category = "";
    public string Texture = "";
    public string Variant = "";
    public string Delivery = "";
    public int Frames = 1;
    public int FrameTicks = 3;
    public Dictionary<string, object> Defaults = new();
    public bool Accessory => Kind is "accessory" or "spellstone" or "boots";
}

/// <summary>Immutable import metadata. Minecraft IDs are stable save identifiers.</summary>
public static class ImportCatalog
{
    public static IReadOnlyDictionary<string, ImportedDefinition> Items { get; private set; }
        = new Dictionary<string, ImportedDefinition>();
    public static Mod Owner { get; private set; }
    public static ImportedDefinition Get(Mod mod, string id) { Load(mod); return Items[id]; }
    public static ImportedDefinition Get(string id) => Items[id];
    public static int Type(string id) => Owner.Find<ModItem>(id).Type;
    public static bool TryType(string id, out int type)
    {
        if (Owner.TryFind<ModItem>(id, out var item)) { type = item.Type; return true; }
        if (BridgeExtensions.TryExternalItem("enigmaticlegacy:" + id, out type)) return true;
        type = 0;
        return false;
    }
    public static void Load(Mod mod)
    {
        if (Owner == mod && Items.Count > 0) return;
        Owner = mod;
        var items = JsonSerializer.Deserialize<List<ImportedDefinition>>(mod.GetFileBytes("Data/items.json"),
            new JsonSerializerOptions { IncludeFields = true, PropertyNameCaseInsensitive = true });
        Items = items.ToDictionary(i => i.Id, StringComparer.Ordinal);
    }
    public static void Unload()
    {
        Items = new Dictionary<string, ImportedDefinition>();
        Owner = null;
    }
}

public static class BridgeUnits
{
    public static int Health(float minecraft) => Math.Max(1, (int)MathF.Round(minecraft * PortConfig.Current.HealthAndDamageScale));
    public static int Armor(float minecraft) => Math.Max(0, (int)MathF.Round(minecraft * PortConfig.Current.ArmorScale));
    public static int Ticks(int minecraftTicks) => checked(minecraftTicks * 3);
    public static float Blocks(float minecraft) => minecraft * 16f;
    public static string Text(string chinese, string english) => Language.ActiveCulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? chinese : english;
    public static bool IsBoss(NPC npc) => npc.boss || (npc.realLife >= 0 && npc.realLife < Main.maxNPCs && Main.npc[npc.realLife].boss) || Compatibility.HasNpcTag(npc, "boss");
}
