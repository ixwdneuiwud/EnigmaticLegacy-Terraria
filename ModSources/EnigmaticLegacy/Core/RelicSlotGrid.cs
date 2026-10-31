// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace EnigmaticLegacy.Core;

// All slots remain native tML accessories: the native draw/click/save/network
// path is retained. Only their location and scale are supplied by this grid.
public sealed class RelicSlotGrid : ModSystem
{
    private Hook drawHook, panelHook, switchHook, cellHook;
    private delegate bool DrawSlot(AccessorySlotLoader self, int index, bool modded, int slot, Color color);
    private delegate void DrawPanel(AccessorySlotLoader self, int top);
    private delegate void DrawSwitch(AccessorySlotLoader self);
    private delegate void DrawCell(AccessorySlotLoader self, Item[] items, int context, int slot, bool skip, int x, int y, bool flag);
    private static readonly FieldInfo scroll = typeof(ModAccessorySlotPlayer).GetField("scrollSlots", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly Dictionary<int, Vector2> positions = new();
    private static Player layoutPlayer;
    private static ulong layoutTick = ulong.MaxValue;
    private static int layoutWidth, layoutHeight, layoutTop;
    private static float layoutScale = .85f;
    public readonly record struct Cell(int Type, int ExtraCells);
    public readonly record struct Grid(float Scale, int Rows, int Columns, Dictionary<int, Vector2> Positions);
    public static void Invalidate() => layoutTick = ulong.MaxValue;

    public override void Load()
    {
        if (Main.dedServ) return;
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        drawHook = new Hook(typeof(AccessorySlotLoader).GetMethod("Draw", flags), (Func<DrawSlot, AccessorySlotLoader, int, bool, int, Color, bool>)Draw);
        panelHook = new Hook(typeof(AccessorySlotLoader).GetMethod("DrawAccSlots", flags), (Action<DrawPanel, AccessorySlotLoader, int>)Panel);
        switchHook = new Hook(typeof(AccessorySlotLoader).GetMethod("DrawScrollSwitch", flags), (Action<DrawSwitch, AccessorySlotLoader>)HideScrollSwitch);
        cellHook = new Hook(typeof(AccessorySlotLoader).GetMethod("DrawSlot", flags), (Action<DrawCell, AccessorySlotLoader, Item[], int, int, bool, int, int, bool>)CellDraw);
    }
    public override void Unload()
    {
        drawHook?.Dispose(); panelHook?.Dispose(); switchHook?.Dispose(); cellHook?.Dispose();
        drawHook = panelHook = switchHook = cellHook = null;
        positions.Clear(); layoutPlayer = null; Invalidate(); AccessoryChannels.Clear();
    }
    private static void Panel(DrawPanel original, AccessorySlotLoader loader, int top)
    {
        // The user's layout is columns. Do not expose the native scroll toggle.
        scroll.SetValue(Main.LocalPlayer.GetModPlayer<ModAccessorySlotPlayer>(), false);
        layoutTop = top; Invalidate();
        original(loader, top);
    }
    private static void HideScrollSwitch(DrawSwitch original, AccessorySlotLoader loader) { }
    private static void CellDraw(DrawCell original, AccessorySlotLoader loader, Item[] items, int context, int slot, bool skip, int x, int y, bool flag)
    {
        // A collapsed slot stays inactive, but its saved item must be retrievable.
        // Only an empty cursor gets recovery clicks; incoming swaps stay blocked.
        if (context < 0 && loader.Get(slot) is IRelicAccessorySlot && skip && Main.mouseItem.IsAir && !items[slot].IsAir) skip = false;
        original(loader, items, context, slot, skip, x, y, flag);
    }
    private static bool Draw(DrawSlot original, AccessorySlotLoader loader, int index, bool modded, int slot, Color color)
    {
        if (!modded || loader.Get(slot) is not IRelicAccessorySlot) return original(loader, index, modded, slot, color);
        Ensure();
        float prior = Main.inventoryScale;
        try { Main.inventoryScale = layoutScale; return original(loader, index, modded, slot, color); }
        finally { Main.inventoryScale = prior; }
    }
    public static Vector2? Location(ModAccessorySlot slot)
    {
        if (Main.EquipPage != 0) return null;
        Ensure();
        return positions.TryGetValue(slot.Type, out var at) ? at : null;
    }
    private static void Ensure()
    {
        Player p = Main.CurrentPlayer;
        int width = Main.screenWidth > 0 ? Main.screenWidth : 1280;
        int height = Main.screenHeight > 0 ? Main.screenHeight : 720;
        if (layoutTick == Main.GameUpdateCount && ReferenceEquals(layoutPlayer, p) && layoutWidth == width && layoutHeight == height) return;
        var all = ModContent.GetContent<ModAccessorySlot>().ToArray();
        var own = all.Where(s => s is IRelicAccessorySlot && (s.IsEnabled() || s.IsVisibleWhenNotEnabled())).ToArray();
        int nativeRows = Math.Max(1, (int)((height - (layoutTop + 112 * .85f + 4)) / (56 * .85f) - 1.8f));
        int external = all.Count(s => s is not IRelicAccessorySlot && s.IsEnabled() && !s.CustomLocation.HasValue);
        int nativeColumns = Math.Max(1, (AccessoryChannels.NativeCapacity(p) + external + nativeRows - 1) / nativeRows);
        float reserved = 240 + (nativeColumns - 1) * 141;
        var cells = own.Select(s => new Cell(s.Type, (s.DrawVanitySlot ? 1 : 0) + (s.DrawDyeSlot ? 1 : 0))).ToArray();
        var grid = Arrange(cells, width, height, reserved);
        positions.Clear(); foreach (var (type, at) in grid.Positions) positions[type] = at;
        layoutScale = grid.Scale; layoutPlayer = p; layoutWidth = width; layoutHeight = height; layoutTick = Main.GameUpdateCount;
    }
    public static Grid Arrange(IReadOnlyList<Cell> cells, int width, int height, float reservedRight = 240)
    {
        // Place the grid below inventory. Compact only when the current window
        // cannot fit the normal 0.85 scale; every cell keeps the same hit scale.
        float top = Math.Min(330, height * .48f), bottom = Math.Max(top + 30, height - 88);
        float right = Math.Max(80, width - reservedRight), available = right - 16;
        float scale = .85f; int rows, columns; float pitchY;
        int[] extraColumns; float[] widths;
        do
        {
            pitchY = 52 * scale + 8;
            rows = Math.Max(1, (int)((bottom - top) / pitchY));
            columns = Math.Max(1, (cells.Count + rows - 1) / rows);
            extraColumns = new int[columns];
            for (int n = 0; n < cells.Count; n++) extraColumns[n / rows] = Math.Max(extraColumns[n / rows], cells[n].ExtraCells);
            widths = extraColumns.Select(extra => 52 * scale + 12 + extra * 47).ToArray();
            if (widths.Sum() <= available || scale <= .12f) break;
            scale -= .01f;
        } while (true);
        var result = new Dictionary<int, Vector2>();
        float[] left = new float[columns]; float edge = right;
        for (int c = 0; c < columns; c++) { edge -= widths[c]; left[c] = edge + extraColumns[c] * 47 + 4; }
        for (int n = 0; n < cells.Count; n++)
            result[cells[n].Type] = new Vector2((int)left[n / rows], (int)(top + n % rows * pitchY));
        return new Grid(scale, rows, columns, result);
    }
    public static void DrawGlyph(string texture, AccessorySlotType context, Item item, Vector2 position, bool hover)
    {
        if (Main.dedServ || !item.IsAir || context == AccessorySlotType.DyeSlot) return;
        // MC uses dark glyph pixels on a light background. The UI variant keeps
        // the exact source pixel mask with a palette suited to Terraria's panel.
        bool native = texture.StartsWith("Terraria/", StringComparison.Ordinal);
        var icon = ModContent.Request<Texture2D>(native ? texture : texture + "_ui", AssetRequestMode.ImmediateLoad).Value;
        Vector2 center = position + TextureAssets.InventoryBack.Value.Size() * Main.inventoryScale * .5f;
        float scale = Main.inventoryScale * (native ? 32f / Math.Max(icon.Width, icon.Height) : 1);
        Main.spriteBatch.Draw(icon, center, null, Color.White * (hover ? 1 : .9f), 0, icon.Size() * .5f, scale, SpriteEffects.None, 0);
    }
}
