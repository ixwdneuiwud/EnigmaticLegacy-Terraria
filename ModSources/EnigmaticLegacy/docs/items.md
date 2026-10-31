# 逐物品移植速查 · 0.1.17

本体数据目录共 182 个物品变体，包含四种原作灯笼与维度锚；另有袋装星辰、以太块。三种红石方块按既定口径暂停。这里是当前效果速查；完整 MC 依赖、未实现及不等价内容见 [机制审计](mechanism-audit.md)。获取栏结合实测配方与本轮掉落；完整概率和条件见 [获取/掉率表](acquisition-rules.md)，尚缺来源见 [待补清单](acquisition-gaps.md)。

| MC ID / 泰拉内部 ID | 名称 | 类别 | 当前行为与留空项 | 获取 / 配方 |
|---|---|---|---|---|
| enigmatic_item | 创造之心 | spellstone | 创造之心：自由飞行、环境伤害防护、负面状态免疫<br>不会因致命伤害死亡<br>V：发射追踪凋零之颅 | 暂无生存来源，见待补清单/机制审计 |
| xp_scroll | 永恒智慧卷轴 | accessory | 经验储存与提取：暂留空 | 1 条配方 |
| enigmatic_amulet | 神秘护身符 | accessory | +1.5 点原作攻击伤害<br>死亡掉落存入原地的超维容器（沿用角色死亡模式） | 无主护符随机转化，七色之一 |
| magnet_ring | 磁力之戒 | accessory | 吸引 8 格范围内的物品<br>M 键切换；按住下键暂停吸引 | 1 条配方 |
| extradimensional_eye | 超维之眼 | utility | 右键绑定当前位置；左键点击 6 格内生物，将其送到绑定地点并消耗此物<br>目标必须与绑定地点同维度；地狱高度区视作下界<br>保护 Boss 与多节敌怪的 AI | 1 条配方 |
| relic_of_testing | Relic Of Testing | material | 原作隐藏调试物品 | 暂无生存来源，见待补清单/机制审计 |
| recall_potion | 召回药水 | utility | 传送到床或世界出生点 | 1 条配方 |
| forbidden_axe | 行刑者之斧 | weapon | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| escape_scroll | 死后召回卷轴 | accessory | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| heaven_scroll | 天堂之礼 | accessory | 同维度水晶球周围 50 格内获得翅膀飞行<br>持续飞行每秒消耗 2 魔力 | 1 条配方 |
| super_magnet_ring | 错位之戒 | accessory | 将 16 格范围内的物品传送到你身边<br>M 键切换；按住下键暂停 | 1 条配方 |
| golem_heart | 魔像之心 | spellstone | +4 护甲点；未穿盔甲时改为 +16<br>近战伤害抗性 25%，无甲时爆炸抗性 40%<br>免疫击退与灼热地面；受到魔法伤害翻倍<br>V：无主动技能 | 宝箱 20%（dungeon_gold） |
| mega_sponge | 无内海绵 | accessory | 彩蛋物品：沿用原贴图<br>吸水效果与获取渠道留空 | 按约定无获取的彩蛋 |
| unholy_grail | 不洁圣杯 | utility | 可无限次饮用，不消耗圣杯<br>已食用禁忌之果且承受七咒时：再生 III 25 秒、吸收 II 40 秒、力量 II 50 秒、速度 I 与抗性 II 60 秒<br>否则：凋零 III 5 秒、中毒 II 8 秒、虚弱 II 10 秒、困惑与缓慢 I 12 秒<br>未食用禁忌之果时额外给予虚弱（泰拉）8 秒，替代饥饿 III | 宝箱 20%（dungeon） |
| eye_of_nebula | 星云之眼 | spellstone | +40% 魔法伤害；65% 魔法伤害抗性<br>15% 概率闪避并传送；在水中受到双倍伤害<br>V：闪到目标身后，下一击伤害 +150%；冷却 3 秒 | 敌怪 100%（CultistBoss，条件见掉率表） |
| blazing_core | 烈焰之核 | spellstone | 免疫火焰；熔岩保护积累热量，离开熔岩后恢复<br>无其他熔岩保护时，连续浸泡约 15 秒耗尽<br>近战攻击者受到 4 点火伤并燃烧 4 秒<br>普通状态计时加速；熔火之心剩余时间以半速减少<br>受到水生敌怪的伤害翻倍 | 宝箱 20%（hell_shadow） |
| void_pearl | 虚空珍珠 | spellstone | 无需呼吸，免疫普通状态效果，生命回复减半<br>攻击附加凋零；黑暗中的附近敌怪每半秒受到 4 点原作基础伤害、凋零与缓慢<br>充分照明可阻止黑暗光环；间接魔法伤害转为治疗<br>35% 概率以 1 点生命承受致命伤害<br>饥饿相关效果暂留空 | 敌怪 100%（MoonLordCore，条件见掉率表） |
| ocean_stone | 海洋意志 | spellstone | 水下呼吸与夜视，游泳速度 +200%<br>对水生敌怪伤害抗性 40%；火伤易伤<br>V：消耗 150 魔力召雨；需位于主世界且尚未下雨<br>冷却 30 秒；建筑师的青睐下 15 秒 | 敌怪 100%（DukeFishron，条件见掉率表） |
| angel_blessing | 天使之祝 | spellstone | 免疫坠落伤害；50% 概率格开投射物<br>自身普通投射物速度 +75%；凋零和虚空伤害翻倍<br>空中再跳或 V 冲刺；共享 2 秒冷却<br>建筑师的青睐下冷却 0.75 秒 | 宝箱 33.3333%（sky） |
| monster_charm | 怪物猎人勋章 | accessory | 对亡灵伤害 +25%，对其他敌怪伤害 +10%<br>击杀敌怪有 10% 概率获得双倍战利品；经验奖励留空 | 1 条配方 |
| mining_charm | 猎宝者护符 | accessory | +30% 挖掘速度；+2 格挖掘距离；夜视<br>时运：10% 概率获得双倍矿物掉落<br>手持使用可切换夜视 | 1 条配方 |
| ender_ring | 末影之戒 | accessory | K 键远程打开虚空保险库 | 1 条配方 |
| mending_mixture | 修补混合物 | utility | 仅作为合成素材；没有耐久修复效果 | 1 条配方 |
| loot_generator | 战利品一体化盒 | utility | 原作调试物品，不提供生存获取方式 | 暂无生存来源，见待补清单/机制审计 |
| thicc_scroll | 空卷轴 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| iron_ring | 铁指环 | accessory | +1 护甲点 | 1 条配方 |
| etherium_ore | 以太矿石 | material | 材料，作为配方输入；具体来源见获取栏 | 敌怪 100%（pillars，条件见掉率表）；敌怪 50%（pillar_enemies，条件见掉率表） |
| etherium_ingot | 以太锭 | material | 材料，作为配方输入；具体来源见获取栏 | 4 条配方 |
| etherium_nugget | 以太粒 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| etherium_scraps | 以太合金碎片 | material | 材料，作为配方输入；具体来源见获取栏 | 暂无生存来源，见待补清单/机制审计 |
| ultimate_potion | Ultimate Potion | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| ultimate_potion_splash | Ultimate Potion Splash | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| ultimate_potion_lingering | Ultimate Potion Lingering | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| common_potion | Common Potion | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| common_potion_splash | Common Potion Splash | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| common_potion_lingering | Common Potion Lingering | utility | 原作空药瓶；没有药效，不能饮用 | 暂无生存来源，见待补清单/机制审计 |
| etherium_helmet | 以太头盔 | armor | 原生防御、原生前缀、转换自MC的穿戴图；四件套低血护盾 | 1 条配方 |
| etherium_chestplate | 以太胸甲 | armor | 原生防御、原生前缀、转换自MC的穿戴图；四件套低血护盾 | 1 条配方 |
| etherium_leggings | 以太护腿 | armor | 原生防御、原生前缀、转换自MC的穿戴图；四件套低血护盾 | 1 条配方 |
| etherium_boots | 以太靴子 | boots | 原生防御、原生前缀、转换自MC的穿戴图；四件套低血护盾 | 1 条配方 |
| etherium_pickaxe | 以太镐 | weapon | 原生镐挥舞与采掘，镐力比原版最高工具略高；+3 格工具距离 | 1 条配方 |
| etherium_axe | 以太斧 | weapon | 原生斧挥舞与伐木，斧力比原版最高工具略高；+3 格工具距离 | 1 条配方 |
| etherium_shovel | 以太锹 | weapon | 按原生快速镐接入挖掘；镐力比原版最高工具略高；+3 格工具距离 | 1 条配方 |
| etherium_sword | 以太阔剑 | weapon | 挥剑并发射穿透弹幕；以四柱阶段适配 | 1 条配方 |
| etherium_scythe | 以太镰刀 | weapon | 挥舞并发射死亡镰刀式弹幕；以四柱阶段适配 | 1 条配方 |
| astral_dust | 星尘 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方；敌怪 100%（pillars，条件见掉率表）；敌怪 50%（pillar_enemies，条件见掉率表） |
| lore_inscriber | 设计师的墨水 | utility | 仅保留物品、贴图及合成材料身份；没有刻字效果 | 1 条配方 |
| lore_fragment | 物品描述片段 | material | 物品描述材料占位；文字编辑与铁砧功能留空 | 1 条配方 |
| ender_rod | 末影棍 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| astral_breaker | 星光破坏锤 | weapon | 原生镐斧挥舞与采掘；镐力、斧力与已加载全部模组的最高工具持平；+3 格工具距离 | 1 条配方 |
| void_stone | 湮灭心石 | utility | 手持使用：将右侧相邻物品加入或移出销毁列表<br>右键切换；拾取列表中物品时将其消除 | 宝箱 5%（gold） |
| enchantment_transposer | 求知之书 | utility | 附魔转移暂留空 | 1 条配方 |
| curse_transposer | 噬咒之书 | utility | 诅咒附魔转移暂留空 | 1 条配方 |
| fabulous_scroll | 造物主的恩赐 | accessory | 获得翅膀飞行；持续飞行每秒消耗 8 魔力<br>同维度水晶球周围 50 格内免费飞行 | 1 条配方 |
| storage_crystal | 超维容器 | utility | 容器/灵魂系统显示资源；不可主动使用 | 暂无生存来源，见待补清单/机制审计 |
| soul_crystal | 灵魂水晶 | utility | 容器/灵魂系统显示资源；不可主动使用 | 暂无生存来源，见待补清单/机制审计 |
| the_acknowledgment | 启示之证 | utility | 使用打开神秘遗物手册 | 1 条配方；首次入场赠礼 |
| tattered_tome | 破烂书籍 | utility | 阅读与经验奖励暂留空 | 暂无生存来源，见待补清单/机制审计 |
| withered_tome | 凋零书籍 | utility | 阅读与经验奖励暂留空 | 暂无生存来源，见待补清单/机制审计 |
| corrupted_tome | 腐烂书籍 | utility | 阅读与经验奖励暂留空 | 暂无生存来源，见待补清单/机制审计 |
| cursed_ring | 七咒之戒 | accessory | 佩戴后七咒绑定于你：受到伤害 ×2，护甲效能 -30%，对敌怪伤害 -50%<br>火焰需用水熄灭；死亡失去 10% 最大生命，可到原地找回灵魂；无法睡眠<br>K 键打开虚空保险库；解锁受诅咒遗物<br>MC 中立怪物诅咒没有对应目标<br>抢夺与时运：双倍战利品/矿物概率各 +10 个百分点<br>魔力回复效率 +20%；经验奖励与附魔留空 | 首次入场赠礼 |
| twisted_mirror | 扭曲魔镜 | utility | 传送到床或世界出生点；冷却 10 秒 | 1 条配方 |
| cursed_scroll | 千咒卷轴 | accessory | 每件穿戴或手持装备的总体负面前缀计 1 层<br>每层：伤害 +8%，挖掘速度 +14%，生命回复 +8%<br>不计背包、时装及闲置配装 | 1 条配方 |
| berserk_charm | 血战沙场之证 | accessory | 每失去 1% 生命：伤害与攻速 +1%，移速与伤害抗性 +0.5% | 1 条配方 |
| earth_heart | 大地之心 | material | 材料，作为配方输入；具体来源见获取栏 | 宝箱 8%（overworld）；敌怪 0.05%（hostile，条件见掉率表） |
| twisted_heart | 扭曲之心 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| guardian_heart | 守卫者之心 | utility | 手持使用：激怒鼠标方向的非 Boss 敌怪，使其攻击附近敌怪<br>范围 24 格，持续 10 秒，冷却 10 秒 | 敌怪 100%（DukeFishron，条件见掉率表） |
| the_twist | 倒转之启 | weapon | 发射紫金符文弹幕，按肉山后战斗数值适配<br>对 Boss 伤害 +300%、击退 +300%；本武器攻击不受七咒第四诅咒的 50% 减伤<br>右键打开手册 | 1 条配方 |
| evil_essence | 邪恶精髓 | material | 材料，作为配方输入；具体来源见获取栏 | 敌怪 100%（WallofFlesh，条件见掉率表） |
| evil_ingot | 极恶锭 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| animal_guidebook | 兽友指南 | utility | 放在物品栏时，你和你的战斗召唤物不会伤害小动物<br>无生命值的召唤物不进行受伤与伤害转移 | 1 条配方 |
| hunter_guidebook | 野猎指南 | utility | 保留物品、贴图与配方<br>泰拉战斗召唤物无生命值，伤害转移按约定留空<br>原作没有独立的召唤增伤效果 | 1 条配方 |
| forbidden_fruit | 禁忌之果 | utility | 每角色可食用一次，永久记录状态以改变不洁圣杯的饮用效果<br>食用后：凋零 IV 与困惑 15 秒、虚弱 IV 20 秒、缓慢 III 25 秒<br>饥饿与永久回复代价仍留空 | 宝箱 20%（hell_shadow） |
| redemption_potion | 救赎药水 | utility | 依赖禁忌之果的饥饿状态，暂留空 | 暂无生存来源，见待补清单/机制审计 |
| golden_ring | 精美戒指 | accessory | +1 幸运<br>猪灵中立：当前世界没有对应生物 | 1 条配方 |
| cursed_stone | 邪恶之石 | utility | 持有它，在地狱熔岩中死亡，可解除七咒绑定 | 1 条配方 |
| enchanter_pearl | 附魔师的珍珠 | accessory | 七咒下装备：魔力回复效率 +50%，+1 护符槽<br>附魔功能留空 | 1 条配方 |
| avarice_scroll | 无尽贪婪契约 | accessory | 击杀敌怪有 15% 概率获得金币<br>全部 NPC 商店、护士与重铸价格降低 35%<br>每次购买随机额外获得 0～2 件商品；回购不触发<br>时运：双倍矿物掉落概率 +10 个百分点 | 1 条配方 |
| infinimeal | 狂野生长精华 | utility | 仅保留物品、贴图及合成材料身份；没有作物催熟效果 | 1 条配方 |
| darkest_scroll | 至暗卷轴 | material | 材料，作为配方输入；具体来源见获取栏 | 宝箱 20%（hell_shadow） |
| unwitnessed_amulet | 无主护身符 | material | 获得后转化为绑定玩家的神秘护身符，随机七种颜色及对应效果之一 | 首次入场赠礼 |
| twisted_potion | 扭曲恩惠药水 | utility | 传送到本世界上次死亡位置，只剩 1 点生命<br>可在主世界和下界之间返回 | 1 条配方 |
| infernal_shield | 烈焰之傲壁垒 | utility | 放在物品栏可熄灭普通火焰（熔岩中除外）<br>按住 B 格挡正面攻击；背后攻击伤害 +50%<br>成功格挡获得近战加成，受到伤害后失去 | 1 条配方 |
| cosmic_heart | 寰宇之心 | material | 材料，作为配方输入；具体来源见获取栏 | 1 条配方 |
| abyssal_heart | 深渊之心 | material | 材料，作为配方输入；具体来源见获取栏 | 敌怪 100%（MoonLordCore，条件见掉率表） |
| the_infinitum | 无止之言 | weapon | 发射追踪穿透星光弹幕，每发对每个目标命中一次<br>对 Boss 伤害 +200%、击退 +200%；10% 受限吸血<br>命中附加凋零 IV 8 秒、虚弱 IV / 缓慢 IV 15 秒、困惑 25 秒<br>手持时 85% 概率承受致命伤；本武器攻击不受七咒第四诅咒的 50% 减伤<br>右键打开手册 | 1 条配方 |
| astral_fruit | 天体果实 | utility | 首次食用永久解锁一个通用饰品栏；再次食用不增加栏位<br>每次食用：再生 IV、抗性 III 各 150 秒，力量 IV 200 秒，抗火 250 秒 | 角色首次参与击杀月球领主 |
| ender_slayer | 末影之屠 | weapon | 攻击、挖掘与攻击触发留空；保留贴图、基础数据和原生重铸 | 1 条配方 |
| the_cube | 非欧立方 | spellstone | 奔跑 +35%，游泳 +100%，采掘 +60%，攻速 +40%，幸运 +1<br>免疫普通负面状态、火、熔岩、坠落和窒息<br>格开 35% 攻击；实体造成的攻击超出 100 点原作伤害时无效（七咒下 150）<br>受伤前至少有原作 1.5 点生命时，致命伤后保留原作 1 点生命；否则尝试主动逃脱<br>V：随机安全传送；冷却 160 秒<br>逃脱回复到 30% 生命，获得 60 秒再生 III、抗性 II、抗火、力量 II<br>主世界与地狱之间传送；末地不存在<br>时运：双倍矿物掉落概率 +10 个百分点 | 1 条配方 |
| desolation_ring | 荒芜之负 | accessory | 佩戴后绑定<br>全世界自然刷怪间隔翻倍，数量上限减半<br>多名佩戴者不重复叠加 | 1 条配方 |
| astral_potato | 传送薯 | utility | 无限食物效果暂留空 | 1 条配方 |
| soul_compass | 被诅咒者的寻路指针 | utility | 显示本世界上次死亡坐标 | 1 条配方 |
| ascension_amulet | 飞升护符 | accessory | 同时提供七种神秘护符的效果 | 1 条配方 |
| eldritch_amulet | 轻蔑之约 | accessory | +3 点原作攻击伤害；15% 生命窃取<br>凝视敌怪施加缓慢与虚弱；死亡时保留掉落物 | 1 条配方 |
| enigmatic_elytra | 壮丽鞘翅 | accessory | 无限时长的翅膀飞行；免疫坠落伤害<br>飞行时按住跳跃键强化水平速度与加速 | 1 条配方 |
| enigmatic_eye | 休眠之眼 | accessory | 使用唤醒；觉醒后可装备，+1 护符槽、+3 格触及<br>解锁原作语音与中英字幕；可在配置中开关<br>不存在的 MC 生物与末地事件不触发语音 | 角色首次开箱 |
| ichor_bottle | 灵液瓶 | material | 永久解锁一个神秘遗物护符槽；每个角色一次<br>再生 III 60 秒、吸收 V 40 秒、抗性 III 80 秒、抗火 150 秒 | 宝箱 20%（hell_shadow） |
| quote_player | 呓语播放器 | utility | 原作调试/叙事物品 | 暂无生存来源，见待补清单/机制审计 |
| insignia | 迷人徽章 | accessory | +1 护符槽<br>铁砧改名与刻字功能留空 | 1 条配方 |
| cosmic_scroll | 建筑师的青睐 | accessory | +1 幸运，+1 奥秘卷轴槽<br>以太套护盾阈值提高到 80%；术石主动冷却减半<br>每 600 秒可在致命伤害后回复到 50% 生命 | 1 条配方；按用户口径使用普通天宇之心，无额外祝福条件 |
| deception_amulet | 诡计护符 [未完成] | accessory | 原作 WIP 物品，无可移植的完整效果 | 暂无生存来源，见待补清单/机制审计 |
| the_judgement | 审判 | weapon | 攻击、挖掘与攻击触发留空；保留贴图、基础数据和原生重铸 | 暂无生存来源，见待补清单/机制审计 |
| soul_dust | 灵魂尘 | material | 材料，作为配方输入；具体来源见获取栏 | 敌怪 5%（undead，条件见掉率表） |
| eldritch_pan | 饕餮之锅 | weapon | 圣骑士之锤式回旋投掷，同时最多 2 口锅；需七咒时间资格<br>15% 受限吸血；每击杀一种新敌怪，基础伤害与护甲各 +0.5 原作点（最多 100 种）<br>本武器攻击不受七咒第四诅咒的 50% 减伤；饥饿窃取留空 | 1 条配方 |
| enigmatic_amulet_aqua | 神秘护身符·青 | accessory | 奔跑速度 +15%；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| enigmatic_amulet_violet | 神秘护身符·紫 | accessory | 投射物格开概率 +15%；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| enigmatic_amulet_magenta | 神秘护身符·品红 | accessory | 重力 -25%；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| enigmatic_amulet_green | 神秘护身符·绿 | accessory | 采掘速度 +25%；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| enigmatic_amulet_black | 神秘护身符·黑 | accessory | 10% 生命窃取；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| enigmatic_amulet_blue | 神秘护身符·蓝 | accessory | 游泳速度 +25%；死亡掉落存入超维容器 | 无主护符随机转化，七色之一 |
| ultimate_night_vision | 终极夜视药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_night_vision_splash | 喷溅型终极夜视药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_night_vision_lingering | 滞留型终极夜视药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_invisibility | 终极隐身药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_invisibility_splash | 喷溅型终极隐身药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_invisibility_lingering | 滞留型终极隐身药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_leaping | 终极跳跃提升药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_leaping_splash | 喷溅型终极跳跃提升药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_leaping_lingering | 滞留型终极跳跃提升药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_fire_resistance | 终极防火药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_fire_resistance_splash | 喷溅型终极防火药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_fire_resistance_lingering | 滞留型终极防火药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_swiftness | 终极迅捷药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_swiftness_splash | 喷溅型终极迅捷药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_swiftness_lingering | 滞留型终极迅捷药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_slowness | 终极缓慢药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_slowness_splash | 喷溅型终极缓慢药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_slowness_lingering | 滞留型终极缓慢药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_turtle_master | 终极神龟药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_turtle_master_splash | 喷溅型终极神龟药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_turtle_master_lingering | 滞留型终极神龟药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_water_breathing | 终极水肺药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_water_breathing_splash | 喷溅型终极水肺药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_water_breathing_lingering | 滞留型终极水肺药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_healing | 终极治疗药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_healing_splash | 喷溅型终极治疗药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_healing_lingering | 滞留型终极治疗药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_harming | 终极伤害药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_harming_splash | 喷溅型终极伤害药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_harming_lingering | 滞留型终极伤害药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_poison | 终极剧毒药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_poison_splash | 喷溅型终极剧毒药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_poison_lingering | 滞留型终极剧毒药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_regeneration | 终极再生药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_regeneration_splash | 喷溅型终极再生药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_regeneration_lingering | 滞留型终极再生药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_strength | 终极力量药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_strength_splash | 喷溅型终极力量药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_strength_lingering | 滞留型终极力量药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_weakness | 终极虚弱药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_weakness_splash | 喷溅型终极虚弱药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_weakness_lingering | 滞留型终极虚弱药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_slow_falling | 终极缓降药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_slow_falling_splash | 喷溅型终极缓降药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_slow_falling_lingering | 滞留型终极缓降药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_haste | 终极急迫药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 2 条配方 |
| ultimate_haste_splash | 喷溅型终极急迫药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_haste_lingering | 滞留型终极急迫药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| ultimate_molten_heart | 熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| ultimate_molten_heart_splash | 喷溅型熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| ultimate_molten_heart_lingering | 滞留型熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| common_haste | 急迫药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| common_haste_splash | 喷溅型急迫药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| common_haste_lingering | 滞留型急迫药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| common_long_haste | 急迫药水（延长） | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| common_long_haste_splash | 喷溅型急迫药水（延长） | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| common_long_haste_lingering | 滞留型急迫药水（延长） | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| common_strong_haste | 急迫药水（强化） | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| common_strong_haste_splash | 喷溅型急迫药水（强化） | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| common_strong_haste_lingering | 滞留型急迫药水（强化） | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| common_molten_heart | 熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| common_molten_heart_splash | 喷溅型熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| common_molten_heart_lingering | 滞留型熔火之心药水 | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| common_long_molten_heart | 熔火之心药水（延长） | utility | 已接入玩家及NPC药效与原作时长；饮用；酿造适配见材料表 | 1 条配方 |
| common_long_molten_heart_splash | 喷溅型熔火之心药水（延长） | utility | 已接入玩家及NPC药效与原作时长；喷溅范围与距离衰减；酿造适配见材料表 | 1 条配方 |
| common_long_molten_heart_lingering | 滞留型熔火之心药水（延长） | utility | 已接入玩家及NPC药效与原作时长；滞留云等待、缩小与逐目标冷却；酿造适配见材料表 | 1 条配方 |
| nether_star | 下界之星 | material | 佩戴七咒之戒击杀血肉之墙时固定获得一个<br>用于原下界之星参与的合成 | 敌怪 100%（WallofFlesh，条件见掉率表） |
| big_lamp | 大灯笼 | material | 原作灯笼贴图、放置、发光、住房光源与原物掉回 | 1 条配方 |
| massive_lamp | 封装的大灯笼 | material | 原作灯笼贴图、放置、发光、住房光源与原物掉回 | 1 条配方 |
| big_shroomlamp | 菌光体灯笼 | material | 原作灯笼贴图、放置、发光、住房光源与原物掉回 | 1 条配方 |
| massive_shroomlamp | 封装的菌光体灯笼 | material | 原作灯笼贴图、放置、发光、住房光源与原物掉回 | 1 条配方 |
| end_anchor | 维度锚 | material | 维度锚：紫晶充能、角色绑定、安全重生，沿用原作35%不耗充能概率 | 1 条配方 |
