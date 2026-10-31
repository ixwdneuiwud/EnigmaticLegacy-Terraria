// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Linq;
using EnigmaticLegacy.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Diagnostics;

// The disposable world may already contain a real test death or dropped items.
// Isolate fixture-owned drops and graves, then restore the original world state.
internal sealed class ClientFixtureScope : IDisposable
{
    private readonly Item[] items = Main.item.Select(i => i.Clone()).ToArray();
    private readonly TagCompound graves = new();
    public ClientFixtureScope()
    {
        var world = ModContent.GetInstance<LegacyWorld>();
        world.SaveWorldData(graves); world.ClearWorld();
        for (int i = 0; i < Main.item.Length; i++) Main.item[i] = new Item { whoAmI = i };
    }
    public void Dispose()
    {
        for (int i = 0; i < Main.item.Length; i++) Main.item[i] = items[i];
        var world = ModContent.GetInstance<LegacyWorld>();
        world.ClearWorld(); world.LoadWorldData(graves);
    }
}
