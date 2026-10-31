# 合成材料替代审核清单 · 0.1.17

下界合金锭、末影之眼与末影珍珠使用原版早期材料；末影珍珠现改用紫晶，作为前期可采掘的宝石素材，四条普通配方及召回药水酿造均同步替换。烈焰棒/粉统一为狱石锭并以消耗数量区分；幻翼膜按最新口径使用原版飞翔之魂（天空之魂）。保留原作配方结构、输出数量与七咒条件；泰拉合成无需九宫格，原输入出现次数乘以每份替代数量，同类材料汇总为一个堆叠。除烈焰棒每份换 2 锭外，其余普通映射每份换 1 件。

逐条的全部实际配方输入、数量、工作站和条件见 [139 条完整配方材料表](recipe-ingredients.md)。已恢复的 Java 隐藏配方包含非欧立方、飞升护符、轻蔑之约、建筑师卷轴；千咒卷轴的 JSON 隐藏配方也已注册。

## 本轮调整与倒转之启进度

| 原材料 | 此前替代 | 当前替代 | 每份用量 / 进度 |
| --- | --- | --- | --- |
| 下界合金锭 | 夜明锭 | 魔矿锭（`DemoniteBar`） / 猩红矿锭（`CrimtaneBar`） | 1 锭；魔矿或猩红矿均可，困难模式前可得 |
| 末影之眼 | 视域之魂 | 晶状体（`Lens`） | 1 个；恶魔眼等早期敌怪掉落 |
| 末影珍珠 | 传送药水 | 紫晶（`Amethyst`） | 1 块；困难模式前可采掘的宝石素材，四条普通配方和召回药水酿造均采用此映射 |
| 烈焰粉 | 活火块 | 狱石锭（`HellstoneBar`） | 1 锭；困难模式前可采掘和熔炼 |
| 烈焰棒 | 狱石锭 | 狱石锭（`HellstoneBar`） | 2 锭；按原作 1 棒 = 2 粉的比例区分消耗 |
| 幻翼膜 | 羽毛（更早为灵气） | 飞翔之魂（`SoulofFlight`） | 1 个；原版飞翔之魂，即本次指定的天空之魂，困难模式高空飞龙掉落；原配方中的羽毛仍单独保留 |

倒转之启当前完整配方：邪恶精粹 ×4、魔矿锭或猩红矿锭 ×2、扭曲之心 ×1、电线 ×1、神秘手册 ×1。扭曲之心当前配方：大地之心 ×1、狱石锭 ×2、电线 ×2、晶状体 ×1、暗影之魂 ×1。两者均保留七咒条件。邪恶精粹仍须实际佩戴七咒击杀血肉之墙获得，每次固定 10 个；暗影之魂也在肉山后可获取，不要求机械 Boss。下界合金锭、末影之眼及活火块的旧替代不再阻塞制作。

## 优先审核：会影响获取阶段的替代

上表列出已确定的六项重点映射，下表记录其余保留的阶段差异。

| 原材料 | 当前替代 | 阶段或含义影响 |
| --- | --- | --- |
| 钻石斧 | 斧（`TheAxe`） | 吉他斧来自世纪之花，且是稀有掉落；目前用于行刑者之斧配方 |
| 附魔金苹果 | 生命果（`LifeFruit`） | 生命果要求困难模式进度；用于飞升护符隐藏配方，附魔本身不实现 |
| 末影箱 | 虚空保险库（`VoidVault`） | 虚空保险库的取得与 MC 末影箱不同；用于末影之戒 |
| 龙息 | 星云碎片（`FragmentNebula`） | 改为四柱材料，影响相关滞留药水配方 |

## 全部普通材料映射

同一行用“/”列出的铁/铅、金/铂金、魔矿/猩红矿属于原生配方组，任选或混合同数量即可；不是同时消耗两种。一个材料参与多条配方时，所有输出列在最后一栏。

