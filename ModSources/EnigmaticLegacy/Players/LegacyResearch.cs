// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.Collections.Generic;
using System.Linq;
using EnigmaticLegacy.Core;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Players;

public sealed class LegacyResearch : ModPlayer
{
    public readonly HashSet<string> Discovered = new(), ReadPages = new();
    public void Observe(int type)
    {
        foreach (string id in LegacyBook.Discoveries(type)) Discovered.Add(id);
    }
    public override void SaveData(TagCompound tag) { tag["discovered"] = Discovered.ToList(); tag["readPages"] = ReadPages.ToList(); }
    public override void LoadData(TagCompound tag)
    {
        Discovered.Clear(); ReadPages.Clear();
        foreach (string id in tag.GetList<string>("discovered")) Discovered.Add(id);
        foreach (string id in tag.GetList<string>("readPages")) ReadPages.Add(id);
    }
}
