// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Content.Status;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content.Projectiles;

public sealed class LegacySkull : ModProjectile
{
    public override string Texture => "EnigmaticLegacy/Assets/Status/void_pearl";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 20; Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic; Projectile.timeLeft = 300; Projectile.penetrate = 1;
    }
    public override void AI()
    {
        NPC target = null; float best = 640 * 640;
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < best)
            { best = npc.DistanceSQ(Projectile.Center); target = npc; }
        if (target != null) Projectile.velocity = Vector2.Lerp(Projectile.velocity, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12, 0.04f);
        Projectile.rotation += 0.08f;
        if (!Main.dedServ) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0, 0, 150);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Withering>(), 600);
}
public sealed class LegacyPotionProjectile : ImportedAreaPotion
{
    public override string Texture => "EnigmaticLegacy/Assets/Items/recall_potion";
    private ImportedDefinition Potion => ContentSamples.ItemsByType.TryGetValue((int)Projectile.ai[0], out var item) && item.ModItem is ImportedItem imported ? imported.Definition : null;
    protected override bool Valid => Potion != null;
    protected override bool Negative => Potion?.Variant is "harming" or "poison" or "weakness" or "slowness";
    protected override bool Instant => Potion?.Variant is "healing" or "harming";
    protected override int EffectTicks => Potion == null ? 0 : LegacyPotions.Effect(Potion).Ticks;
    protected override void Apply(NPC npc, Player owner, float factor) => LegacyPotions.Apply(npc, owner, Potion, factor);
    protected override void Apply(Player player, float factor) => LegacyPotions.Apply(player, Potion, factor);
    public override bool PreDraw(ref Color lightColor)
    {
        if (Cloud || Potion == null) return false;
        int type = (int)Projectile.ai[0];
        Texture2D texture = TextureAssets.Item[type].Value;
        Rectangle frame = Main.itemAnimations[type]?.GetFrame(texture) ?? texture.Bounds;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2, .7f, SpriteEffects.None);
        return false;
    }
}
