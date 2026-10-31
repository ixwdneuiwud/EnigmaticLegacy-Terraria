// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using EnigmaticLegacy.Players;
using Terraria.GameContent;
using Terraria.UI.Chat;
using EnigmaticLegacy.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace EnigmaticLegacy.Systems;

public sealed class LegacyJournal : ModSystem
{
    private UserInterface journal;
    private JournalState state;
    public override void Load()
    {
        ImportCatalog.Load(Mod);
        if (Main.dedServ) return;
        journal = new UserInterface(); state = new JournalState(); state.Activate();
    }
    public static void Open()
    {
        if (Main.dedServ) return;
        var system = ModContent.GetInstance<LegacyJournal>();
        if (system.journal.CurrentState != null) system.journal.SetState(null);
        else { system.state.Refresh(); system.journal.SetState(system.state); }
    }
    public override void UpdateUI(GameTime gameTime)
    {
        if (journal?.CurrentState == null) return;
        if (Main.keyState.IsKeyDown(Keys.Escape)) { journal.SetState(null); return; }
        journal.Update(gameTime);
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index < 0) return;
        layers.Insert(index, new LegacyGameInterfaceLayer("EnigmaticLegacy: Journal", () =>
        {
            if (journal?.CurrentState != null) journal.Draw(Main.spriteBatch, new GameTime());
            return true;
        }, InterfaceScaleType.UI));
    }
    public override void Unload() { journal = null; state = null; LegacyBook.Clear(); ImportedRecipes.Reset(); }
    private sealed class JournalState : UIState
    {
        private UIText title, counter;
        private UIList contents;
        private UITextPanel<string> categoryButton;
        private JsonObject[] categories, entries;
        private int category, entry, page;
        public override void OnInitialize()
        {
            categories = LegacyBook.Data["categories"].AsArray().Select(n => n.AsObject()).OrderBy(n => (int?)n["sortnum"] ?? 0).ToArray();
            var panel = new UIPanel { Width = { Pixels = 780 }, Height = { Pixels = 560 }, HAlign = .5f, VAlign = .5f,
                BackgroundColor = new Color(27, 23, 49, 245) };
            panel.OnMouseOver += (_, _) => Main.LocalPlayer.mouseInterface = true;
            categoryButton = new UITextPanel<string>("") { Width = { Pixels = 220 }, Height = { Pixels = 36 }, Left = { Pixels = 6 }, Top = { Pixels = 4 } };
            categoryButton.OnLeftClick += (_, _) => { category = (category + 1) % categories.Length; entry = page = 0; Refresh(); };
            title = new UIText("", 1f, true) { Left = { Pixels = 242 }, Top = { Pixels = 10 } };
            contents = new UIList { Left = { Pixels = 12 }, Top = { Pixels = 58 }, Width = { Pixels = 708 }, Height = { Pixels = 404 }, ListPadding = 10 };
            var scroll = new UIScrollbar { Left = { Pixels = 726 }, Top = { Pixels = 58 }, Height = { Pixels = 404 } };
            scroll.SetView(100, 1000); contents.SetScrollbar(scroll);
            panel.Append(categoryButton); panel.Append(title); panel.Append(contents); panel.Append(scroll);
            counter = new UIText("") { HAlign = .5f, Top = { Pixels = 480 } }; panel.Append(counter);
            void Button(string name, float x, Action click)
            {
                var button = new UITextPanel<string>(name) { Width = { Pixels = 130 }, Height = { Pixels = 32 }, Left = { Pixels = x }, Top = { Pixels = 505 } };
                button.OnLeftClick += (_, _) => click(); panel.Append(button);
            }
            Button(BridgeUnits.Text("上一条目", "Previous entry"), 4, () => { entry = (entry + entries.Length - 1) % entries.Length; page = 0; Refresh(); });
            Button(BridgeUnits.Text("上一页", "Previous page"), 154, () => { page--; Refresh(); });
            Button(BridgeUnits.Text("下一页", "Next page"), 468, () => { page++; Refresh(); });
            Button(BridgeUnits.Text("下一条目", "Next entry"), 618, () => { entry = (entry + 1) % entries.Length; page = 0; Refresh(); });
            // Loading UI assets precedes initialization of player ModPlayer data.
            // Refresh is deferred until a player actually opens the book.
            Append(panel);
        }
        private void Text(string text, Color? color = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            string[] lines = Utils.WordwrapString(text, FontAssets.MouseText.Value, 690, 1000, out int count);
            string wrapped = string.Join("\n", lines.Take(count + 1));
            var element = new UIText(wrapped, .95f) { Width = { Pixels = 700 }, Height = { Pixels = Math.Max(28, (count + 2) * 28) }, TextColor = color ?? Color.White };
            contents.Add(element);
        }
        private void Item(int type)
        {
            if (type <= 0) return;
            Text(Lang.GetItemNameValue(type), Color.Gold);
        }
        private void RecipePage(string source)
        {
            string key = source.Split(':').Last();
            string output = ImportedRecipes.RegisteredSources.FirstOrDefault(k => k.Key.EndsWith("/" + key + ".json")).Value ?? key;
            if (!ImportCatalog.TryType(output, out int type)) return;
            foreach (Recipe recipe in Main.recipe.Take(Recipe.numRecipes).Where(r => r.createItem.type == type))
            {
                Text(string.Join(" + ", recipe.requiredItem.Where(i => !i.IsAir).Select(i => i.Name + " ×" + i.stack)) + " → " + recipe.createItem.Name + " ×" + recipe.createItem.stack, Color.LightGoldenrodYellow);
            }
        }
        public void Refresh()
        {
            if (categories == null) return;
            categoryButton.SetText(LegacyBook.Text(LegacyBook.Local(categories[category])["name"]));
            string categoryId = "enigmaticlegacy:" + (string)categories[category]["id"];
            var research = Main.LocalPlayer.GetModPlayer<LegacyResearch>();
            entries = LegacyBook.Entries.Where(n => (string)n["category"] == categoryId &&
                ((bool?)n["secret"] != true || research.Discovered.Contains((string)n["id"]) || Main.LocalPlayer.difficulty == 3)).ToArray();
            contents.Clear();
            if (entries.Length == 0) { title.SetText(""); Text(LegacyBook.Text(LegacyBook.Local(categories[category])["description"])); return; }
            entry = Math.Clamp(entry, 0, entries.Length - 1); var row = LegacyBook.Local(entries[entry]);
            title.SetText(LegacyBook.Text(row["name"]));
            JsonArray pages = row["pages"].AsArray(); page = (page % pages.Count + pages.Count) % pages.Count;
            var current = pages[page].AsObject();
            Text(LegacyBook.Text(current["title"] ?? current["heading"]), Color.Gold);
            if (current["item"] != null) Item(LegacyBook.ItemType((string)current["item"]));
            foreach (string key in new[] { "input1", "input2", "input3", "input4" })
                if (current[key] != null) Item(LegacyBook.ItemType((string)current[key]));
            if (current["recipe"] != null) RecipePage((string)current["recipe"]);
            if (current["recipe2"] != null) RecipePage((string)current["recipe2"]);
            if (current["images"] is JsonArray images)
                foreach (JsonNode path in images)
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension((string)path);
                    if (!ImportCatalog.Owner.FileExists("Assets/Book/" + filename + ".png") && !ImportCatalog.Owner.FileExists("Assets/Book/" + filename + ".rawimg")) continue;
                    var image = new UIImage(ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>("EnigmaticLegacy/Assets/Book/" + filename)) { HAlign = .5f };
                    contents.Add(image);
                }
            Text(LegacyBook.Text(current["text"]));
            research.ReadPages.Add((string)entries[entry]["id"] + ":" + page);
            counter.SetText($"{entry + 1}/{entries.Length} · {page + 1}/{pages.Count}");
        }
    }
}
