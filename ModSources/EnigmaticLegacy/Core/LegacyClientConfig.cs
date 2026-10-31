// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace EnigmaticLegacy.Core;

public sealed class LegacyClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    [DefaultValue(true)] public bool NarratorVoice = true;
    [DefaultValue(true)] public bool NarratorSubtitles = true;
}
