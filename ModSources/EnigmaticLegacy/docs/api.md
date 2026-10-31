# 后续移植与扩展接口

在自己的 Mod.PostSetupContent 中，通过 `ModLoader.TryGetMod("EnigmaticLegacy", out var legacy)` 获取本模组；不存在时跳过即可，无需硬引用灾厄或其他第三方 DLL。注册应在每次加载时执行，卸载会清理。

| 调用 | 返回 / 用途 |
|---|---|
| `legacy.Call("GetApiVersion")` | 当前为 int 3 |
| `legacy.Call("GetItemType", "cursed_scroll")` | 当前物品 type；未知 ID 返回 0 |
| `legacy.Call("RegisterNpcTag", npcType, "aquatic")` | 给 NPC 类型登记水生语义 |
| `legacy.Call("RegisterNpcTag", npcType, "undead")` | 给 NPC 类型登记亡灵语义 |
| `legacy.Call("RegisterNpcTag", npcType, "boss")` | 标记未设置 npc.boss 的 Boss 部件 |
| `legacy.Call("RegisterNpcTag", npcType, "darkness_hunter")` | 明确指定珍珠黑暗光环的幻翼式光照例外；普通蝙蝠不自动获得此标签 |
| `legacy.Call("RegisterProjectileTag", projectileType, "magic")` | 魔法伤害语义 |
| `legacy.Call("RegisterProjectileTag", projectileType, "fire")` | 火焰伤害语义 |
| `legacy.Call("RegisterProjectileTag", projectileType, "explosion")` | 爆炸伤害语义 |
| `legacy.Call("RegisterProjectileTag", projectileType, "environment")` | 将弹幕视为环境来源，不触发立方体超额实体免伤或星云实体闪避 |
| `legacy.Call("RegisterProjectileTag", projectileType, "reflectable")` | 明确允许反射，仅用于可独立改变 owner/速度的弹幕 |
| `legacy.Call("RegisterProjectileTag", projectileType, "unreflectable")` | 禁止反射，优先于允许标签 |
| `legacy.Call("ProtectBuff", buffType)` | 防止遗物状态免疫/缩时干预技术状态 |
| `legacy.Call("RegisterCursePrefix", prefixType, true)` | 将前缀登记为总体负面，每件生效装备计一层 |
| `legacy.Call("RegisterCursePrefix", prefixType, false)` | 取消总体负面分类 |
| `legacy.Call("IsPlayerSummon", projectile)` | bool；战斗召唤物/哨兵及对应射弹，排除装饰标签 |
| `legacy.Call("RegisterFortuneTile", tileType)` | 登记未加入原生 Ore 集合的模组矿物 |
| `legacy.Call("RegisterFlightAnchor", tileType)` | 登记可作飞行信标的 Tile 类型 |
| `legacy.Call("GetDamageClass", "darkness")` | 返回可供原生攻击使用的 DamageClass |
| `legacy.Call("ApplyNpcDamage", owner, npc, "darkness", damage)` | 服务端/单人应用原生 NPC 伤害；联机客户端返回 false |
| `legacy.Call("ApplyPlayerDamage", player, reason, "indirect_magic", damage)` | 仅归属客户端应用原生 Hurt，返回实际伤害 |
| `legacy.Call("GetRealm", worldCenter)` | Vector2 世界像素坐标；返回 `overworld` 或 `nether` |
| `legacy.Call("RegisterDynamicLight", provider)` | 注册动态光提供器，返回 IDisposable；provider 为 `Func<IEnumerable<(Vector2 Position, Vector3 Color)>>` |
| `legacy.Call("RegisterUniversalAccessorySlot", "YourMod/YourSlot", true)` | 显式将一个原生 ModAccessorySlot 登记为通用饰品类别；false 表示专用类别 |
| `legacy.Call("GetNativeAccessoryCapacity", player)` | 原生当前已解锁且可用的通用饰品栏数量 |
| `legacy.Call("RegisterDamageTag", tag, classifier)` | 注册 `Func<DamageHitContext, bool>` 元素／来源分类器 |
| `legacy.Call("UnregisterDamageTag", tag)` | 注销自己的分类器 |
| `legacy.Call("RegisterDamageRule", source, rule, priority)` | 注册 `DamageHitRule`；优先级从小到大，相同优先级按注册顺序；priority 可省略，默认 0 |
| `legacy.Call("UnregisterDamageRule", source)` | 注销自己的命中公式 |
| `legacy.Call("AddUniversalDamage", player, source, operation, amount)` | 添加本帧独立通用属性；使用 `DamageOperation`，amount 为 float |

