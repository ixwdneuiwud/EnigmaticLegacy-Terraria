// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Players;

public sealed class LegacyAnchor : ModPlayer
{
    public Point Position;
    public string World = "";
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        var packet = Mod.GetPacket(); packet.Write((byte)12); packet.Write((byte)Player.whoAmI);
        packet.Write((short)Position.X); packet.Write((short)Position.Y); packet.Write(World); packet.Send(toWho, fromWho);
    }
    internal bool TryRespawn()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || World != Main.ActiveWorldFileData.UniqueId.ToString() ||
            !WorldGen.InWorld(Position.X, Position.Y, 4) || DimensionalAnchorTile.Charges(Position.X,Position.Y)==0) return false;
        for (int distance=1; distance<=3; distance++)
            for (int direction=-1; direction<=1; direction++)
            {
                Vector2 location = new(Position.X*16 + 8 + direction*distance*16 - Player.width/2f, Position.Y*16 - Player.height - 2);
                if (Collision.SolidCollision(location,Player.width,Player.height) || Collision.LavaCollision(location,Player.width,Player.height)) continue;
                Player.Teleport(location,1);
                DimensionalAnchorTile.UseCharge(Position.X,Position.Y);
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.TeleportEntity,-1,-1,null,0,Player.whoAmI,location.X,location.Y,1);
                return true;
            }
        return false;
    }
    public override void SaveData(TagCompound tag) { tag["anchorX"]=Position.X; tag["anchorY"]=Position.Y; tag["anchorWorld"]=World; }
    public override void LoadData(TagCompound tag) { Position=new Point(tag.GetInt("anchorX"),tag.GetInt("anchorY")); World=tag.GetString("anchorWorld"); }
}

// ModPlayer.OnRespawn runs before vanilla assigns the final spawn position.
// Complete the anchor relocation only after that native routine returns;
// the server owns both relocation and the single charge roll in multiplayer.
public sealed class LegacyAnchorRespawns : ModSystem
{
    public override void Load() => On_Player.Spawn += Spawn;
    private static void Spawn(On_Player.orig_Spawn orig, Player player, PlayerSpawnContext context)
    {
        bool respawning = player.dead;
        orig(player, context);
        if (respawning && Main.netMode != NetmodeID.MultiplayerClient)
            player.GetModPlayer<LegacyAnchor>().TryRespawn();
    }
}