| MC 材料 / 标签 | 泰拉替代物 | 数量关系 | 涉及的已注册输出 |
| --- | --- | --- | --- |
| 书（`minecraft:book`） | 书（`Book`） | 1 → 1 | 兽友指南（`animal_guidebook`）、求知之书（`enchantment_transposer`）、野猎指南（`hunter_guidebook`）、启示之证（`the_acknowledgment`） |
| 金粒（`minecraft:gold_nugget`） | 金矿（`GoldOre`） / 铂金矿（`PlatinumOre`） | 1 → 1 | 兽友指南（`animal_guidebook`）、传送薯（`astral_potato`）、求知之书（`enchantment_transposer`）、末影之戒（`ender_ring`）、超维之眼（`extradimensional_eye`）、精美戒指（`golden_ring`）、野猎指南（`hunter_guidebook`）、大灯笼（`big_lamp`）、菌光体灯笼（`big_shroomlamp`） |
| 金锭（`minecraft:gold_ingot`） | 金锭（`GoldBar`） / 铂金锭（`PlatinumBar`） | 1 → 1 | 无尽贪婪契约（`avarice_scroll`）、末影之戒（`ender_ring`）、超维之眼（`extradimensional_eye`）、精美戒指（`golden_ring`）、天堂之礼（`heaven_scroll`）、迷人徽章（`insignia`）、磁力之戒（`magnet_ring`）、猎宝者护符（`mining_charm`）、被诅咒者的寻路指针（`soul_compass`）、错位之戒（`super_magnet_ring`） |
| 铁粒（`minecraft:iron_nugget`） | 铁矿（`IronOre`） / 铅矿（`LeadOre`） | 1 → 1 | 铁指环（`iron_ring`）、猎宝者护符（`mining_charm`） |
| 铁锭（`minecraft:iron_ingot`） | 铁锭（`IronBar`） / 铅锭（`LeadBar`） | 1 → 1 | 铁指环（`iron_ring`）、大灯笼（`big_lamp`）、菌光体灯笼（`big_shroomlamp`）、磁力之戒（`magnet_ring`）、扭曲魔镜（`twisted_mirror`） |
| 蒲公英（`minecraft:dandelion`） | 太阳花（`Daybloom`） | 1 → 1 | 兽友指南（`animal_guidebook`） |
| 虞美人（`minecraft:poppy`） | 闪耀根（`Blinkroot`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 苹果（`minecraft:apple`） | 苹果（`Apple`） | 1 → 1 | 兽友指南（`animal_guidebook`）、狂野生长精华（`infinimeal`） |
| 烤马铃薯（`minecraft:baked_potato`） | 薯片（`PotatoChips`） | 1 → 1 | 传送薯（`astral_potato`） |
| 羽毛（`minecraft:feather`） | 羽毛（`Feather`） | 1 → 1 | 无尽贪婪契约（`avarice_scroll`）、千咒卷轴（`cursed_scroll`）、死后召回卷轴（`escape_scroll`）、天堂之礼（`heaven_scroll`）、设计师的墨水（`lore_inscriber`）、永恒智慧卷轴（`xp_scroll`） |
| 墨囊（`minecraft:ink_sac`） | 黑墨水（`BlackInk`） | 1 → 1 | 无尽贪婪契约（`avarice_scroll`）、千咒卷轴（`cursed_scroll`）、死后召回卷轴（`escape_scroll`）、天堂之礼（`heaven_scroll`）、设计师的墨水（`lore_inscriber`）、永恒智慧卷轴（`xp_scroll`） |
| 绯红菌（`minecraft:crimson_fungus`） | 毒蘑菇（`ViciousMushroom`） | 1 → 1 | 血战沙场之证（`berserk_charm`） |
| 诡异菌（`minecraft:warped_fungus`） | 发光蘑菇（`GlowingMushroom`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 金剑（`minecraft:golden_sword`） | 金阔剑（`GoldBroadsword`） / 铂金宽剑（`PlatinumBroadsword`） | 1 → 1 | 血战沙场之证（`berserk_charm`） |
| 烈焰粉（`minecraft:blaze_powder`） | 狱石锭（`HellstoneBar`） | 1 → 1 | 血战沙场之证（`berserk_charm`）、寰宇之心（`cosmic_heart`）、附魔师的珍珠（`enchanter_pearl`）、求知之书（`enchantment_transposer`）、超维之眼（`extradimensional_eye`）、行刑者之斧（`forbidden_axe`）、烈焰之傲壁垒（`infernal_shield`）、设计师的墨水（`lore_inscriber`）、修补混合物（`mending_mixture`）、怪物猎人勋章（`monster_charm`）、被诅咒者的寻路指针（`soul_compass`）、扭曲之心（`twisted_heart`） |
| 烈焰棒（`minecraft:blaze_rod`） | 狱石锭（`HellstoneBar`） | 1 → 2 | 末影棍（`ender_rod`）、行刑者之斧（`forbidden_axe`）、烈焰之傲壁垒（`infernal_shield`） |
| 恶魂之泪（`minecraft:ghast_tear`） | 暗影之魂（`SoulofNight`） | 1 → 1 | 血战沙场之证（`berserk_charm`）、噬咒之书（`curse_transposer`）、饕餮之锅（`eldritch_pan`）、末影之屠（`ender_slayer`）、极恶锭（`evil_ingot`）、修补混合物（`mending_mixture`）、猎宝者护符（`mining_charm`）、扭曲之心（`twisted_heart`） |
| 下界合金锭（`minecraft:netherite_ingot`） | 魔矿锭（`DemoniteBar`） / 猩红矿锭（`CrimtaneBar`） | 1 → 1 | 血战沙场之证（`berserk_charm`）、荒芜之负（`desolation_ring`）、极恶锭（`evil_ingot`）、行刑者之斧（`forbidden_axe`）、烈焰之傲壁垒（`infernal_shield`）、怪物猎人勋章（`monster_charm`）、被诅咒者的寻路指针（`soul_compass`）、无止之言（`the_infinitum`）、倒转之启（`the_twist`）、轻蔑之约（`eldritch_amulet`） |
| 海洋之心（`minecraft:heart_of_the_sea`） | 海神贝壳（`NeptunesShell`） | 1 → 1 | 寰宇之心（`cosmic_heart`） |
| 幻翼膜（`minecraft:phantom_membrane`） | 飞翔之魂（`SoulofFlight`） | 1 → 1 | 千咒卷轴（`cursed_scroll`）、噬咒之书（`curse_transposer`）、死后召回卷轴（`escape_scroll`）、超维之眼（`extradimensional_eye`）、修补混合物（`mending_mixture`） |
| 红石粉（`minecraft:redstone`） | 电线（`Wire`） | 1 → 1 | 千咒卷轴（`cursed_scroll`）、噬咒之书（`curse_transposer`）、求知之书（`enchantment_transposer`）、磁力之戒（`magnet_ring`）、倒转之启（`the_twist`）、扭曲之心（`twisted_heart`） |
| 荧石粉（`minecraft:glowstone_dust`） | 妖精尘（`PixieDust`） | 1 → 1 | 猎宝者护符（`mining_charm`） |
| 荧石（`minecraft:glowstone`） | 水晶碎块（`CrystalShard`） | 1 → 1 | 大灯笼（`big_lamp`） |
| 石头（`minecraft:stone`） | 石块（`StoneBlock`） | 1 → 1 | 邪恶之石（`cursed_stone`） |
| 熔岩桶（`minecraft:lava_bucket`） | 熔岩桶（`LavaBucket`） | 1 → 1 | 邪恶之石（`cursed_stone`） |
| 海晶砂粒（`minecraft:prismarine_crystals`） | 水晶碎块（`CrystalShard`） | 1 → 1 | 邪恶之石（`cursed_stone`）、求知之书（`enchantment_transposer`）、迷人徽章（`insignia`）、修补混合物（`mending_mixture`） |
| 绿宝石（`minecraft:emerald`） | 金币（`GoldCoin`） | 1 → 1 | 附魔师的珍珠（`enchanter_pearl`）、迷人徽章（`insignia`）、永恒智慧卷轴（`xp_scroll`） |
| 末影珍珠（`minecraft:ender_pearl`） | 紫晶（`Amethyst`） | 1 → 1 | 附魔师的珍珠（`enchanter_pearl`）、末影之戒（`ender_ring`）、末影棍（`ender_rod`）、野猎指南（`hunter_guidebook`） |
| 末影之眼（`minecraft:ender_eye`） | 晶状体（`Lens`） | 1 → 1 | 星光破坏锤（`astral_breaker`）、寰宇之心（`cosmic_heart`）、末影之屠（`ender_slayer`）、维度锚（`end_anchor`）、超维之眼（`extradimensional_eye`）、迷人徽章（`insignia`）、错位之戒（`super_magnet_ring`）、扭曲之心（`twisted_heart`）、永恒智慧卷轴（`xp_scroll`） |
| 龙息（`minecraft:dragon_breath`） | 星云碎片（`FragmentNebula`） | 1 → 1 | 星光破坏锤（`astral_breaker`）、壮丽鞘翅（`enigmatic_elytra`）、造物主的恩赐（`fabulous_scroll`）、修补混合物（`mending_mixture`）、飞升护符（`ascension_amulet`） |
| 哭泣的黑曜石（`minecraft:crying_obsidian`） | 黑曜石（`Obsidian`） | 1 → 1 | 附魔师的珍珠（`enchanter_pearl`） |
| 青金石（`minecraft:lapis_lazuli`） | 萨菲（`Sapphire`） | 1 → 1 | 求知之书（`enchantment_transposer`）、天堂之礼（`heaven_scroll`）、错位之戒（`super_magnet_ring`） |
| 末影箱（`minecraft:ender_chest`） | 虚空保险库（`VoidVault`） | 1 → 1 | 末影之戒（`ender_ring`） |
| 木棍（`minecraft:stick`） | 木材（`Wood`） | 1 → 1 | 末影之屠（`ender_slayer`）、物品描述片段（`lore_fragment`）、空卷轴（`thicc_scroll`） |
| 黑曜石（`minecraft:obsidian`） | 黑曜石（`Obsidian`） | 1 → 1 | 末影之屠（`ender_slayer`）、烈焰之傲壁垒（`infernal_shield`）、非欧立方（`the_cube`） |
| 末地石（`minecraft:end_stone`） | 珍珠石块（`PearlstoneBlock`） | 1 → 1 | 维度锚（`end_anchor`） |
| 鞘翅（`minecraft:elytra`） | 天使之翼（`AngelWings`） | 1 → 1 | 壮丽鞘翅（`enigmatic_elytra`）、造物主的恩赐（`fabulous_scroll`） |
| 钻石（`minecraft:diamond`） | 钻石（`Diamond`） | 1 → 1 | 以太阔剑（`etherium_sword`）、磁力之戒（`magnet_ring`）、猎宝者护符（`mining_charm`） |
| 钻石斧（`minecraft:diamond_axe`） | 斧（`TheAxe`） | 1 → 1 | 行刑者之斧（`forbidden_axe`） |
| 凋灵骷髅头颅（`minecraft:wither_skeleton_skull`） | 骷髅王面具（`SkeletronMask`） | 1 → 1 | 行刑者之斧（`forbidden_axe`） |
| 骷髅头颅（`minecraft:skeleton_skull`） | 骷髅头（`Skull`） | 1 → 1 | 怪物猎人勋章（`monster_charm`） |
| 皮革（`minecraft:leather`） | 皮革（`Leather`） | 1 → 1 | 野猎指南（`hunter_guidebook`） |
| 骨头（`minecraft:bone`） | 骨头（`Bone`） | 1 → 1 | 野猎指南（`hunter_guidebook`） |
| 盾牌（`minecraft:shield`） | 钴护盾（`CobaltShield`） | 1 → 1 | 烈焰之傲壁垒（`infernal_shield`） |
| 可可豆（`minecraft:cocoa_beans`） | 椰子（`Coconut`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 垂泪藤（`minecraft:weeping_vines`） | 藤蔓（`Vine`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 小麦（`minecraft:wheat`） | 干草（`Hay`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 下界疣（`minecraft:nether_wart`） | 火焰花（`Fireblossom`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 睡莲（`minecraft:lily_pad`） | 幌菊（`Waterleaf`） | 1 → 1 | 狂野生长精华（`infinimeal`） |
| 命名牌（`minecraft:name_tag`） | 标牌（`Sign`） | 1 → 1 | 迷人徽章（`insignia`） |
| 锁链（`minecraft:chain`） | 链条（`Chain`） | 1 → 1 | 大灯笼（`big_lamp`）、菌光体灯笼（`big_shroomlamp`） |
| 菌光体（`minecraft:shroomlight`） | 发光蘑菇（`GlowingMushroom`） | 1 → 1 | 菌光体灯笼（`big_shroomlamp`） |
| 玻璃（`minecraft:glass`） | 玻璃（`Glass`） | 1 → 1 | 封装的大灯笼（`massive_lamp`）、封装的菌光体灯笼（`massive_shroomlamp`） |
| 玻璃瓶（`minecraft:glass_bottle`） | 玻璃瓶（`Bottle`） | 1 → 1 | 设计师的墨水（`lore_inscriber`） |
| 玻璃板（`minecraft:glass_pane`） | 玻璃（`Glass`） | 1 → 1 | 扭曲魔镜（`twisted_mirror`） |
| 纸（`minecraft:paper`） | 丝绸（`Silk`） | 1 → 1 | 物品描述片段（`lore_fragment`）、空卷轴（`thicc_scroll`） |
| 闪烁的西瓜片（`minecraft:glistering_melon_slice`） | 生命果（`LifeFruit`） | 1 → 1 | 修补混合物（`mending_mixture`） |
| 灵魂灯笼（`minecraft:soul_lantern`） | 妖灵瓶（`WispinaBottle`） | 1 → 1 | 怪物猎人勋章（`monster_charm`）、被诅咒者的寻路指针（`soul_compass`） |
| 指南针（`minecraft:compass`） | 罗盘（`Compass`） | 1 → 1 | 被诅咒者的寻路指针（`soul_compass`） |
| 灯笼（`minecraft:lantern`） | 挂链灯笼（`ChainLantern`） | 1 → 1 | 启示之证（`the_acknowledgment`） |
| 紫水晶碎片（`minecraft:amethyst_shard`） | 紫晶（`Amethyst`） | 1 → 1 | 飞升护符（`ascension_amulet`） |
| 附魔金苹果（`minecraft:enchanted_golden_apple`） | 生命果（`LifeFruit`） | 1 → 1 | 建筑师的青睐（`cosmic_scroll`） |
| 附魔书（`minecraft:enchanted_book`） | 魔法书（`SpellTome`） | 1 → 1 | 千咒卷轴（`cursed_scroll`） |
| 附魔之瓶（`minecraft:experience_bottle`） | 魔力药水（`ManaPotion`） | 1 → 1 | 怪物猎人勋章（`monster_charm`）、永恒智慧卷轴（`xp_scroll`） |
| minecraft:respawn_anchor（`minecraft:respawn_anchor`） | 床（`Bed`） | 1 → 1 | 维度锚（`end_anchor`） |

