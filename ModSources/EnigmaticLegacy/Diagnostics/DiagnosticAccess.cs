// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.IO;
using System.Text.Json;
using Terraria;
using Terraria.ID;

namespace EnigmaticLegacy.Diagnostics;

public static class DiagnosticAccess
{
    private static string path = "", character = "", world = "";
    public static void Refresh() { path = ""; character = world = ""; }
    public static bool Allowed(Player p)
    {
        if (Main.dedServ || Main.netMode != NetmodeID.SinglePlayer) return false;
        if (Main.worldName.StartsWith("RelicValidation", StringComparison.Ordinal) && p.name.StartsWith("RelicValidation", StringComparison.Ordinal)) return true;
        string file = Path.Combine(Main.SavePath, "relic-test-access.json");
        if (path != file)
        {
            path = file; character = world = "";
            if (File.Exists(file))
            {
                using var data = JsonDocument.Parse(File.ReadAllText(file));
                character = data.RootElement.GetProperty("character").GetString();
                world = data.RootElement.GetProperty("world").GetString();
            }
        }
        return character.Length > 0 && world.Length > 0 && p.name == character && Main.worldName == world;
    }
}

public sealed class DiagnosticAccessCache : Terraria.ModLoader.ModSystem
{
    public override void OnWorldLoad() => DiagnosticAccess.Refresh();
    public override void OnWorldUnload() => DiagnosticAccess.Refresh();
}
