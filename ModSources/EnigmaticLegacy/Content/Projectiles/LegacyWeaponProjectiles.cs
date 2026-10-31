// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.IO;
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Content.Projectiles;

public abstract class LegacyWeaponProjectile : ModProjectile
{
    public abstract string WeaponId { get; }
    public override string Texture => "EnigmaticLegacy/Assets/Items/" + WeaponId;
    private Item sourceItem;
    private string sourceIdentity = "";
    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_ItemUse use && use.Item.ModItem is ImportedItem imported && imported.ImportId == WeaponId)
        {
            sourceItem = use.Item;
            if (WeaponId == "eldritch_pan")
            {
                var data = sourceItem.GetGlobalItem<ItemMetadata>();
                if (data.WeaponIdentity == "") data.WeaponIdentity = Guid.NewGuid().ToString("N");
                sourceIdentity = data.WeaponIdentity;
            }
        }
    }
    private Item SourceItem(Player player)
    {
        // Inventory moves can clone the item. Its saved identity follows that copy;
        // a newly held pan must never receive another pan's kill history.
        if (sourceItem?.ModItem is ImportedItem imported && imported.ImportId == WeaponId && player.inventory.Contains(sourceItem) &&
            (sourceIdentity == "" || sourceItem.GetGlobalItem<ItemMetadata>().WeaponIdentity == sourceIdentity)) return sourceItem;
        return sourceIdentity == "" ? null : player.inventory.FirstOrDefault(i => i.ModItem is ImportedItem { ImportId: "eldritch_pan" } && i.GetGlobalItem<ItemMetadata>().WeaponIdentity == sourceIdentity);
    }
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(sourceIdentity);
    public override void ReceiveExtraAI(BinaryReader reader)
    {
        string value = reader.ReadString(); sourceIdentity = Guid.TryParse(value, out _) ? value : "";
    }
    public override bool? CanHitNPC(NPC target) => Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers &&
        Main.player[Projectile.owner].active && LegacyWeapons.Qualified(Main.player[Projectile.owner], WeaponId) ? null : false;
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => LegacyWeapons.ModifyHit(Main.player[Projectile.owner], WeaponId, target, ref modifiers);
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var player = Main.player[Projectile.owner];
        LegacyWeapons.OnHit(player, WeaponId, target, damageDone, SourceItem(player));
    }
    public override bool PreDraw(ref Color lightColor)
    {
        int type = ImportCatalog.Type(WeaponId);
        var texture = TextureAssets.Item[type].Value;
        Rectangle frame = Main.itemAnimations[type]?.GetFrame(texture) ?? texture.Bounds;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor),
            Projectile.rotation, frame.Size() / 2, Projectile.scale, Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        return false;
    }
}

public abstract class LegacyBookBolt : LegacyWeaponProjectile
{
    public override string Texture => "EnigmaticLegacy/Assets/Projectiles/" + (WeaponId == "the_twist" ? "twist_bolt" : WeaponId == "the_infinitum" ? "infinitum_bolt" : "etherium_bolt");
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
        ProjectileID.Sets.TrailCacheLength[Type] = WeaponId == "the_infinitum" ? 24 : 5;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 28; Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee; Projectile.timeLeft = WeaponId == "the_infinitum" ? 360 : 150;
        Projectile.penetrate = WeaponId == "the_infinitum" ? 4 : 2;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
        Projectile.tileCollide = WeaponId != "the_infinitum"; Projectile.ignoreWater = true;
    }
    public override void AI()
    {
        if (WeaponId == "the_infinitum")
        {
            LegacyHoming.Steer(Projectile, LegacyHoming.Find(Projectile, 2400), 30, .14f);
            if (!Main.dedServ && Projectile.timeLeft % 3 == 0)
            {
                Vector2 side = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * MathF.Sin(Projectile.timeLeft * .35f) * 18;
                foreach (int sign in new[] { -1, 1 })
                { var dust = Dust.NewDustPerfect(Projectile.Center + side * sign, DustID.PurpleTorch, -Projectile.velocity * .08f, 120, default, 1.2f); dust.noGravity = true; }
            }
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (++Projectile.frameCounter >= 5) { Projectile.frameCounter = 0; Projectile.frame = (Projectile.frame + 1) % 4; }
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, 0.25f, 0.15f, 0.5f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        for (int n = Projectile.oldPos.Length - 1; n >= 0; n--)
        {
            if (Projectile.oldPos[n] == Vector2.Zero) continue;
            float strength = (1f - (float)n / Projectile.oldPos.Length) * 0.35f;
            Main.EntitySpriteDraw(texture, Projectile.oldPos[n] + Projectile.Size / 2 - Main.screenPosition,
                frame, Color.White * strength, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None);
        }
        if (WeaponId == "the_infinitum")
        {
            var glow = TextureAssets.Extra[98].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float pulse = 1 + .12f * MathF.Sin(Projectile.timeLeft * .3f);
            Main.EntitySpriteDraw(glow, center, null, new Color(110, 40, 210, 0), Projectile.rotation, glow.Size() / 2,
                new Vector2(.65f, .19f) * pulse, SpriteEffects.None);
            Main.EntitySpriteDraw(glow, center, null, new Color(210, 160, 255, 0), Projectile.rotation + MathHelper.PiOver2, glow.Size() / 2,
                new Vector2(.24f, .065f), SpriteEffects.None);
        }
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame,
            Color.White, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None);
        return false;
    }
}
public sealed class LegacyTwistBolt : LegacyBookBolt { public override string WeaponId => "the_twist"; }
public sealed class LegacyInfinitumBolt : LegacyBookBolt { public override string WeaponId => "the_infinitum"; }
public sealed class LegacyEtheriumBolt : LegacyBookBolt { public override string WeaponId => "etherium_sword"; }

