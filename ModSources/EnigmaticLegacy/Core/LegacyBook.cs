// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace EnigmaticLegacy.Core;

public static class LegacyBook
{
    private static JsonObject book;
    private static JsonObject[] entries;
    private static Dictionary<int, string[]> discoveries;
    public static JsonObject Data => book ??= JsonNode.Parse(ImportCatalog.Owner.GetFileBytes("Data/source-book.json")).AsObject();
    public static JsonObject Local(JsonObject row) => row["localized"]?[Language.ActiveCulture.Name.StartsWith("zh") ? "zh_cn" : "en_us"]?.AsObject() ?? row;
    public static string Text(JsonNode value)
    {
        string text = (string)value ?? "";
        if (Data["macros"] is JsonObject macros)
            foreach (var (token, replacement) in macros) text = text.Replace(token, (string)replacement ?? "");
        text = text.Replace("$(br2)", "\n\n").Replace("$(br)", "\n").Replace("$(p)", "\n\n").Replace("$(li)", "\n• ").Replace("$(playername)", Main.LocalPlayer.name);
        return Regex.Replace(text, @"\$\([^)]*\)|§.", "");
    }
    public static int ItemType(string source)
    {
        if (string.IsNullOrEmpty(source)) return 0;
        string variant = Regex.Match(source, @"EnigmaticPotion:'([^']+)'").Groups[1].Value;
        string id = source.Split('#', '{')[0];
        if (variant.Length > 0) id = "enigmaticlegacy:" + (variant.StartsWith("ultimate_") ? variant : "common_" + variant);
        if (id.StartsWith("enigmaticlegacy:")) return ImportCatalog.TryType(id[16..], out int t) ? t : 0;
        return id.StartsWith("minecraft:") ? id[10..] switch {
            "writable_book" or "book" => ItemID.Book, "brewing_stand" => ItemID.AlchemyTable, _ => 0 } : 0;
    }
    public static JsonObject[] Entries => entries ??= Data["entries"].AsArray().Select(n => n.AsObject()).OrderBy(n => (int?)n["sortnum"] ?? 0).ToArray();
    public static IEnumerable<string> Discoveries(int type)
    {
        discoveries ??= Entries.GroupBy(e => ItemType((string)e["icon"]))
            .Where(g => g.Key > 0).ToDictionary(g => g.Key, g => g.Select(e => (string)e["id"]).ToArray());
        return discoveries.TryGetValue(type, out var ids) ? ids : Array.Empty<string>();
    }
    public static void Clear() { book = null; entries = null; discoveries = null; }
}