例如 `legacy.Call("RegisterCursePrefix", ModContent.PrefixType<YourNegativePrefix>(), true);`。系统不猜测第三方复杂前缀的好坏；应由该前缀的作者按整体属性注册。注册 ID 必须在当前加载内容范围内。参数错误与未知命令抛出 ArgumentException。

动态光扫描合并地形、`Lighting.AddLight`、原生弹幕 `light` 与提供器。只在客户端私有AI里发光的效果若影响服务器暗度判定，应注册服务器也可执行的提供器；卸载时 Dispose 注册令牌。坐标使用世界像素，颜色为非负 RGB 浮点值。

## 伤害与饰品栏统一

原 MC `ATTACK_DAMAGE` 映射为 Melee，投射物属性映射为 Ranged；法术属性对应 Magic，战斗召唤物增伤沿用 Summon。原作全伤属性和事件单独保存，不能把所有属性归入 Generic。原生 Generic 与职业加成分别乘算：通用 +20%、近战 +30% 时为 `1.2×1.3`。其他模组仍通过原生 DamageClass、继承规则和命中钩子获得加成。

`DamageArithmetic.Attribute` 保留原属性三种操作：Addition 为固定点数；MultiplyBase 在同一属性内相加；MultiplyTotal 逐项相乘。`AddUniversalDamage` 的 source 必须采用 `modid:attribute_or_event`；同一 source 合并原操作，不同 source 独立相乘。该方法应在每帧装备更新时调用，ResetEffects 会清除临时属性。请勿用物品名称随意拆分原作同一属性，也勿把原来互不相加的事件强行合并。

火、冰、虚空等元素标签与职业相互独立；标签不会默认施加加算或乘算。`DamageHitRule(in DamageHitContext, ref NPC.HitModifiers)` 可明确表达原公式。`DamageArithmetic.Product(1.2f, 1.3f)` 返回 1.56；`DamageArithmetic.AddBonuses(1.2f, 1.3f)` 返回 1.5。Context 提供来源物品／弹幕、玩家、目标、职业与 `HasTag`；调用方须按原作决定运算阶段与分组，避免重复应用已计入原生射弹快照的倍率。使用类型化委托的接口需引用本包程序集。

原生配饰栏等价于通用饰品栏；不会把护甲因支持前缀而设置的 accessory 标记当作饰品资格。专用栏继续由自己的 CanAcceptItem 和原生 ItemLoader 否决决定。未显式登记的第三方 ModAccessorySlot 依据其自身对多种原生饰品的接受规则判断；复杂通用栏应主动登记。需要复制通用栏的外部模组，应读取当前原生、天体果实与其他模组可用通用栏的总数量，避免递归复制自身生成的栏位。布局使用原生保存、同步和点击流程，按窗口自动换列，禁止滚动；失效栏内物品只允许取回，不继续生效。

末影书袋保留 12 格，其中前两格可放石英花。实际携带袋子或将该袋存放于 Void Vault 时，袋内书本进入对应携带被动；原生 Void Bag 本身不替代末影书袋。倒转之启 Boss ×1.1、无止之言 Boss ×1.2 按原资格依次乘算，无止之言携带吸血 10%，启示之书命中点火 3 秒；仍须手持的免死等分支不会因放入书袋而启用。

爆炸致死条件在原生 `Projectile.Damage` / `NPC.StrikeNPC` 作用域判定，支持原生爆炸集合、`explosion` 标签及桥接爆炸伤害类；前一次非致死爆炸不会使之后的普通攻击或环境死亡满足条件。对绕开原生伤害流程直接修改生命的第三方效果，不推测其伤害来源。

## 0.1.12 诅咒计数桥接

直接引用本包程序集的移植可向 `BridgeExtensions.CurseProviders` 注册 `Func<Player, int>`，返回该来源当前实际生效的诅咒条数；卸载时移除自身委托。各来源只贡献自己负责的诅咒，不重复计入七咒或负面前缀，反转或失效后返回相应扣除值。星月遗物通过该接口接入灾厄之册未反转的诅咒。

`LegacyPrefixes.CurseCount(player)` 继续只表示生效装备的总体负面前缀件数。`LegacyPrefixes.TotalCurses(player)` 统一合计前缀、七咒与上述提供者；千咒的伤害、挖掘、回复和原文动态层数均读取总数。无原生触发的灾厄诅咒仍由有效历史记录，刻蚀板反转后贡献0层且显示原作反转说明。

