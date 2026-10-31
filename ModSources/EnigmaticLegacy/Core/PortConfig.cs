// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace EnigmaticLegacy.Core;

public sealed class PortConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(5f), Range(1f, 50f), ReloadRequired]
    public float HealthAndDamageScale = 5f;

    [DefaultValue(2f), Range(0.1f, 10f), ReloadRequired]
    public float ArmorScale = 2f;

    [DefaultValue(true)]
    public bool DedicatedSpellstoneSlot = true;

    [DefaultValue(true)]
    public bool StartingGifts = true;

    [DefaultValue(true)]
    public bool PermanentCurses = true;

    [DefaultValue(false)]
    public bool AllowMultipleSpellstones;

    public static PortConfig Current => ModContent.GetInstance<PortConfig>();
}