下列映射仍在数据表中，当前没有普通已注册配方消耗它们；不表示另外新增了对应 MC 物品。酿造直接使用泰拉物品的方案见后表。

| 备用 MC 材料 / 标签 | 泰拉替代物 |
| --- | --- |
| 紫颂果（`minecraft:chorus_fruit`） | 火龙果（`Dragonfruit`） |
| 蛋糕（`minecraft:cake`） | 蛋糕块（`SliceOfCake`） |
| 红石灯（`minecraft:redstone_lamp`） | 彩线灯泡（`WireBulb`） |
| 鹦鹉螺壳（`minecraft:nautilus_shell`） | 贝壳（`Seashell`） |
| 海绵（`minecraft:sponge`） | 超级吸水棉（`SuperAbsorbantSponge`） |
| 铁锭标签（`forge:ingots/iron`） | 铁锭（`IronBar`） / 铅锭（`LeadBar`） |
| 金锭标签（`forge:ingots/gold`） | 金锭（`GoldBar`） / 铂金锭（`PlatinumBar`） |
| 钻石标签（`forge:gems/diamond`） | 钻石（`Diamond`） |
| 绿宝石标签（`forge:gems/emerald`） | 金币（`GoldCoin`） |

## 特殊材料和工作站

- 下界之星是独立材料，佩戴七咒击杀血肉之墙必掉 1 个；使用原 MC 贴图，不再用日耀碎片替代。
- 以太矿 → 以太锭为 1 → 1，只能使用精金熔炉或钛金熔炉；两条原熔炼/高炉配方均接入同一三级熔炉工作站。
- 其他原工作台配方接泰拉工作台；酿造接放置的瓶子工作站。
- 飞升护符接受七种个人神秘护符，并保留绑定主人；轻蔑之约继续保留升级护符的主人。
- 建筑师卷轴仍要求受祝福的天宇之心，祝福途径尚未指定；不把它记作已能制作。
- 以太块、袋装星辰的压缩与拆分配方均已注册；海绵按要求不提供配方和获取。