目录中的原始 MC ID 是稳定标识。不同 MC NBT 药水/护符颜色被转换为独立物品 ID，新增移植应维持 ID 稳定，避免存档物品变种错位。不要覆盖别的模组 GlobalItem，不以动态数值 type 作为持久化主键；物品用 ItemIO，跨模组过滤用 `ModName/ItemName`。

联网采用 Terraria 通常的角色归属：本地角色处理物品消耗、伤害与移动，服务器处理 NPC、世界液体和容器持久化。状态包只接受发送者自身角色的状态，NPC 交互核对物品/范围，死亡掉落物品使用 ItemIO。它没有实现服务端角色或反作弊插件，也没有跨服务器存档共享协议。

维度区分统一调用 LegacyRealms，避免只比较 Main.worldID。绑定/死亡 key 为 `worldID|overworld` 或 `worldID|nether`，旧版只存世界 ID 的记录按保存位置推断。既有字段在物品、角色存档及网络中传递。当前口径末地不存在；未来用户提供子世界口径时扩展该服务，不在各物品分散写高度常数。

等级药效由 `Core/LegacyStatus.cs` 与 `Content/Status/LegacyBuffs.cs` 统一处理。本模组同家族取最高等级；较低等级仍用原生 buff 计时，过期/同步/免疫均由 tML 处理，不删除或改写其他模组的药效。

| 稳定 ModBuff 名 | 效果 |
|---|---|
| `StrongRegeneration` / `RegenerationIII` / `RegenerationIV` | 再生 II / III / IV，分别每 75 / 36 / 18 tick 回复 1 换算点 |
| `LegacyStrength` / `StrengthIV` | 力量 II / IV，近战 +6 / +12 原作点；NPC 作用于对玩家/友方 NPC 的接触伤害 |
| `LegacyWeakness` | 虚弱 I，近战或 NPC 接触伤害 -4 原作点 |
| `ResistanceII` / `ResistanceIII` | 攻击减伤 40% / 60% |
| `AbsorptionV` | 玩家临时吸收 20 原作生命；不增加最大生命，不因同等级延时而补满 |
| `LegacyFireResistance` / `MoltenHeart` | 普通火、熔岩及 `fire` 标签弹幕保护；熔火之心仍有烈焰之核计时交互 |
| `LegacySwiftness` / `LegacyLeaping` / `LegacySlowness` | 迅捷 II / 跳跃 II / 缓慢 IV |
| `LegacyTurtleMaster` | 缓慢 VI 与抗性 IV 的组合，分别与同类等级比较 |
| `LegacyHaste` / `StrongHaste` | 玩家急迫 I / II；不加速 NPC AI |
| `LegacySlowFalling` | 玩家慢落/免摔，普通 NPC 下落限速 |

例如可选集成：`if (legacy.TryFind<ModBuff>("RegenerationIII", out var effect)) player.AddBuff(effect.Type, 60 * 60);`。这只需要 Terraria/tML 的类型，不要求硬引用本模组。扩展本移植源码时，可继承 `LeveledLegacyBuff` 声明 `Effect` 与 `Level`，避免在各物品分散实现等级叠加。

`undead` 标签也用于再生/剧毒的亡灵排除；`fire` 标签被玩家和 NPC 的上述抗火效果识别。NPC 移动药效保护 `boss` 标签、`npc.boss` 和 `realLife` 多节实体；不会修改其 AI 计时或永久基础伤害。抗性使用原生攻击伤害钩子，未将不经过这些钩子的持续/环境伤害统一改写。完全吸收使用原生取消或消耗闪避钩子，反馈与 MC 不完全一致；联机角色仍由归属客户端结算，未引入服务端反作弊语义。

## 0.1.7 材料数量映射

`Data/vanilla-map.json` 中的普通字符串仍表示每份原材料换 1 件泰拉物品；也接受 `{ "item": "HellstoneBar", "count": 2 }`，表示每份换 2 件。数量与原配方输入出现次数相乘，再按泰拉物品类型合并；烈焰棒用 2 狱石锭、烈焰粉用 1 狱石锭，同一配方不受九宫格位置影响。转换后的魔矿锭使用原生魔矿/猩红矿配方组，两个世界类型均可制作。

三件特殊武器的第四诅咒豁免继续按攻击来源结算；逐件检查在叠加其他增伤/减伤后仍只抵消第四诅咒的 50% 削减。

## 0.1.6 武器与工具扩展

`Core/LegacyWeapons.cs` 统一声明已实现的攻击、资格、工具默认能力和命中效果，未获口径的武器仍由原生钩子禁用。星光破坏锤在 `PostAddRecipes` 读取 `ContentSamples.ItemsByType` 中其他内容的最大镐力、斧力，分别取最大值；以太工具固定采用 Terraria 1.4.4.9 的原版 230%/175% 基准再略加，不随第三方对原版工具的修改而降低。