public sealed class LegacyEtheriumScythe : LegacyWeaponProjectile
{
    public override string WeaponId => "etherium_scythe";
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.DeathSickle);
        AIType = ProjectileID.DeathSickle; Projectile.DamageType = DamageClass.Melee;
        Projectile.width = Projectile.height = 64; Projectile.scale = 2;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 12;
    }
}
public sealed class LegacyPan : LegacyWeaponProjectile
{
    public override string WeaponId => "eldritch_pan";
    private Vector2 viewCenter, viewSize;
    private Vector2 sentViewCenter, sentViewSize;
    public Rectangle ViewBounds => new((int)(viewCenter.X - viewSize.X / 2), (int)(viewCenter.Y - viewSize.Y / 2), (int)viewSize.X, (int)viewSize.Y);
    public void SetView(Vector2 center, Vector2 size)
    {
        if (!float.IsFinite(center.X) || !float.IsFinite(center.Y) || !float.IsFinite(size.X) || !float.IsFinite(size.Y)) return;
        viewCenter = center; viewSize = Vector2.Clamp(size, new Vector2(64), new Vector2(16384));
    }
    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
        SetView(Main.player[Projectile.owner].Center, new Vector2(1280, 720));
        CaptureView();
    }
    private void CaptureView()
    {
        if (Main.dedServ || Projectile.owner != Main.myPlayer) return;
        Vector2 center = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) / 2;
        Vector2 size = new Vector2(Main.screenWidth, Main.screenHeight) / Vector2.Max(Main.GameViewMatrix.Zoom, new Vector2(.1f));
        if (Vector2.DistanceSquared(center, sentViewCenter) > 24 * 24 || size != sentViewSize) Projectile.netUpdate = true;
        SetView(center, size);
    }
    private bool DamageableFriendly(NPC npc) => npc.active && npc.friendly && !npc.dontTakeDamage && !npc.immortal && npc.life > 0 &&
        npc.type != NPCID.TargetDummy && Main.player[Projectile.owner].CanNPCBeHitByPlayerOrPlayerProjectile(npc, Projectile);
    public override bool? CanHitNPC(NPC target)
    {
        if (base.CanHitNPC(target) == false) return false;
        // An explicit native hit permission is required for friendly NPCs.
        // CombinedHooks still lets another mod or the animal guidebook veto it.
        return target.friendly ? DamageableFriendly(target) : null;
    }
    public bool VisibleTarget(NPC npc) => (npc.CanBeChasedBy(Projectile) || DamageableFriendly(npc)) && ViewBounds.Intersects(npc.Hitbox) &&
        LegacyRealms.Same(Main.player[Projectile.owner].Center, npc.Center) &&
        (!Projectile.usesLocalNPCImmunity || Projectile.localNPCImmunity[npc.whoAmI] == 0) &&
        CombinedHooks.CanHitNPCWithProj(Projectile, npc) != false;
    private NPC FindTarget()
    {
        NPC best = null; float bestScore = float.MaxValue;
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!VisibleTarget(npc)) continue;
            float score = npc.Distance(Projectile.Center);
            foreach (Projectile sibling in Main.ActiveProjectiles)
                if (sibling.whoAmI != Projectile.whoAmI && sibling.owner == Projectile.owner && sibling.type == Type &&
                    sibling.ai[0] >= 0 && (int)sibling.ai[2] == npc.whoAmI + 1) score += viewSize.Length() * 2;
            if (score < bestScore) { bestScore = score; best = npc; }
        }
        return best;
    }
    private void Return()
    {
        Projectile.ai[0] = -1; Projectile.ai[2] = 0; Projectile.netUpdate = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    { base.SendExtraAI(writer); writer.Write(viewCenter.X); writer.Write(viewCenter.Y); writer.Write(viewSize.X); writer.Write(viewSize.Y); sentViewCenter = viewCenter; sentViewSize = viewSize; }
    public override void ReceiveExtraAI(BinaryReader reader)
    { base.ReceiveExtraAI(reader); SetView(new Vector2(reader.ReadSingle(), reader.ReadSingle()), new Vector2(reader.ReadSingle(), reader.ReadSingle())); }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);
        if (Projectile.ai[0] >= 0 && (Projectile.ai[2] == 0 || (int)Projectile.ai[2] == target.whoAmI + 1)) Return();
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 44; Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee; Projectile.penetrate = -1;
        Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.timeLeft = 600;
        Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 10;
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead) { Projectile.Kill(); return; }
        Projectile.rotation += .42f;
        if (Projectile.ai[0] >= 0)
        {
            CaptureView();
            Projectile.ai[0]++;
            NPC target = null;
            int index = (int)Projectile.ai[2] - 1;
            if (index >= 0 && index < Main.maxNPCs && VisibleTarget(Main.npc[index])) target = Main.npc[index];
            if (target == null && Projectile.owner == Main.myPlayer)
            {
                target = FindTarget();
                int nextTarget = target == null ? 0 : target.whoAmI + 1;
                if (Projectile.ai[2] != nextTarget) { Projectile.ai[2] = nextTarget; Projectile.netUpdate = true; }
            }
            LegacyHoming.Steer(Projectile, target, 24, .2f);
            if (!ViewBounds.Intersects(Projectile.Hitbox) || Projectile.ai[0] >= Math.Max(55, viewSize.Length() / 22 + 15)) Return();
        }
        else
        {
            Vector2 delta = owner.Center - Projectile.Center;
            if (delta.LengthSquared() < 40 * 40) { Projectile.Kill(); return; }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, delta.SafeNormalize(Vector2.UnitX) * 38, .22f);
        }
        if (!Main.dedServ) Lighting.AddLight(Projectile.Center, .28f, .07f, .4f);
    }
}
