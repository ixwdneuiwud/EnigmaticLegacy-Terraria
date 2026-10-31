# Attribution and asset licenses

The original Enigmatic Legacy code, names, text, textures, animations and sound assets belong to **Aizistral / Aizistral Studios and their contributors**. This is an unofficial, GPT-assisted Terraria adaptation, maintained here by ixwdneuiwud; no original authorship is claimed.

- Original project and distribution source: https://github.com/Aizistral-Studios/Enigmatic-Legacy (branch `1.20.X`, source release 2.30.0).
- Original license: [Enigmatic Legacy License, II Edition](LICENSE-UPSTREAM.md), unchanged. The adaptation's source-availability and downstream permission terms are in [LICENSE.md](LICENSE.md).
- Modifications dated **2026-10-31**: the Java/Forge implementation was rewritten in C# for tModLoader; original resources were scaled/packed for 2D animation; native equipment/prefix, status, crafting, persistence, multiplayer, and optional Calamity hooks replace Minecraft integrations. These are substantial changes, not an official version of the original mod.

**Curios slot glyphs.** `Assets/Slots/{back,body,bracelet,charm,curio,head,necklace,ring,cosmetic}*.png` in the mod source are adapted from Curios 5.12.1 by TheIllusiveC4/C4, Copyright (C) 2018-2020 C4, under **LGPL-3.0-or-later**. Originals are included in `third_party/curios/slots`. Changes: nearest-neighbor resize to 32×32; `_ui` variants raise RGB components to at least 192 and preserve alpha. Source: https://github.com/TheIllusiveC4/Curios/tree/1.20.x . License notice and GNU license texts are in `licenses/`. These assets retain their license independently of the C# code.

**Minecraft vanilla icons.** Nether Star, equipment boot-slot and Totem of Undying artwork remain the property of Mojang/Microsoft. Where used by this module, these icons are restored from a local Minecraft 1.20.1 client for the binary build and excluded from Git. Their original entries and pixel hashes are recorded in `external-assets.json`; restore with `tools/restore_external_assets.py`. The original mod licenses do not relicense Minecraft artwork. This notice records provenance and does not claim permission from Mojang/Microsoft or an official affiliation.

**Audio actually included.** All 29 OGG files match the corresponding Enigmatic Legacy 2.30.0 JAR entries byte-for-byte: 28 original `sounds/quote/*.ogg` files and `sounds/misc/hhon.ogg` renamed `charged_on.ogg`. No pan-clang/shovel audio is present in this snapshot. The original mod additionally credits Timbre (`Cartoony Clangs`, https://freesound.org/s/91696/, CC BY-NC 4.0) and snardin42 (`Shovel Thwack 1`, https://freesound.org/s/73200/, CC0); those files are not included here. Do not assume the mod's license overrides those credits if adding their files later.

The original mod's credits also acknowledge frdovahkick, SkySem and SoulRacer65 for textures/models; Terraria and its mods, Minecraft Dungeons, and Vazkii/Botania as inspirations or original contributions. Original credits are preserved in `UPSTREAM-CREDITS.txt`. Third-party material retains its original rights.

No Minecraft client/JAR, Forge, Curios implementation, Caelus, Patchouli, Terraria, tModLoader, Calamity, other port, user saves or credentials are distributed in this repository. Installed game/runtime dependencies are obtained separately from their own distributions.