`LegacyWeaponProjectile` 按发射时的武器 ID 分派命中效果；切换手持物品不会让别的召唤物获得这次攻击的 Boss 增伤。饕餮之锅使用 `ItemMetadata.WeaponIdentity` 的持久 GUID 跟踪来源物品；背包移动、保存和网络传递保留身份与敌怪种类集合，成长上限 100。同类武器扩展应保持这一归属方式，并使用原生伤害类别、吸血预算和采掘保护。

## 0.1.5 宝箱概率

宝箱规则可指定 `numerator`，省略时为 1；概率为 `numerator / denominator`。大地之心使用 2/25 精确表示 8%。世界注入标记仍沿用原值，更新概率不会让已有世界的箱子重掷。

## 0.1.4 获取与配方扩展（继续保留）

`Data/acquisition.json` 是宝箱池、敌怪组、分母概率、数量、条件及角色奖励的声明来源；`Systems/LegacyAcquisition.cs` 承接原生掉落和世界注入。第三方只需登记 `RegisterNpcTag(npcType, "undead")`，其有效敌怪便可进入灵魂尘全局规则；不依赖灾厄 DLL，也不覆盖原 AI 或掉落表。

宝箱仅向原版 Containers/Containers2 非空箱的空格追加；世界 `chestLootSeededV1` 保存一次注入标记。角色保存 `firstChestReward`、`firstMoonLordReward`，换世界不重置；服务器使用原生个人掉落消息，客户端不重复发奖。新增自定义箱体时应显式加入识别规则，不用反射遍历其他模组箱子。

`Data/hidden-recipes.json` 保留从原作 Java 加载阶段还原的四条配方，普通配方继续由原 JSON 导入。配方组包括七色个人护符及铁/铅、金/铂金；`ItemMetadata` 保存护符所有者，升级回调保留。建筑师卷轴按最新口径使用普通天宇之心，原Blessed字段只保留历史存档兼容，不再作为配方门槛。

## 0.1.3 扩展细节（继续保留）

原生 `TileID.Sets.Ore` 与六种天然宝石默认支持时运。`RegisterFortuneTile` 用于额外登记，注册在卸载时清理。时运只在原生 PickTile 或服务器处理原生挖矿包的同步作用域中生效，捕捉该坐标的 `EntitySource_TileBreak` 实际掉落；不按附近玩家推断来源，不额外执行矿物掉落函数。炸弹或绕过原生流程的自动矿机不猜测归属。

飞行信标默认 `TileID.CrystalBall`，同逻辑维度、50 格方形范围；每 30 tick 缓存扫描。新增信标只用于飞行条件，不生成额外世界结构。原生翼装逻辑保留当前已有翅膀和坐骑，魔力付款不自动喝药。

弹幕还支持 `summon`、`decorative_pet`、`acceleratable` 标签。前者可补充第三方战斗召唤物识别，第二项强制排除装饰宠物，后者只应给可安全乘速度的弹幕；天使不会自动加速复杂自定义 AI 或召唤物。**没有召唤物受伤/转伤接口**，按用户口径只接已有增伤/命中相关效果。

`GetDamageClass` 接受以下 ID；未知 ID 抛出 ArgumentException。它们是伤害来源类别，不附带武器攻击动作。

| ID | 原生流程特性 |
| --- | --- |
| `darkness` | 绕护甲和本模组抗性 |
| `nemesis_curse` | 绕护甲 |
| `wither` | 绕护甲、凋零语义 |
| `void` | 绕护甲/本模组抗性、虚空语义 |
| `indirect_magic` | 绕护甲、魔法继承；虚空珍珠可转治疗 |
| `explosion` | 爆炸语义 |

`owner.ApplyDamageToNPC(npc, amount, 0, 0, false, (DamageClass)legacy.Call("GetDamageClass", "darkness"))` 可直接在服务端使用。`ApplyNpcDamage` 和 `ApplyPlayerDamage` 的数值均为**泰拉点**，不自动重复换算；传入 Terraria 原生 `PlayerDeathReason` 保留归因。直接引用移植程序集的后续代码可以继承公共 `ImportedDamageClass` 声明特性。

这些特性只影响本桥负责的伤害修饰器，不承诺绕过其他模组的任意无敌/脚本减伤；不通过直接改生命来跳过对方的受伤与死亡钩子。六种来源仍继承 Generic；间接魔法还继承 Magic。原版/其他模组已有受伤事件不会自动被重新归类，必须在创建攻击时选用类别或登记对应标签。
