// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.IO;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content.Projectiles;

// Shared source delivery rules, independent of potion level and effect catalog.
public abstract class ImportedAreaPotion : ModProjectile
{
    private int age, directNpc = -1, directPlayer = -1;
    private float consumedRadius;
    private readonly Dictionary<int, int> npcReady = new(), playerReady = new();
    protected abstract bool Valid { get; }
    protected abstract bool Negative { get; }
    protected virtual bool Instant => false;
    protected abstract int EffectTicks { get; }
    protected abstract void Apply(NPC npc, Player owner, float factor);
    protected abstract void Apply(Player player, float factor);
    public bool Cloud => Projectile.ai[2] != 0;
    public float Radius => Math.Max(0, 48 - age * (48f / 1800) - consumedRadius);
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16; Projectile.timeLeft = 180;
        Projectile.friendly = Projectile.hostile = false; Projectile.tileCollide = true;
    }
    public override void AI()
    {
        if (!Valid) { Projectile.Kill(); return; }
        if (!Cloud)
        {
            Projectile.velocity.Y += .2f; Projectile.rotation += Projectile.velocity.X * .03f;
            if (Projectile.timeLeft >= 175) return;
            foreach (NPC npc in Main.ActiveNPCs)
                if (!npc.immortal && !npc.dontTakeDamage && npc.Hitbox.Intersects(Projectile.Hitbox))
                { directNpc = npc.whoAmI; Impact(); return; }
            foreach (Player player in Main.ActivePlayers)
                if (!player.dead && (player.whoAmI != Projectile.owner || Projectile.timeLeft < 165) &&
                    player.Hitbox.Intersects(Projectile.Hitbox) && Eligible(player))
                { directPlayer = player.whoAmI; Impact(); return; }
        }
        else
        {
            age++;
            if (Radius < 8) { Projectile.Kill(); return; }
            if (age >= 30 && age % 15 == 0) Area(true);
            if (!Main.dedServ)
            {
                Vector2 offset = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * MathF.Sqrt(Main.rand.NextFloat()) * Radius;
                Dust.NewDustPerfect(Projectile.Center + offset, DustID.PurpleTorch, Vector2.Zero, 120).noGravity = true;
            }
        }
    }
    private void Impact()
    {
        if (Projectile.ai[1] == 0) { Projectile.Kill(); return; }
        Projectile.ai[2] = 1; Projectile.velocity = Vector2.Zero; Projectile.tileCollide = false;
        Projectile.timeLeft = 1800; age = 0; consumedRadius = 0; Projectile.netUpdate = true;
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.ai[1] == 0) return true;
        Impact(); return false;
    }
    public override void OnKill(int timeLeft)
    {
        if (!Cloud && Valid) Area(false);
        if (!Main.dedServ && !Cloud)
            for (int n = 0; n < 20; n++) Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(3, 3), 100).noGravity = true;
    }
    private bool Eligible(Player player)
    {
        if (!player.active || player.dead || !LegacyRealms.Same(player.Center, Projectile.Center) || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers) return false;
        Player owner = Main.player[Projectile.owner];
        return !Negative || player == owner || LegacyGaze.Pvp(owner, player);
    }
    private float Factor(Entity target, bool cloud, bool direct)
    {
        Vector2 delta = target.Center - Projectile.Center;
        float radius = cloud ? Radius : 64;
        if (MathF.Abs(delta.Y) > (cloud ? 8 : 32) + target.height / 2f || delta.LengthSquared() >= radius * radius) return 0;
        float factor = cloud ? Instant ? .5f : .25f : direct ? 1 : 1 - delta.Length() / 64;
        return !cloud && !Instant && (int)MathF.Floor(EffectTicks * factor + .5f) <= 60 ? 0 : factor;
    }
    private void Area(bool cloud)
    {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers) return;
        Player owner = Main.player[Projectile.owner];
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.immortal || npc.dontTakeDamage || npc.life <= 0 || !LegacyRealms.Same(npc.Center, Projectile.Center) ||
                cloud && npcReady.GetValueOrDefault(npc.whoAmI) > age) continue;
            float factor = Factor(npc, cloud, npc.whoAmI == directNpc);
            if (factor <= 0) continue;
            if (Main.netMode != NetmodeID.MultiplayerClient) Apply(npc, owner, factor);
            if (cloud) { npcReady[npc.whoAmI] = age + 60; consumedRadius += 8; }
        }
        foreach (Player player in Main.ActivePlayers)
        {
            if (!Eligible(player) || cloud && playerReady.GetValueOrDefault(player.whoAmI) > age) continue;
            float factor = Factor(player, cloud, player.whoAmI == directPlayer);
            if (factor <= 0) continue;
            if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer) Apply(player, factor);
            if (cloud) { playerReady[player.whoAmI] = age + 60; consumedRadius += 8; }
        }
        if (cloud && Main.netMode != NetmodeID.MultiplayerClient) Projectile.netUpdate = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    { writer.Write(age); writer.Write(consumedRadius); writer.Write((short)directNpc); writer.Write((short)directPlayer); }
    public override void ReceiveExtraAI(BinaryReader reader)
    { age = Math.Clamp(reader.ReadInt32(), 0, 1800); consumedRadius = Math.Clamp(reader.ReadSingle(), 0, 48); directNpc = reader.ReadInt16(); directPlayer = reader.ReadInt16(); }
}
