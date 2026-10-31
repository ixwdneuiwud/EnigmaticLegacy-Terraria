// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Core;

public static class TooltipLayout
{
    public static int ScreenWidth => Math.Clamp(Main.screenWidth - 100, 120, 1100);
    public static float FitScale(Vector2 size, int screenWidth, int screenHeight) =>
        Math.Min(1, Math.Min(Math.Max(1, screenWidth - 40) / Math.Max(1, size.X),
            Math.Max(1, screenHeight - 36) / Math.Max(1, size.Y)));
    // Terraria's color snippets must remain balanced across wrapping. Iterate
    // text elements so a CJK character, surrogate pair or combining mark cannot
    // be lost at the end of a line.
    public static List<string> Wrap(string text, int width, Func<string,float> measure = null)
    {
        float Measure(string glyph) => measure != null ? measure(glyph) : !Main.dedServ && FontAssets.MouseText?.IsLoaded == true
            ? FontAssets.MouseText.Value.MeasureString(glyph).X : glyph[0] < 128 ? 9 : 18;
        var lines = new List<string>(); var line = new StringBuilder(); var run = new StringBuilder();
        string color = ""; float length = 0;
        void FlushRun() { if(run.Length==0)return; line.Append(color=="" ? run.ToString() : "[c/"+color+":"+run+"]"); run.Clear(); }
        void FlushLine() { FlushRun(); lines.Add(line.ToString()); line.Clear(); length=0; }
        foreach(Match segment in Regex.Matches(text, @"\[c/([0-9a-fA-F]{6}):([^\]]*)\]|([^\[]+|\[)"))
        {
            FlushRun(); color=segment.Groups[1].Success ? segment.Groups[1].Value : "";
            string value=segment.Groups[1].Success ? segment.Groups[2].Value : segment.Groups[3].Value;
            var glyphs=StringInfo.GetTextElementEnumerator(value);
            while(glyphs.MoveNext())
            {
                string glyph=glyphs.GetTextElement();
                if(glyph=="\r")continue;
                if(glyph=="\n") { FlushLine(); continue; }
                float size=Measure(glyph);
                if(length>0 && length+size>width)FlushLine();
                run.Append(glyph); length+=size;
            }
        }
        if(run.Length>0 || line.Length>0 || lines.Count==0)FlushLine();
        return lines;
    }
    public static List<TooltipLine> Wrap(IEnumerable<TooltipLine> lines, int width)
    {
        var output=new List<TooltipLine>();
        foreach(var line in lines)
        {
            int part=0;
            foreach(string text in Wrap(line.Text,width))
                output.Add(new TooltipLine(line.Mod=="Terraria" ? ModContent.GetInstance<EnigmaticLegacy>() : ModLoader.GetMod(line.Mod), line.Name+"_"+part++,text) { OverrideColor=line.OverrideColor });
        }
        return output;
    }
}
