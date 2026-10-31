// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.IO;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;

namespace EnigmaticLegacy.Systems;

internal enum BridgeAction : byte { Spellstone, Utility, ToggleMagnet, ShieldUp, ShieldDown }
internal static class LegacyNetwork
{
    private static readonly ulong[] LastAction = new ulong[256];
    private static readonly ulong[] LastStorm = new ulong[256];
    public static void Reset() { Array.Clear(LastAction); Array.Clear(LastStorm); }
    public static void SendAcquisition(Player player)
    {
        if (Main.netMode != NetmodeID.Server) return;
        var legacy = player.GetModPlayer<LegacyPlayer>();
        var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)9); packet.Write((byte)player.whoAmI);
        packet.Write(legacy.FirstChestReward); packet.Write(legacy.FirstMoonLordReward); packet.Send(player.whoAmI);
    }
    public static void RequestStorm()
    {
        var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)6); packet.Send();
    }
    internal static void ReflectContact(Player player, int npcIndex, int damage)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) { LegacyActions.ReflectContact(player, npcIndex, damage); return; }
        var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)8); packet.Write((short)npcIndex); packet.Write(damage); packet.Send();
    }
    public static void Request(BridgeAction action, Vector2 target)
    {
        Player player = Main.LocalPlayer;
        // Terraria player inventory, movement and hurt hooks run on the owning client.
        // Run these once on that owner, before vanilla consumes the held stack.
        bool affectsServerNpc = action == BridgeAction.Utility && player.HeldItem.ModItem is ImportedItem held &&
            (held.ImportId == "guardian_heart" || held.ImportId == "extradimensional_eye" && player.altFunctionUse != 2);
        if (Main.netMode == NetmodeID.SinglePlayer || !affectsServerNpc)
        {
            Execute(player, action, target, player.altFunctionUse == 2); return;
        }
        var packet = ImportCatalog.Owner.GetPacket();
        packet.Write((byte)1); packet.Write((byte)action); packet.Write(target.X); packet.Write(target.Y); packet.Write(player.altFunctionUse == 2);
        packet.Send();
    }
    public static void SendState(LegacyPlayer player, int toWho = -1, int ignore = -1)
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        var packet = ImportCatalog.Owner.GetPacket();
        packet.Write((byte)0); packet.Write((byte)player.Player.whoAmI); player.WriteState(packet); packet.Send(toWho, ignore);
    }
    public static void ContactReaction(Player player, int npc, bool shield)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient) { LegacyActions.ReactToContact(player, npc, shield); return; }
        var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)5); packet.Write((short)npc); packet.Write(shield); packet.Send();
    }
    public static void Handle(BinaryReader reader, int whoAmI)
    {
        byte type = reader.ReadByte();
        if (type == 12)
        {
            int index = reader.ReadByte(), x = reader.ReadInt16(), y = reader.ReadInt16(); string world = reader.ReadString();
            if (index >= Main.maxPlayers || Main.netMode == NetmodeID.Server && index != whoAmI || world.Length > 36) return;
            var anchor = Main.player[index].GetModPlayer<LegacyAnchor>();
            anchor.Position = new Point(x, y); anchor.World = world;
            if (Main.netMode == NetmodeID.Server) anchor.SyncPlayer(-1, index, false);
            return;
        }
        if (type == 11 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
        {
            int x = reader.ReadInt16(), y = reader.ReadInt16(); bool use = reader.ReadBoolean(); Player p = Main.player[whoAmI];
            if (!p.active || p.dead || !WorldGen.InWorld(x,y,2)) return;
            if (!use) DimensionalAnchorTile.Charge(p,x,y);
            else DimensionalAnchorTile.Bind(p,x,y);
            return;
        }
        if (type == 10)
        {
            int index = reader.ReadByte(); Vector2 direction = new(reader.ReadSingle(), reader.ReadSingle());
            if (index >= Main.maxPlayers || Main.netMode == NetmodeID.Server && index != whoAmI ||
                !float.IsFinite(direction.X) || !float.IsFinite(direction.Y) || direction.LengthSquared() < .01f) return;
            direction.Normalize(); Main.player[index].GetModPlayer<LegacyAim>().Direction = direction;
            if (Main.netMode == NetmodeID.Server)
            {
                var packet = ImportCatalog.Owner.GetPacket(); packet.Write((byte)10); packet.Write((byte)index);
                packet.Write(direction.X); packet.Write(direction.Y); packet.Send(-1, index);
            }
            return;
        }
        if (type == 9 && Main.netMode == NetmodeID.MultiplayerClient)
        {
            int index = reader.ReadByte(); if (index >= Main.maxPlayers) return;
            var legacy = Main.player[index].GetModPlayer<LegacyPlayer>();
            legacy.FirstChestReward |= reader.ReadBoolean(); legacy.FirstMoonLordReward |= reader.ReadBoolean(); return;
        }
        if (type == 0)
        {
            int index = reader.ReadByte();
            if (index >= Main.maxPlayers || Main.netMode == NetmodeID.Server && index != whoAmI) return;
            Main.player[index].GetModPlayer<LegacyPlayer>().ReadState(reader);
            if (Main.netMode == NetmodeID.Server)
            {
                SendState(Main.player[index].GetModPlayer<LegacyPlayer>(), -1, index);
            }
        }
        else if (type == 1 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
        {
            var action = (BridgeAction)reader.ReadByte();
            var target = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            bool alternate = reader.ReadBoolean();
            if (!float.IsFinite(target.X) || !float.IsFinite(target.Y)) return;
            Player player = Main.player[whoAmI];
            if (!player.active || player.dead || !Enum.IsDefined(action)) return;
            // NPC interactions run on the server after checking the held item.
            Vector2 offset = target - player.Center;
            if (offset.LengthSquared() > 2048f * 2048f) target = player.Center + offset.SafeNormalize(Vector2.UnitX) * 2048;
            if (action == BridgeAction.Utility && player.HeldItem.ModItem is ImportedItem { ImportId: "guardian_heart" or "extradimensional_eye" })
                Execute(player, action, target, alternate);
        }
        else if (type == 2 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
            LegacyWorld.ReceiveDrop(reader, Main.player[whoAmI]);
        else if (type == 3 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
            LegacyWorld.ReceiveSoul(reader, Main.player[whoAmI]);
        else if (type == 4 && Main.netMode == NetmodeID.MultiplayerClient)
            LegacyWorld.ReadVisuals(reader);
        else if (type == 5 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
            LegacyActions.ReactToContact(Main.player[whoAmI], reader.ReadInt16(), reader.ReadBoolean());
        else if (type == 6 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
        {
            Player player = Main.player[whoAmI];
            var legacy = player.GetModPlayer<LegacyPlayer>();
            int cooldown = legacy.Has("cosmic_scroll") ? 900 : 1800;
            if (player.active && !player.dead && legacy.Has("ocean_stone") &&
                LegacyRealms.At(player.Center) != LegacyRealms.Nether && !Main.raining &&
                (LastStorm[whoAmI] == 0 || Main.GameUpdateCount - LastStorm[whoAmI] >= (ulong)cooldown) && player.CheckMana(150, true, false))
            {
                LastStorm[whoAmI] = Main.GameUpdateCount;
                legacy.SpellCooldown = cooldown; player.manaRegenDelay = Math.Max(player.manaRegenDelay, 120);
                LegacyActions.StartStorm();
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, player.whoAmI);
                SendState(legacy);
            }
        }
        else if (type == 8 && Main.netMode == NetmodeID.Server && whoAmI >= 0 && whoAmI < Main.maxPlayers)
            LegacyActions.ReflectContact(Main.player[whoAmI], reader.ReadInt16(), reader.ReadInt32());
    }
    private static void Execute(Player player, BridgeAction action, Vector2 target, bool alternate)
    {
        var legacy = player.GetModPlayer<LegacyPlayer>();
        int index = player.whoAmI;
        if (player.dead) return;
        if (action is BridgeAction.ShieldUp or BridgeAction.ShieldDown)
        {
            legacy.ShieldHeld = action == BridgeAction.ShieldUp && legacy.Carries("infernal_shield") && legacy.Cursed;
            SendState(legacy);
            return;
        }
        if (LastAction[index] != 0 && Main.GameUpdateCount - LastAction[index] < 8) return;
        LastAction[index] = Main.GameUpdateCount;
        switch (action)
        {
            case BridgeAction.ToggleMagnet:
                legacy.MagnetEnabled = !legacy.MagnetEnabled;
                break;
            case BridgeAction.Spellstone:
                LegacyActions.ActivateSpellstone(player, target);
                if (Main.netMode == NetmodeID.MultiplayerClient && legacy.ActiveSpellstone == "ocean_stone") return;
                break;
            case BridgeAction.Utility:
                if (player.HeldItem.ModItem is not ImportedItem imported || !legacy.CanUse(imported.Definition) || !LegacyActions.CanUse(player, imported)) break;
                LegacyActions.ExecuteUtility(player, imported, target, alternate);
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);
                    NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, (player.selectedItem + 1) % 50);
                }
                break;
        }
        SendState(legacy);
    }
}
