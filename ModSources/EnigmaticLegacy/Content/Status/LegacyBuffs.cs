// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Players;
using EnigmaticLegacy.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Content.Status;

public abstract class LegacyBuff : ModBuff
{
    public override string Texture => "EnigmaticLegacy/Assets/Status/void_pearl";
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = this is Withering or LesserWithering or DevouringDarkness or LegacyWeakness;
        Main.buffNoSave[Type] = true;
    }
}
public sealed class Withering : LegacyBuff { }
public sealed class WitheringIV : LegacyBuff
{
    public override void SetStaticDefaults() { base.SetStaticDefaults(); Main.debuff[Type] = true; }
}
public sealed class LesserWithering : LegacyBuff { }
public sealed class DevouringDarkness : LegacyBuff { }
public abstract class LeveledLegacyBuff : LegacyBuff
{
    public abstract LegacyEffect Effect { get; }
    public virtual int Level => 1;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.debuff[Type] = Effect is LegacyEffect.Weakness or LegacyEffect.Slowness;
    }
    public override void Update(Player player, ref int buffIndex)
    {
        if (Effect == LegacyEffect.Regeneration) LegacyStatus.Regenerate(player, this, player.buffTime[buffIndex]);
        if (Effect == LegacyEffect.FireResistance) LegacyStatus.ProtectFire(player);
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
        if (Effect == LegacyEffect.Regeneration) LegacyStatus.Regenerate(npc, this, npc.buffTime[buffIndex]);
        if (Effect == LegacyEffect.FireResistance) npc.GetGlobalNPC<LegacyGlobalNPC>().ProtectFire(npc);
    }
}
public sealed class LegacyWeakness : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Weakness; }
public sealed class WeaknessII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Weakness; public override int Level => 2; }
public sealed class WeaknessIII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Weakness; public override int Level => 3; }
public sealed class WeaknessIV : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Weakness; public override int Level => 4; }
public sealed class StrengthI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Strength; }
public sealed class RegenerationI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Regeneration; }
public sealed class ResistanceI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Resistance; }
public sealed class AbsorptionI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Absorption; }
public sealed class AbsorptionII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Absorption; public override int Level => 2; }
public sealed class SwiftnessI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Swiftness; }
public sealed class LeapingI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Leaping; }
public sealed class SlownessI : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Slowness; }
public sealed class SlownessII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Slowness; public override int Level => 2; }
public sealed class SlownessIII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Slowness; public override int Level => 3; }
public sealed class StrongRegeneration : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Regeneration; public override int Level => 2; }
public sealed class RegenerationIII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Regeneration; public override int Level => 3; }
public sealed class RegenerationIV : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Regeneration; public override int Level => 4; }
public sealed class LegacyStrength : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Strength; public override int Level => 2; }
public sealed class StrengthIV : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Strength; public override int Level => 4; }
public sealed class ResistanceII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Resistance; public override int Level => 2; }
public sealed class ResistanceIII : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Resistance; public override int Level => 3; }
public sealed class AbsorptionV : LeveledLegacyBuff
{
    public override LegacyEffect Effect => LegacyEffect.Absorption;
    public override int Level => 5;
    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) => tip += "\n" +
        BridgeUnits.Text("剩余吸收生命：", "Remaining absorption: ") + Main.LocalPlayer.GetModPlayer<LegacyPlayer>().AbsorptionShield;
}
public sealed class LegacyFireResistance : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.FireResistance; }
public sealed class LegacySlowFalling : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.SlowFalling; }
public sealed class LegacyHaste : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Haste; }
public sealed class StrongHaste : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Haste; public override int Level => 2; }
public sealed class StrongPoison : LegacyBuff
{
    public override void SetStaticDefaults() { base.SetStaticDefaults(); Main.debuff[Type] = true; }
    // Poison II: one source health every 12 Minecraft ticks, never delivering a killing blow.
    public override void Update(Player player, ref int buffIndex)
    {
        if (player.dead || player.statLife <= 1 || player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server || player.buffTime[buffIndex] % 36 != 0) return;
        player.statLife = System.Math.Max(1, player.statLife - BridgeUnits.Health(1));
        if (Main.netMode == NetmodeID.MultiplayerClient) NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, player.whoAmI);
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || npc.buffTime[buffIndex] % 36 != 0 || npc.immortal || npc.life <= 1 || Compatibility.HasNpcTag(npc, "undead")) return;
        npc.life = System.Math.Max(1, npc.life - BridgeUnits.Health(1)); npc.netUpdate = true;
    }
}
public sealed class MoltenHeart : LegacyBuff
{
    public override string Texture => "EnigmaticLegacy/Assets/Status/molten_heart";
    public override void Update(Player player, ref int buffIndex)
    {
        LegacyStatus.ProtectFire(player);
    }
    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<LegacyGlobalNPC>().ProtectFire(npc);
}
public sealed class LegacySwiftness : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Swiftness; public override int Level => 2; }
public sealed class LegacyLeaping : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Leaping; public override int Level => 2; }
public sealed class LegacySlowness : LeveledLegacyBuff { public override LegacyEffect Effect => LegacyEffect.Slowness; public override int Level => 4; }
public sealed class LegacyTurtleMaster : LegacyBuff { }
