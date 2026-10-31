// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.Linq;
using EnigmaticLegacy.Content.Items;
using EnigmaticLegacy.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EnigmaticLegacy.Players;

public sealed class LegacyVoidConsumption : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server || Main.GameUpdateCount % 12 != 0) return;
        foreach (Item stone in Player.inventory)
            if (stone.ModItem is ImportedItem { ImportId: "void_stone", Disabled: false }) Consume(Player, stone);
    }
    public static void Consume(Player player, Item stone)
    {
        if (stone.ModItem is not ImportedItem { ImportId: "void_stone", Disabled: false }) return;
        var data = stone.GetGlobalItem<ItemMetadata>();
        if (data.VoidFilters.Count == 0) return;
        int limit = System.Math.Min(50, player.inventory.Length);
        bool full = player.inventory.Take(limit).All(i => !i.IsAir);
        if (data.VoidMode == 2 && !full) return;
        var candidates = Enumerable.Range(0, limit).Where(n => !player.inventory[n].IsAir &&
            !ReferenceEquals(player.inventory[n], stone) && !player.inventory[n].favorited &&
            data.VoidFilters.Contains(ItemMetadata.ItemKey(player.inventory[n]))).ToArray();
        var remove = data.VoidMode switch {
            0 => candidates,
            1 => candidates.GroupBy(n => ItemMetadata.ItemKey(player.inventory[n]))
                .SelectMany(g => g.OrderByDescending(n => player.inventory[n].stack).ThenBy(n => n).Skip(1)).ToArray(),
            _ => candidates.OrderBy(n => player.inventory[n].stack).ThenByDescending(n => n).Take(1).ToArray()
        };
        foreach (int slot in remove)
        {
            player.inventory[slot].TurnToAir();
            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, slot);
        }
    }
    public static string DisplayName(string key)
    {
        if (key.StartsWith("Terraria/") && int.TryParse(key[9..], out int id) && id > 0 && id < ItemLoader.ItemCount) return Lang.GetItemNameValue(id);
        return ModContent.TryFind<ModItem>(key, out var item) ? Lang.GetItemNameValue(item.Type) : key;
    }
}
