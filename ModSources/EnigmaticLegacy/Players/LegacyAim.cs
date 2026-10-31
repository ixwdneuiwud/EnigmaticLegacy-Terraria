// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Content.Status;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Players;

public sealed class LegacyAim : ModPlayer
{
    public Vector2 Direction;
    private readonly LegacyLight light = new();
    public override void PostUpdate()
    {
        if (Player.whoAmI == Main.myPlayer)
        {
            Direction = (Main.MouseWorld - Player.Center).SafeNormalize(new Vector2(Player.direction, 0));
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.GameUpdateCount % 6 == 0 && Player.GetModPlayer<LegacyPlayer>().Has("eldritch_amulet"))
            {
                var packet = Mod.GetPacket(); packet.Write((byte)10); packet.Write((byte)Player.whoAmI);
                packet.Write(Direction.X); packet.Write(Direction.Y); packet.Send();
            }
        }
        if (Player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server || Main.GameUpdateCount % 15 != 0) return;
        foreach (Player owner in Main.ActivePlayers)
        {
            var legacy = owner.GetModPlayer<LegacyPlayer>();
            if (!LegacyGaze.Pvp(owner, Player)) continue;
            var self = Player.GetModPlayer<LegacyPlayer>();
            if (legacy.Has("eldritch_amulet") && legacy.Worthy && !(self.Has("eldritch_amulet") && self.Worthy) && LegacyGaze.Observed(owner, Player))
            {
                Player.AddBuff(ModContent.BuffType<SlownessII>(), 30);
                Player.AddBuff(ModContent.BuffType<WeaknessII>(), 30);
            }
            if (Main.GameUpdateCount % 30 == 0 && legacy.Has("void_pearl") && LegacyRealms.Same(Player.Center, owner.Center) && Player.DistanceSQ(owner.Center) <= 256 * 256)
            {
                light.Scan(owner.Center, 16);
                if (light.Brightness(Player.Center) < .2f)
                {
                    LegacyDamage.Hurt(Player, LegacyDamage.Type("darkness"), Terraria.DataStructures.PlayerDeathReason.ByPlayerItem(owner.whoAmI, owner.HeldItem), BridgeUnits.Health(4), pvp: true);
                    Player.AddBuff(ModContent.BuffType<LesserWithering>(), 240);
                    Player.AddBuff(ModContent.BuffType<SlownessIII>(), 300);
                    Player.AddBuff(BuffID.Darkness, 300);
                }
            }
        }
    }
}
