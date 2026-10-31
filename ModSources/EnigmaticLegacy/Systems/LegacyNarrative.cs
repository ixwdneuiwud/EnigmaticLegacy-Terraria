// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace EnigmaticLegacy.Systems;

internal sealed class LegacyQuote
{
    public float duration { get; set; }
    public float[] starts { get; set; }
    public string[] en_us { get; set; }
    public string[] zh_cn { get; set; }
}
public sealed class LegacyNarrative : ModSystem
{
    private static Dictionary<string, LegacyQuote> quotes;
    private static readonly Queue<(string Name, int Delay)> Pending = new();
    private static string current;
    private static int delay, elapsed;
    public static int ClipCount => quotes?.Count ?? 0;
    public static int PendingCount => Pending.Count;
    public override void Load() => quotes = JsonSerializer.Deserialize<Dictionary<string, LegacyQuote>>(Encoding.UTF8.GetString(Mod.GetFileBytes("Data/quotes.json")));
    public static bool Queue(string name, int ticks)
    {
        if (quotes == null || !quotes.ContainsKey(name) || Pending.Count >= 8) return false;
        Pending.Enqueue((name, Math.Max(1, ticks))); return true;
    }
    public static void Clear() { Pending.Clear(); current = null; delay = elapsed = 0; }
    public override void OnWorldUnload() => Clear();
    public override void Unload() { Clear(); quotes = null; }
    public override void PostUpdateEverything()
    {
        if (Main.dedServ || Main.gameMenu) return;
        if (current == null)
        {
            if (!Pending.TryDequeue(out var next)) return;
            current = next.Name; delay = next.Delay; elapsed = 0;
        }
        if (delay > 0)
        {
            if (--delay == 0 && ModContent.GetInstance<LegacyClientConfig>().NarratorVoice)
                SoundEngine.PlaySound(new SoundStyle("EnigmaticLegacy/Assets/Sounds/Quotes/" + current) { Volume = 0.7f, MaxInstances = 1 });
            return;
        }
        if (++elapsed >= quotes[current].duration * 60) current = null;
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.dedServ) return;
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0) index = layers.Count;
        layers.Insert(index, new LegacyGameInterfaceLayer("EnigmaticLegacy: Narrator", Draw, InterfaceScaleType.UI));
    }
    private static bool Draw()
    {
        if (current == null || delay > 0 || !ModContent.GetInstance<LegacyClientConfig>().NarratorSubtitles) return true;
        LegacyQuote quote = quotes[current];
        float seconds = elapsed / 60f;
        int line = 0;
        while (line + 1 < quote.starts.Length && quote.starts[line + 1] <= seconds) line++;
        string text = BridgeUnits.Text(quote.zh_cn[line], quote.en_us[line]);
        float alpha = MathHelper.Clamp(Math.Min(seconds / 0.5f, (quote.duration - seconds) / 0.5f), 0, 1);
        int width = Math.Max(160, Math.Min(700, Main.screenWidth - 48));
        string[] lines = Utils.WordwrapString(text, FontAssets.MouseText.Value, width, 8, out int last);
        for (int i = 0; i <= last; i++)
            Utils.DrawBorderString(Main.spriteBatch, lines[i], new Vector2(Main.screenWidth / 2f, Main.screenHeight - 110 - (last - i) * 25), Color.White * alpha, 1, 0.5f);
        return true;
    }
}
