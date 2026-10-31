// Unofficial Terraria adaptation of Enigmatic Legacy by Aizistral and contributors.
// GPT-assisted C#/tModLoader rewrite; public-source snapshot modified 2026-10-31.
// See LICENSE.md, LICENSE-UPSTREAM.md and THIRD_PARTY_NOTICES.md.
using System.Collections.Generic;

namespace EnigmaticLegacy.Core;

public static class LegacyDescriptions
{
    private static readonly Dictionary<string, (string Zh, string En)> Texts = new()
    {
        ["iron_ring"] = ("+1 护甲点", "+1 armor point"),
        ["golden_ring"] = ("+1 幸运\n猪灵中立：当前世界没有对应生物", "+1 luck\nPiglin neutrality has no target in this world"),
        ["magnet_ring"] = ("吸引 8 格范围内的物品\nM 键切换；按住下键暂停吸引", "Attract items within 8 tiles\nM: toggle; hold Down to suspend"),
        ["super_magnet_ring"] = ("将 16 格范围内的物品传送到你身边\nM 键切换；按住下键暂停", "Teleport items within 16 tiles to you\nM: toggle; hold Down to suspend"),
        ["ender_ring"] = ("K 键远程打开虚空保险库", "K: remotely open your Void Vault"),
        ["mining_charm"] = ("+30% 挖掘速度；+2 格挖掘距离；夜视\n时运：10% 概率获得双倍矿物掉落\n手持使用可切换夜视", "+30% mining speed; +2 tile reach; night vision\nFortune: 10% chance of double mineral drops\nUse in hand to toggle night vision"),
        ["monster_charm"] = ("对亡灵伤害 +25%，对其他敌怪伤害 +10%\n击杀敌怪有 10% 概率获得双倍战利品；经验奖励留空", "+25% damage to undead; +10% to other hostiles\n10% chance of double hostile loot; XP rewards are deferred"),
        ["golem_heart"] = ("+4 护甲点；未穿盔甲时改为 +16\n近战伤害抗性 25%，无甲时爆炸抗性 40%\n免疫击退与灼热地面；受到魔法伤害翻倍\nV：无主动技能", "+4 armor; +16 without armor\n25% contact resistance; 40% explosion resistance without armor\nKnockback and hot-tile immunity; double magic damage taken\nNo active ability"),
        ["eye_of_nebula"] = ("+40% 魔法伤害；65% 魔法伤害抗性\n15% 概率闪避并传送；在水中受到双倍伤害\nV：闪到目标身后，下一击伤害 +150%；冷却 3 秒", "+40% magic damage; 65% magic resistance\n15% teleport dodge; double damage taken in water\nV: phase behind target; next hit +150%; 3 s cooldown"),
        ["blazing_core"] = ("免疫火焰；熔岩保护积累热量，离开熔岩后恢复\n无其他熔岩保护时，连续浸泡约 15 秒耗尽\n近战攻击者受到 4 点火伤并燃烧 4 秒\n普通状态计时加速；熔火之心剩余时间以半速减少\n受到水生敌怪的伤害翻倍", "Fire immunity; lava protection builds heat and recovers away from lava\nAbout 15 s of continuous exposure without other protection exhausts it\nContact attackers take 4 fire damage and burn for 4 s\nOrdinary status timers run faster; remaining Molten Heart time runs at half speed\nDouble damage taken from aquatic creatures"),
        ["void_pearl"] = ("无需呼吸，免疫普通状态效果，生命回复减半\n攻击附加凋零；黑暗中的附近敌怪每半秒受到 4 点原作基础伤害、凋零与缓慢\n充分照明可阻止黑暗光环；间接魔法伤害转为治疗\n35% 概率以 1 点生命承受致命伤害\n饥饿相关效果暂留空", "No breathing; ordinary status immunity; half regeneration\nAttacks wither; dark nearby enemies take 4 source base damage every half second, wither and slow\nSufficient light stops the darkness aura; indirect magic damage heals instead\n35% chance to survive lethal damage at 1 life\nHunger effect is deferred"),
        ["ocean_stone"] = ("水下呼吸与夜视，游泳速度 +200%\n对水生敌怪伤害抗性 40%；火伤易伤\nV：消耗 150 魔力召雨；需位于主世界且尚未下雨\n冷却 30 秒；建筑师的青睐下 15 秒", "Water breathing and vision; +200% swim speed\n40% resistance to aquatic creatures; vulnerable to fire\nV: spend 150 mana to summon rain in the Overworld while dry\n30 s cooldown; 15 s with Architect’s Favor"),
        ["angel_blessing"] = ("免疫坠落伤害；50% 概率格开投射物\n自身普通投射物速度 +75%；凋零和虚空伤害翻倍\n空中再跳或 V 冲刺；共享 2 秒冷却\n建筑师的青睐下冷却 0.75 秒", "Fall immunity; 50% projectile deflection\nYour ordinary projectiles travel 75% faster; double wither and void damage taken\nExtra aerial jump or V dash; shared 2 s cooldown\n0.75 s with Architect’s Favor"),
        ["enigmatic_item"] = ("创造之心：自由飞行、环境伤害防护、负面状态免疫\n不会因致命伤害死亡\nV：发射追踪凋零之颅", "Heart of Creation: flight, environmental protection, debuff immunity\nSurvive lethal damage\nV: launch a homing wither skull"),
        ["the_cube"] = ("奔跑 +35%，游泳 +100%，采掘 +60%，攻速 +40%，幸运 +1\n免疫普通负面状态、火、熔岩、坠落和窒息\n格开 35% 攻击；实体造成的攻击超出 100 点原作伤害时无效（七咒下 150）\n受伤前至少有原作 1.5 点生命时，致命伤后保留原作 1 点生命；否则尝试主动逃脱\nV：随机安全传送；冷却 160 秒\n逃脱回复到 30% 生命，获得 60 秒再生 III、抗性 II、抗火、力量 II\n主世界与地狱之间传送；末地不存在\n时运：双倍矿物掉落概率 +10 个百分点", "+35% run, +100% swim, +60% mining, +40% attack speed, +1 luck\nOrdinary debuff, fire, lava, fall and drowning immunity\n35% deflection; negate creature attacks above 100 source damage (150 when cursed)\nAt least 1.5 source health before a hit: survive with 1 source health; otherwise attempt escape\nV: random safe teleport; 160 s cooldown\nEscape at 30% life with Regeneration III, Resistance II, Fire Resistance and Strength II for 60 s\nTravel between Overworld and Nether; the End is absent\nFortune: +10 percentage points to double mineral drops"),
        ["cursed_ring"] = ("佩戴后七咒绑定于你：受到伤害 ×2，护甲效能 -30%，对敌怪伤害 -50%\n火焰需用水熄灭；死亡失去 10% 最大生命，可到原地找回灵魂；无法睡眠\nK 键打开虚空保险库；解锁受诅咒遗物\nMC 中立怪物诅咒没有对应目标\n抢夺与时运：双倍战利品/矿物概率各 +10 个百分点\n魔力回复效率 +20%；经验奖励与附魔留空", "Seven Curses bind on equip: damage taken ×2; armor effectiveness -30%; damage to monsters -50%\nFire persists until extinguished with water; death tears away 10% max life until your soul is reclaimed; no sleep\nK: Void Vault; unlock cursed relics\nThe neutral-creature curse has no Minecraft targets here\nLooting and Fortune: +10 percentage points to double loot/mineral drops\n+20% mana regeneration efficiency; XP rewards and enchanting are deferred"),
        ["berserk_charm"] = ("每失去 1% 生命：伤害与攻速 +1%，移速与伤害抗性 +0.5%", "Per 1% missing health: +1% damage and attack speed; +0.5% movement and damage resistance"),
        ["cursed_scroll"] = ("每件穿戴或手持装备的总体负面前缀计 1 层\n每层：伤害 +8%，挖掘速度 +14%，生命回复 +8%\n不计背包、时装及闲置配装", "Each overall negative prefix on worn or held equipment counts as one curse\nPer curse: +8% damage, +14% mining speed, +8% regeneration\nBackpack, vanity and inactive loadouts do not count"),
        ["xp_scroll"] = ("经验储存与提取：暂留空", "Experience storage and extraction: deferred"),
        ["heaven_scroll"] = ("同维度水晶球周围 50 格内获得翅膀飞行\n持续飞行每秒消耗 2 魔力", "Grants wing flight within 50 tiles of a Crystal Ball in the same realm\nSustained flight costs 2 mana per second"),
        ["fabulous_scroll"] = ("获得翅膀飞行；持续飞行每秒消耗 8 魔力\n同维度水晶球周围 50 格内免费飞行", "Grants wing flight; sustained flight costs 8 mana per second\nFlight is free within 50 tiles of a Crystal Ball in the same realm"),
        ["avarice_scroll"] = ("击杀敌怪有 15% 概率获得金币\n全部 NPC 商店、护士与重铸价格降低 35%\n每次购买随机额外获得 0～2 件商品；回购不触发\n时运：双倍矿物掉落概率 +10 个百分点", "15% chance to gain a Gold Coin on hostile kills\n35% discount at all NPC shops, the Nurse and reforging\nPurchases randomly grant 0–2 bonus items; excludes buyback\nFortune: +10 percentage points to double mineral drops"),
        ["enigmatic_amulet"] = ("+1.5 点原作攻击伤害\n死亡掉落存入原地的超维容器（沿用角色死亡模式）", "+1.5 source attack damage\nActual death drops enter an extradimensional vessel at your death location"),
        ["enigmatic_amulet_aqua"] = ("奔跑速度 +15%；死亡掉落存入超维容器", "+15% running speed; death drops enter a vessel"),
        ["enigmatic_amulet_violet"] = ("投射物格开概率 +15%；死亡掉落存入超维容器", "+15% projectile deflection; death drops enter a vessel"),
        ["enigmatic_amulet_magenta"] = ("重力 -25%；死亡掉落存入超维容器", "-25% gravity; death drops enter a vessel"),
        ["enigmatic_amulet_green"] = ("采掘速度 +25%；死亡掉落存入超维容器", "+25% mining speed; death drops enter a vessel"),
        ["enigmatic_amulet_black"] = ("10% 生命窃取；死亡掉落存入超维容器", "10% lifesteal; death drops enter a vessel"),
        ["enigmatic_amulet_blue"] = ("游泳速度 +25%；死亡掉落存入超维容器", "+25% swimming speed; death drops enter a vessel"),
        ["ascension_amulet"] = ("同时提供七种神秘护符的效果", "All seven Enigmatic Amulet color powers"),
        ["eldritch_amulet"] = ("+3 点原作攻击伤害；15% 生命窃取\n凝视敌怪施加缓慢与虚弱；死亡时保留掉落物", "+3 source attack damage; 15% lifesteal\nGaze slows and weakens enemies; retain death drops"),
        ["cosmic_scroll"] = ("+1 幸运，+1 奥秘卷轴槽\n以太套护盾阈值提高到 80%；术石主动冷却减半\n每 600 秒可在致命伤害后回复到 50% 生命", "+1 luck and arcane scroll slot\nEtherium shield threshold becomes 80%; halve spellstone cooldowns\nSurvive lethal damage at 50% life every 600 s"),
        ["enigmatic_elytra"] = ("无限时长的翅膀飞行；免疫坠落伤害\n飞行时按住跳跃键强化水平速度与加速", "Unlimited wing flight; fall immunity\nHold Jump in flight for enhanced horizontal speed and acceleration"),
        ["infernal_shield"] = ("放在物品栏可熄灭普通火焰（熔岩中除外）\n按住 B 格挡正面攻击；背后攻击伤害 +50%\n成功格挡获得近战加成，受到伤害后失去", "Extinguish ordinary fire while carried, except in lava\nHold B to block frontal attacks; +50% damage taken from behind\nSuccessful blocks empower melee until you take damage"),
        ["insignia"] = ("+1 护符槽\n铁砧改名与刻字功能留空", "+1 charm slot\nAnvil naming and inscription are omitted"),
        ["astral_fruit"] = ("首次食用永久解锁一个通用饰品栏；再次食用不增加栏位\n每次食用：再生 IV、抗性 III 各 150 秒，力量 IV 200 秒，抗火 250 秒", "First consumption permanently unlocks one universal accessory slot; subsequent use grants no extra slots\nEach use: Regeneration IV and Resistance III 150 s, Strength IV 200 s, Fire Resistance 250 s"),
        ["ichor_bottle"] = ("永久解锁一个神秘遗物护符槽；每个角色一次\n再生 III 60 秒、吸收 V 40 秒、抗性 III 80 秒、抗火 150 秒", "Permanently unlock one Enigmatic charm slot; once per character\nRegeneration III 60 s, Absorption V 40 s, Resistance III 80 s, Fire Resistance 150 s"),
        ["forbidden_fruit"] = ("每角色可食用一次，永久记录状态以改变不洁圣杯的饮用效果\n食用后：凋零 IV 与困惑 15 秒、虚弱 IV 20 秒、缓慢 III 25 秒\n饥饿与永久回复代价仍留空", "Consumable once per character; permanently records the state used by the Unholy Grail\nOn consumption: Wither IV and Confused 15 s, Weakness IV 20 s, Slowness III 25 s\nHunger and its permanent regeneration cost remain deferred"),
        ["redemption_potion"] = ("依赖禁忌之果的饥饿状态，暂留空", "Awaits the Forbidden Fruit hunger mechanic"),
        ["astral_potato"] = ("无限食物效果暂留空", "Infinite food effect is deferred"),
        ["enchanter_pearl"] = ("七咒下装备：魔力回复效率 +50%，+1 护符槽\n附魔功能留空", "Equip while cursed: +50% mana regeneration efficiency and +1 charm slot\nEnchanting is deferred"),
        ["enchantment_transposer"] = ("附魔转移暂留空", "Enchantment transfer is deferred"),
        ["curse_transposer"] = ("诅咒附魔转移暂留空", "Curse transfer is deferred"),
        ["mending_mixture"] = ("仅作为合成素材；没有耐久修复效果", "Crafting material only; no durability repair"),
        ["guardian_heart"] = ("手持使用：激怒鼠标方向的非 Boss 敌怪，使其攻击附近敌怪\n范围 24 格，持续 10 秒，冷却 10 秒", "Use: enrage a non-boss in the cursor direction to attack nearby enemies\n24 tile range; 10 s duration and cooldown"),
        ["animal_guidebook"] = ("放在物品栏时，你和你的战斗召唤物不会伤害小动物\n无生命值的召唤物不进行受伤与伤害转移", "While carried, you and your combat summons cannot harm critters\nSummons without health receive no injury or damage redirection mechanic"),
        ["hunter_guidebook"] = ("保留物品、贴图与配方\n泰拉战斗召唤物无生命值，伤害转移按约定留空\n原作没有独立的召唤增伤效果", "Retains the item, texture and recipe\nDamage redirection is omitted for Terraria combat summons without health\nThe original grants no separate summon damage bonus"),
        ["recall_potion"] = ("传送到床或世界出生点", "Return to your bed or world spawn"),
        ["twisted_mirror"] = ("传送到床或世界出生点；冷却 10 秒", "Return to your bed or world spawn; 10 s cooldown"),
        ["twisted_potion"] = ("传送到本世界上次死亡位置，只剩 1 点生命\n可在主世界和下界之间返回", "Return to your last death in this world at 1 life\nCan return across Overworld and Nether"),
        ["extradimensional_eye"] = ("右键绑定当前位置；左键点击 6 格内生物，将其送到绑定地点并消耗此物\n目标必须与绑定地点同维度；地狱高度区视作下界\n保护 Boss 与多节敌怪的 AI", "Right-click to bind; left-click a creature within 6 tiles to send it there and consume the eye\nTarget and binding must share a realm; the Underworld counts as Nether\nBosses and multipart AI are protected"),
        ["mega_sponge"] = ("彩蛋物品：沿用原贴图\n吸水效果与获取渠道留空", "Easter egg retaining the original texture\nWater absorption and acquisition are omitted"),
        ["void_stone"] = ("手持使用：将右侧相邻物品加入或移出销毁列表\n右键切换；拾取列表中物品时将其消除", "Use: add/remove the next inventory item from the void list\nRight-click to toggle; matching pickups are destroyed"),
        ["soul_compass"] = ("显示本世界上次死亡坐标", "Show your last death coordinates"),
        ["cursed_stone"] = ("持有它，在地狱熔岩中死亡，可解除七咒绑定", "Carry this and die in Underworld lava to sever the Seven Curses"),
        ["the_acknowledgment"] = ("使用打开神秘遗物手册", "Use to open the Enigmatic Legacy journal"),
        ["the_twist"] = ("发射紫金符文弹幕，按肉山后战斗数值适配\n对 Boss 伤害 +300%、击退 +300%；本武器攻击不受七咒第四诅咒的 50% 减伤\n右键打开手册", "Fires violet-gold rune bolts, balanced for early Hardmode\n+300% boss damage and knockback; this weapon bypasses the fourth curse\nRight-click: journal"),
        ["the_infinitum"] = ("发射追踪穿透星光弹幕，每发对每个目标命中一次\n对 Boss 伤害 +200%、击退 +200%；10% 受限吸血\n命中附加凋零 IV 8 秒、虚弱 IV / 缓慢 IV 15 秒、困惑 25 秒\n手持时 85% 概率承受致命伤；本武器攻击不受七咒第四诅咒的 50% 减伤\n右键打开手册", "Fires homing piercing starlight bolts, hitting each target once\n+200% boss damage and knockback; 10% capped lifesteal\nWither IV 8 s, Weakness IV / Slowness IV 15 s, Confused 25 s\n85% lethal protection while held; this weapon bypasses the fourth curse\nRight-click: journal"),
        ["ender_slayer"] = ("武器攻击与命中触发待定；末地与末地生物视作不存在", "Weapon attacks and hit effects await design; the End and its creatures are absent"),
        ["eldritch_pan"] = ("圣骑士之锤式回旋投掷，同时最多 2 口锅；需七咒时间资格\n15% 受限吸血；每击杀一种新敌怪，基础伤害与护甲各 +0.5 原作点（最多 100 种）\n本武器攻击不受七咒第四诅咒的 50% 减伤；饥饿窃取留空", "Paladin-style returning throw; at most 2 pans; requires worthiness\n15% capped lifesteal; each unique hostile slain grants +0.5 source damage and armor (up to 100 types)\nThis weapon bypasses the fourth curse; hunger steal is deferred"),
        ["etherium_pickaxe"] = ("原生镐挥舞与采掘，镐力比原版最高工具略高；+3 格工具距离", "Native pickaxe swing and mining; slightly above the strongest vanilla pick; +3 tile tool reach"),
        ["etherium_axe"] = ("原生斧挥舞与伐木，斧力比原版最高工具略高；+3 格工具距离", "Native axe swing and chopping; slightly above the strongest vanilla axe; +3 tile tool reach"),
        ["etherium_shovel"] = ("按原生快速镐接入挖掘；镐力比原版最高工具略高；+3 格工具距离", "Adapted as a fast native digging pick; slightly above the strongest vanilla pick; +3 tile tool reach"),
        ["etherium_sword"] = ("挥剑并发射穿透弹幕；以四柱阶段适配", "Sword swing with piercing projectiles; balanced for the lunar pillars"),
        ["etherium_scythe"] = ("挥舞并发射死亡镰刀式弹幕；以四柱阶段适配", "Swing and fire Death Sickle-style projectiles; balanced for the lunar pillars"),
        ["astral_breaker"] = ("原生镐斧挥舞与采掘；镐力、斧力与已加载全部模组的最高工具持平；+3 格工具距离", "Native pickaxe/axe swing and mining; matches the highest loaded pick/axe power; +3 tile tool reach"),
        ["desolation_ring"] = ("佩戴后绑定\n全世界自然刷怪间隔翻倍，数量上限减半\n多名佩戴者不重复叠加", "Binds on equip\nDoubles the global natural spawn interval and halves its cap\nMultiple bearers do not stack"),
        ["unwitnessed_amulet"] = ("获得后转化为绑定玩家的神秘护身符，随机七种颜色及对应效果之一", "Becomes your personal Enigmatic Amulet with one of seven random colors and effects"),
        ["nether_star"] = ("佩戴七咒之戒击杀血肉之墙时固定获得一个\n用于原下界之星参与的合成", "One drops from the Wall of Flesh when the killer wears the Seven Curses\nReplaces the Nether Star in its original recipes"),
        ["enigmatic_eye"] = ("使用唤醒；觉醒后可装备，+1 护符槽、+3 格触及\n解锁原作语音与中英字幕；可在配置中开关\n不存在的 MC 生物与末地事件不触发语音", "Use to awaken; equippable when awake, granting +1 charm slot and +3 tile reach\nUnlocks original voices and localized subtitles; configurable\nAbsent Minecraft creatures and End events do not trigger quotes"),
        ["unholy_grail"] = ("可无限次饮用，不消耗圣杯\n已食用禁忌之果且承受七咒时：再生 III 25 秒、吸收 II 40 秒、力量 II 50 秒、速度 I 与抗性 II 60 秒\n否则：凋零 III 5 秒、中毒 II 8 秒、虚弱 II 10 秒、困惑与缓慢 I 12 秒\n未食用禁忌之果时额外给予虚弱（泰拉）8 秒，替代饥饿 III", "Reusable; drinking never consumes the grail\nAfter Forbidden Fruit while cursed: Regeneration III 25 s, Absorption II 40 s, Strength II 50 s, Speed I and Resistance II 60 s\nOtherwise: Wither III 5 s, Poison II 8 s, Weakness II 10 s, Confused and Slowness I 12 s\nWithout Forbidden Fruit, also inflicts vanilla Weak 8 s as the Hunger III substitute"),
        ["infinimeal"] = ("仅保留物品、贴图及合成材料身份；没有作物催熟效果", "Retains its item, texture and crafting role; no crop-growth effect"),
        ["lore_inscriber"] = ("仅保留物品、贴图及合成材料身份；没有刻字效果", "Retains its item, texture and crafting role; no inscription effect"),
        ["tattered_tome"] = ("阅读与经验奖励暂留空", "Reading and experience rewards are deferred"),
        ["withered_tome"] = ("阅读与经验奖励暂留空", "Reading and experience rewards are deferred"),
        ["corrupted_tome"] = ("阅读与经验奖励暂留空", "Reading and experience rewards are deferred"),
        ["deception_amulet"] = ("原作 WIP 物品，无可移植的完整效果", "WIP in the original; no complete effect"),
        ["loot_generator"] = ("原作调试物品，不提供生存获取方式", "Original debug item; no survival acquisition"),
        ["quote_player"] = ("原作调试/叙事物品", "Original debug/narrative item"),
        ["relic_of_testing"] = ("原作隐藏调试物品", "Original hidden debug item"),
        ["lore_fragment"] = ("物品描述材料占位；文字编辑与铁砧功能留空", "Lore material placeholder; text editing and anvil effects are omitted"),
    };
    public static string For(ImportedDefinition definition)
    {
        if (definition.Kind == "weapon" && !LegacyWeapons.Implemented(definition.Id))
            return BridgeUnits.Text("武器攻击方式与攻击触发效果留空，等待后续指定", "Weapon attacks and on-hit effects are deferred pending your design");
        if (Texts.TryGetValue(definition.Id, out var text)) return BridgeUnits.Text(text.Zh, text.En);
        if (definition.Kind is "armor" or "boots") return BridgeUnits.Text("穿戴时不可见\n完整四件以太套：生命不高于 40% 时伤害抗性 50%，格开普通投射物并击退近身攻击者", "Invisible while worn\nFull four-piece Etherium set: 50% resistance, ordinary projectile deflection and contact knockback at or below 40% life");
        if (definition.Variant != "") return BridgeUnits.Text("保留原作药水时长；投掷型影响附近目标，滞留型形成短暂药雾", "Original potion durations; splash affects nearby targets; lingering creates a brief cloud");
        if (definition.Id.Contains("potion")) return BridgeUnits.Text("空药瓶，无药效", "Hollow potion; no effect");
        return BridgeUnits.Text("神秘遗物合成材料", "Enigmatic Legacy crafting material");
    }
}