## 酿造材料替代

每条配方输出 1 瓶，下面列出的每种输入均消耗 1 件；普通/终极和饮用/喷溅/滞留均有稳定物品 ID。末影药水类、火焰材料、强化与延时均为合成输入，不由此实现 MC 饥饿、经验或附魔机制。

| 原逻辑 / 成品 | 当前泰拉输入 |
| --- | --- |
| 普通急迫 | 水瓶 + 火焰花 + 水晶碎块（替代粗制药水与下界石英） |
| 普通熔火之心 | 黑曜石皮药水 + 狱石锭 |
| 延长急迫 / 延长熔火之心 | 相应普通药水 + 电线（替代红石） |
| 强化急迫 | 普通急迫 + 妖精尘（替代荧石粉） |
| 终极急迫 / 熔火之心 | 延长或强化急迫 + 星尘 / 延长熔火之心 + 星尘 |
| 终极夜视 / 隐身 / 跳跃 / 抗火 | 夜猫子药水 / 隐身药水 / 羽落药水 / 黑曜石皮药水，分别 + 星尘 |
| 终极迅捷 / 水肺 / 治疗 / 再生 / 力量 / 缓降 | 敏捷药水 / 鱼鳃药水 / 治疗药水 / 再生药水 / 怒气药水 / 羽落药水，分别 + 星尘 |
| 终极缓慢 / 神龟 / 伤害 / 虚弱 | 敏捷药水 / 耐力药水 / 治疗药水 / 水瓶，分别 + 死亡草 + 星尘 |
| 终极剧毒 | 水瓶 + 毒刺 + 星尘 |
| 喷溅 | 对应饮用药水 + 爆炸粉（替代火药） |
| 滞留 | 对应喷溅药水 + 星云碎片（替代龙息） |
| 召回 / 扭曲药水 | 水瓶 + 火焰花 + 紫晶（替代末影珍珠） / 召回药水 + 扭曲之心 |

附魔书 → 魔法书、附魔之瓶 → 魔力药水只影响配方输入；经验存取和附魔操作仍留空。材料表从实际已加载的 139 条配方及映射生成，武器数值见 [武器与工具适配表](weapons.md)。
