// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.ObjectModel;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Systems;

// Keep the native tooltip pipeline (including other mods' lines and colors).
// Its background is measured before PreDrawTooltip, so fit that measurement
// and then use the same scale for the native line positions and glyphs.
public sealed class RelicTooltipSizing : ModSystem
{
    internal static float Scale = 1;
    internal static Vector2 Origin;
    public static bool Installed { get; private set; }

    public override void Load()
    {
        if (!Main.dedServ) IL_Main.MouseText_DrawItemTooltip += FitBackground;
    }

    public override void Unload() { Scale = 1; Installed = false; }

    public static bool IsRelic(Item item) => item.ModItem?.Mod.Name is "EnigmaticLegacy" or "RelicExpansions";

    private void FitBackground(ILContext il)
    {
        var cursor = new ILCursor(il);
        // The native accumulator starts at Vector2.Zero, collects every final
        // TooltipLine, then is used to clamp and draw the tooltip background.
        int dimensions = -1;
        if (!cursor.TryGotoNext(MoveType.After,
                i => i.MatchCall<Vector2>("get_Zero"), i => i.MatchStloc(out dimensions)) ||
            !cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Main>("toolTipDistance")))
            throw new InvalidOperationException("Native tooltip measurement could not be located");
        // Insert after X += toolTipDistance. The no-logo branch targets the
        // preceding X load; inserting before that target would be skipped for
        // virtually every item.
        cursor.GotoNext(MoveType.After, i => i.OpCode == OpCodes.Starg_S || i.OpCode == OpCodes.Starg);
        cursor.Emit(OpCodes.Ldloc, il.Body.Variables[dimensions]);
        cursor.EmitDelegate<Func<Vector2, Vector2>>(size => {
            Scale = IsRelic(Main.HoverItem) ? TooltipLayout.FitScale(size, Main.screenWidth, Main.screenHeight) : 1;
            return size * Scale;
        });
        cursor.Emit(OpCodes.Stloc, il.Body.Variables[dimensions]);
        Installed = true;
    }
}

public sealed class RelicTooltipLines : GlobalItem
{
    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (RelicTooltipSizing.IsRelic(item)) RelicTooltipSizing.Origin = new Vector2(x, y);
        return true;
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        float scale = RelicTooltipSizing.Scale;
        if (!RelicTooltipSizing.IsRelic(item) || scale >= 1) return true;
        Vector2 origin = RelicTooltipSizing.Origin;
        line.X = (int)Math.Round(origin.X + (line.X - origin.X) * scale);
        line.Y = (int)Math.Round(origin.Y + (line.Y - origin.Y) * scale);
        line.BaseScale *= scale;
        return true;
    }
}
