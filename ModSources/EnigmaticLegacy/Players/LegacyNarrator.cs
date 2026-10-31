// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaticLegacy.Core;
using EnigmaticLegacy.Systems;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace EnigmaticLegacy.Players;

public sealed class LegacyNarrator : ModPlayer
{
    public bool Unlocked, DeathFromCreature, RingDestroyed;
    public readonly HashSet<string> Heard = new(StringComparer.Ordinal);
    private string previousRealm = "";
    private string lastQuote = "";
    internal static readonly string[] Intros = { "another_demigod", "another_eon", "perhaps_you" };
    internal static readonly string[] Deaths = { "no_peril", "only_because", "death_may", "demise_is", "we_fall", "you_will_endure", "oblivion_rejects", "setback" };
    internal static readonly string[] CreatureDeaths = Deaths.Concat(new[] { "eternity_to_keep", "immortal", "violence_calls" }).ToArray();
    public void Unlock(int delay)
    {
        if (Unlocked || Player.whoAmI != Main.myPlayer) return;
        Unlocked = true;
        QueueRandom(Intros, delay);
    }
    private void QueueRandom(string[] pool, int delay)
    {
        string[] choices = pool.Where(s => s != lastQuote).ToArray();
        string quote = choices[Main.rand.Next(choices.Length)];
        if (LegacyNarrative.Queue(quote, delay)) lastQuote = quote;
    }
    public void Once(string quote, int delay)
    {
        if (Unlocked && !Heard.Contains(quote) && LegacyNarrative.Queue(quote, delay)) Heard.Add(quote);
    }
    public override void OnEnterWorld()
    {
        if (Player.whoAmI != Main.myPlayer) return;
        previousRealm = LegacyRealms.At(Player.Center);
        LegacyNarrative.Clear();
    }
    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer) return;
        if (Player.GetModPlayer<LegacyPlayer>().Has("enigmatic_eye")) Unlock(180);
        string realm = LegacyRealms.At(Player.Center);
        if (previousRealm != "" && previousRealm != realm && realm == LegacyRealms.Nether) Once("sulfur_air", 720);
        previousRealm = realm;
    }
    public override void OnRespawn()
    {
        if (Player.whoAmI != Main.myPlayer || !Unlocked) return;
        if (RingDestroyed) { QueueRandom(new[] { "toll_paid", "its_destruction" }, 30); RingDestroyed = false; }
        else if (Main.rand.NextFloat() < 0.8f) QueueRandom(DeathFromCreature ? CreatureDeaths : Deaths, 30);
    }
    public override void SaveData(TagCompound tag)
    {
        tag["unlocked"] = Unlocked; tag["heard"] = Heard.ToList();
        tag["ringDestroyed"] = RingDestroyed; tag["deathCreature"] = DeathFromCreature;
    }
    public override void LoadData(TagCompound tag)
    {
        Unlocked = tag.GetBool("unlocked"); Heard.Clear();
        foreach (string quote in tag.GetList<string>("heard")) Heard.Add(quote);
        RingDestroyed = tag.GetBool("ringDestroyed"); DeathFromCreature = tag.GetBool("deathCreature");
    }
}
