// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.IO;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Terraria.ModLoader;

namespace EnigmaticLegacy;

public sealed class EnigmaticLegacy : Mod
{
    internal static ModKeybind SpellstoneKey;
    internal static ModKeybind EnderKey;
    internal static ModKeybind MagnetKey;
    internal static ModKeybind ShieldKey;

    public override void Load()
    {
        ImportCatalog.Load(this);
        SpellstoneKey = KeybindLoader.RegisterKeybind(this, "Spellstone", "V");
        EnderKey = KeybindLoader.RegisterKeybind(this, "EnderStorage", "K");
        MagnetKey = KeybindLoader.RegisterKeybind(this, "Magnet", "M");
        ShieldKey = KeybindLoader.RegisterKeybind(this, "Shield", "B");
    }
    public override void PostSetupContent() { LegacyPrefixes.Initialize(); Compatibility.Initialize(this); }
    public override object Call(params object[] args) => Compatibility.Call(args);
    public override void HandlePacket(BinaryReader reader, int whoAmI) => LegacyNetwork.Handle(reader, whoAmI);
    public override void Unload()
    {
        ImportCatalog.Unload();
        Compatibility.Unload();
        LegacyPrefixes.Unload();
        LegacyFlight.Reset();
        LegacyAcquisition.Reset();
        BridgeExtensions.Clear();
        RelicTooltips.Clear();
        SpellstoneKey = EnderKey = MagnetKey = ShieldKey = null;
    }
}
