// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public sealed class SourceTooltipRow
{
    public string Key = "", Zh = "", En = "", Guard = "", Color = "";
}
public sealed class SourceTooltipPage
{
    public List<SourceTooltipRow> Default = new(), Shift = new(), Alt = new();
}

// Use native TooltipLines so Recipe Browser, Better Tooltips and other tooltip
// extensions continue to run. Original keyboard layers are kept verbatim;
// native combat/tool stats and saved growth progress are displayed separately.
public static class RelicTooltips
{
    private static readonly Dictionary<string, Dictionary<string, SourceTooltipPage>> pages = new();
    private static readonly JsonSerializerOptions options = new() { IncludeFields = true, PropertyNameCaseInsensitive = true };
    public static readonly List<Func<Item, string, string>> ValueProviders = new();
    // The opt-in client validation panel can hold a tooltip layer without
    // synthesizing operating-system keyboard input. Normal play returns null.
    public static Func<(bool Shift, bool Alt)?> DiagnosticModifiers;
    public static SourceTooltipPage Get(Mod mod, string id)
    {
        if (!pages.TryGetValue(mod.Name, out var book))
            pages[mod.Name] = book = JsonSerializer.Deserialize<Dictionary<string, SourceTooltipPage>>(mod.GetFileBytes("Data/source-tooltips.json"), options);
        return book.GetValueOrDefault(id) ?? new();
    }
    public static void Clear() { pages.Clear(); ValueProviders.Clear(); DiagnosticModifiers = null; }
    public static bool Shift => DiagnosticModifiers?.Invoke()?.Shift ?? (Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift));
    public static bool Alt => DiagnosticModifiers?.Invoke()?.Alt ?? (Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt));
    public static string Resolve(Item item, string token)
    {
        foreach (var provider in ValueProviders) { string value = provider(item, token); if (value != null) return value; }
        var p = Main.LocalPlayer;
        var legacy = p.GetModPlayer<LegacyPlayer>();
        var imported = item.ModItem as ImportedItem;
        return token switch {
            "prefixCurses" => LegacyPrefixes.TotalCurses(p).ToString(),
            "prefixDamage" => (LegacyPrefixes.TotalCurses(p) * 8).ToString(),
            "prefixMining" => (LegacyPrefixes.TotalCurses(p) * 14).ToString(),
            "prefixRegen" => (LegacyPrefixes.TotalCurses(p) * 8).ToString(),
            "berserkDamage" => (legacy.MissingHealth * 100).ToString("0.#"),
            "berserkSpeed" => (legacy.MissingHealth * 50).ToString("0.#"),
            "sourceAoeWidth" => (legacy.Has("cosmic_scroll") ? 5 : 3).ToString(),
            "cubeDamageLimit" => (legacy.Cursed ? 150 : 100).ToString(),
            "cooldown" => (Math.Max(0, imported?.ImportId == "cosmic_scroll" ? legacy.RevivalCooldown : legacy.SpellCooldown) / 60f).ToString("0.#"),
            "panDamage" or "panArmor" => "+0.5",
            "xpStored" or "xpLevel" => "0",
            "beheading" => "10", // Original base chance; this weapon's attack is deferred.
            "quote" => "NO_PERIL",
            "mode" => imported?.ImportId switch {
                "mining_charm" => BridgeUnits.Text(imported.Disabled ? "停用" : "启用", imported.Disabled ? "Disabled" : "Enabled"),
                "void_stone" => imported.Disabled ? BridgeUnits.Text("未启用", "Deactivated") : item.GetGlobalItem<ItemMetadata>().VoidMode switch {
                    1 => BridgeUnits.Text("保留一组", "Last Stack"), 2 => BridgeUnits.Text("超量吸收", "Excess Absorption"), _ => BridgeUnits.Text("全部销毁", "Total Absorption") },
                "the_judgement" => BridgeUnits.Text("正常", "Normal"),
                _ => BridgeUnits.Text("已停用", "Deactivated")
            },
            "inscribedName" => imported?.Inscription is { Length: > 0 } inscription ? inscription : BridgeUnits.Text("未题名", "Unnamed"),
            "missingHealth" => (legacy.MissingHealth * 100).ToString("0.#"),
            "sufferingTime" => SufferingPercent(legacy.CursedTicks, legacy.PlayTicks),
            "life" => imported?.ImportId == "eldritch_pan" ? "+15%" : p.statLife.ToString(), "owner" => item.GetGlobalItem<ItemMetadata>().BoundName is { Length: > 0 } name ? name : imported?.Inscription is { Length: > 0 } oldName ? oldName : BridgeUnits.Text("未绑定", "Unbound"),
            "boundX" => ((imported?.BoundPosition.X ?? 0) / 16).ToString("0"),
            "boundY" => ((imported?.BoundPosition.Y ?? 0) / 16).ToString("0"),
            "realm" => LegacyRealms.Describe(imported?.BoundWorld ?? "", imported?.BoundPosition ?? default),
            "spellKey" => AssignedKeys(EnigmaticLegacy.SpellstoneKey),
            "enderKey" => AssignedKeys(EnigmaticLegacy.EnderKey),
            _ => throw new InvalidOperationException("Unresolved relic tooltip token: " + token)
        };
    }
    public static string SufferingPercent(long cursed, long played) => (played > 0 ? 100 * Math.Round(Math.Clamp(cursed / (double)played, 0, 1), 3, MidpointRounding.AwayFromZero) : 0).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";
    private static string AssignedKeys(ModKeybind key)
    {
        // Dedicated servers have no client input profile.
        if (Main.dedServ) return key == EnigmaticLegacy.SpellstoneKey ? "V" : "K";
        return string.Join(" / ", key.GetAssignedKeys());
    }
    private static bool Visible(Item item, SourceTooltipRow row, bool shift, bool alt)
    {
        var p = Main.LocalPlayer; var legacy = p.GetModPlayer<LegacyPlayer>();
        foreach (string part in row.Guard.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            bool negate = part.StartsWith('!'); string guard = negate ? part[1..] : part;
            bool pass = guard switch {
                "shift" => shift, "alt" => alt,
                "ctrl" => Main.keyState.IsKeyDown(Keys.LeftControl) || Main.keyState.IsKeyDown(Keys.RightControl),
                "creative" => p.difficulty == 3,
                "dormant" => item.ModItem is ImportedItem { Tainted: false },
                "awake" => item.ModItem is ImportedItem { Tainted: true },
                "equipped" => item.ModItem is ImportedItem i ? legacy.Has(i.ImportId) : Resolve(item,"equipped") == "true",
                "cursed" => legacy.Cursed,
                "granted" => item.GetGlobalItem<ItemMetadata>().BoundOwner.Length > 0,
                "named" => item.GetGlobalItem<ItemMetadata>().BoundName.Length > 0 || item.ModItem is ImportedItem { Inscription.Length: > 0 },
                "berserk" => legacy.Has("berserk_charm"),
                "abyss" => legacy.Has("abyssal_heart"),
                "xp" or "offhand" or "noHunger" => false,
                "combat" or "customEntity" => false,
                "storedEntity" => Resolve(item, "storedEntity").Length > 0,
                _ when guard.StartsWith("state:") => Resolve(item, guard[6..]) == "true",
                "energy" or "violenceCurses" => Resolve(item,guard) != "0",
                "disabled" => item.ModItem is ImportedItem { Disabled: true },
                _ when guard.StartsWith("bless>=") => int.TryParse(Resolve(item,"blessLevel"), out int level) && level >= int.Parse(guard[7..]),
                _ when guard.StartsWith("bless>") => int.TryParse(Resolve(item,"blessLevel"), out int level) && level > int.Parse(guard[6..]),
                _ when guard.StartsWith("bless<=") => int.TryParse(Resolve(item,"blessLevel"), out int level) && level <= int.Parse(guard[7..]),
                _ => true
            };
            if (pass == negate) return false;
        }
        return true;
    }
    public static List<TooltipLine> SourceLines(Mod mod, Item item, IEnumerable<SourceTooltipRow> rows, bool shift = false, bool alt = false)
    {
        var lines = new List<TooltipLine>();
        foreach (var row in rows.Where(r => Visible(item, r, shift, alt)))
        {
            string text = BridgeUnits.Text(row.Zh.Length == 0 ? row.En : row.Zh, row.En.Length == 0 ? row.Zh : row.En);
            text = Regex.Replace(text, @"\{(\w+)\}", m => Resolve(item, m.Groups[1].Value));
            foreach (string part in text.Split('\n'))
                lines.Add(new TooltipLine(mod, "Original_" + row.Key, Colors(part)) { OverrideColor = SourceColor(row.Color) });
            if (row.Key == "tooltip.enigmaticlegacy.voidStoneCtrlList")
                foreach (string key in item.GetGlobalItem<ItemMetadata>().VoidFilters.Take(25))
                    lines.Add(new TooltipLine(mod, "Original_VoidFilter_" + key, " - " + LegacyVoidConsumption.DisplayName(key)) { OverrideColor = SourceColor("GOLD") });
        }
        return lines;
    }
    private static Color? SourceColor(string name) => name switch {
        "GOLD" => new Color(255,170,0), "RED" => new Color(255,85,85), "DARK_RED" => new Color(170,0,0),
        "BLUE" => new Color(85,85,255), "GREEN" => new Color(85,255,85), "YELLOW" => Color.Yellow,
        "LIGHT_PURPLE" => new Color(255,85,255), "DARK_PURPLE" => new Color(170,0,170),
        "AQUA" => new Color(85,255,255), "GRAY" => new Color(170,170,170), "DARK_GRAY" => new Color(85,85,85),
        _ => null
    };
    public static string Colors(string text)
    {
        string[] colors = { "000000", "0000AA", "00AA00", "00AAAA", "AA0000", "AA00AA", "FFAA00", "AAAAAA", "555555", "5555FF", "55FF55", "55FFFF", "FF5555", "FF55FF", "FFFF55", "FFFFFF" };
        var result = new StringBuilder(); var run = new StringBuilder(); string color = "";
        void Flush() { if (run.Length == 0) return; result.Append(color == "" ? run.ToString() : "[c/" + color + ":" + run + "]"); run.Clear(); }
        for (int n = 0; n < text.Length; n++)
        {
            if (text[n] == '§' && n + 1 == text.Length) continue; // Malformed trailing source color code.
            if (text[n] == '§' && n + 1 < text.Length)
            {
                Flush(); char code = char.ToLowerInvariant(text[++n]); int index = "0123456789abcdef".IndexOf(code);
                if (index >= 0) color = colors[index]; else if (code == 'r') color = "";
            }
            else run.Append(text[n]);
        }
        Flush(); return result.ToString();
    }
    public static List<TooltipLine> Content(Mod mod, Item item, string id, bool shift, bool alt)
    {
        SourceTooltipPage source = Get(mod, id);
        var content = SourceLines(mod, item, source.Default, shift, alt);
        if (shift) content.AddRange(SourceLines(mod, item, source.Shift, shift, alt));
        if (alt) content.AddRange(SourceLines(mod, item, source.Alt, shift, alt));
        return content;
    }
    private static bool WeaponStat(string name) => name is "Damage" or "CritChance" or "Speed" or "Knockback"
        or "NoSpeedScaling" or "SpecialSpeedScaling" or "PickPower" or "AxePower" or "HammerPower" or "TileBoost" or "UseMana"
        || name.StartsWith("Prefix", StringComparison.Ordinal);

    public static void Append(Mod mod, Item item, string id, List<TooltipLine> native,
        bool showWeaponStats = false, IEnumerable<TooltipLine> progress = null)
    {
        // Keep the original native objects: prefixes, damage hooks, other mods'
        // edits, colors and modifier flags must all survive this filter.
        native.RemoveAll(line => line.Mod == "Terraria" && line.Name != "ItemName" && !(showWeaponStats && WeaponStat(line.Name)));
        if (showWeaponStats && item.pick == 0 && item.axe == 0 && item.hammer == 0)
        {
            int ticks = CombinedHooks.TotalUseTime(item.useTime, Main.LocalPlayer, item);
            string label = id == "eldritch_pan" ? BridgeUnits.Text("最短投掷间隔", "Minimum throw interval") : BridgeUnits.Text("使用间隔", "Use interval");
            native.Add(new TooltipLine(mod, "WeaponUseInterval", label + BridgeUnits.Text($"：{ticks / 60f:0.###} 秒（{ticks} tick）", $": {ticks / 60f:0.###} s ({ticks} ticks)")));
        }
        if (progress != null) native.AddRange(TooltipLayout.Wrap(progress, TooltipLayout.ScreenWidth));
        native.AddRange(TooltipLayout.Wrap(Content(mod, item, id, Shift, Alt), TooltipLayout.ScreenWidth));
    }
}

