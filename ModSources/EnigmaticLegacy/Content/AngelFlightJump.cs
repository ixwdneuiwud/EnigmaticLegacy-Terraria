// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Players;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content;

public sealed class AngelFlightJump : ExtraJump
{
    public override Position GetDefaultPosition() => new After(CloudInABottle);
    public override float GetDurationMultiplier(Player player) => 1;
    public override bool CanStart(Player player) => player.GetModPlayer<LegacyPlayer>().Has("angel_blessing") && player.GetModPlayer<LegacyPlayer>().SpellCooldown == 0;
    public override void OnStarted(Player player, ref bool playSound)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        legacy.SpellCooldown = legacy.Has("cosmic_scroll") ? 45 : 120;
    }
}
