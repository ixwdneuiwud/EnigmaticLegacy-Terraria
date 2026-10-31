// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using EnigmaticLegacy.Content.Items;
using Microsoft.Xna.Framework;

namespace EnigmaticLegacy.Core;

public static class LegacyEyeVisual
{
    // The imported six-row strip follows the original mcmeta order: 0,1,2,3,2,1.
    // Its first frame lasts 48 source ticks; each remaining frame lasts two.
    public static int ActiveFrame(long ticks)
    {
        int phase = (int)((ticks % 174 + 174) % 174);
        return phase < 144 ? 0 : 1 + (phase - 144) / 6;
    }
    public static string Texture(ImportedItem eye) => "EnigmaticLegacy/Assets/Items/enigmatic_eye" +
        (eye.EyeAwakeningTicks > 0 ? eye.EyeAwakeningTicks > 6 ? "_dormant_1" : "_dormant_2" : eye.Tainted ? "" : "_dormant");
    public static Rectangle Frame(ImportedItem eye, int width, int height, long ticks) =>
        new(0, eye.Tainted && eye.EyeAwakeningTicks == 0 ? ActiveFrame(ticks) * width : 0, width, width);
}
