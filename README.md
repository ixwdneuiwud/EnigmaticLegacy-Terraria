# 神秘遗物：Terraria 非官方移植

Enigmatic Legacy 2.30.0 → tModLoader 1.4.4 的 C# 移植，当前源码版本 **0.1.19**。原作归 **Aizistral / Aizistral Studios 及贡献者**所有；本仓库由 ixwdneuiwud 维护，Terraria 移植开发、代码改写、资源适配及修复完全由 AI（GPT）完成，非原作者官方发布。

[原作仓库](https://github.com/Aizistral-Studios/Enigmatic-Legacy) · [原作许可](LICENSE-UPSTREAM.md) · [移植代码许可](LICENSE.md) · [素材归属](THIRD_PARTY_NOTICES.md)

## 内容与适配

182 个稳定数据 ID，涵盖物品、药效变体及已接入方块；另有原生材料方块。保留原物品说明的普通、Shift、Alt 内容、动画及休眠之眼语音。使用 Terraria 的饰品/装备、词缀、伤害类别、飞行、合成、掉落、存档和联机接口，提供可选的灾厄兼容入口。

按本移植的既定设计：主世界/地狱以高度区分；经验消耗使用魔力；抢夺/时运采用概率双倍掉落；附魔、饥饿、作物、红石等未对应机制暂停；部分攻击或获取渠道仍按现有清单留空。移植的材料替代、阶段和数值不等于 Minecraft 原版。详细行为见模组目录中的 `docs/`，报告中的历史全套私人移植数据不代表本仓库包含其他模组。

## 从源码构建（Windows）

1. 安装正版 Terraria 与 tModLoader 1.4.4。本快照在 tModLoader **2026.8.3.0** 上验证，使用安装目录自带的 `dotnet/dotnet.exe`，不需要单独安装 .NET SDK。
2. 安装 Python 3.10+ 与 `tools/requirements.txt` 中的 Pillow，并准备本机 Minecraft 1.20.1 客户端 JAR，用于恢复不纳入 Git 的原版图标。
3. 在仓库根目录运行：

```powershell
.\build.ps1 -TModLoaderPath 'D:\SteamLibrary\steamapps\common\tModLoader' -MinecraftJar 'D:\Minecraft\versions\1.20.1\1.20.1.jar'
```

输出在 `.artifacts/EnigmaticLegacy.tmod`，隔离构建目录在 `.build/PortBuildProfile`。脚本不修改当前启用模组、角色或世界。下界之星与靴槽保留 Minecraft 原图；图标作为本地构建输入恢复，源码仓库不提供图片副本，权利归属见 THIRD_PARTY_NOTICES.md。

构建后的 `.tmod` 可手动放入 tModLoader 存档目录的 `Mods` 文件夹。要验证源码，使用隔离测试角色/世界及 `-elselftest`；诊断不会在正常游玩中自行启动。

## 源码与公开范围

- `ModSources/EnigmaticLegacy/`：实际运行的完整 C#、JSON、HJSON、授权素材及说明。
- `third_party/curios/slots/`：复用槽位图标的原始 PNG；许可全文见 `licenses/`。
- `tools/restore_external_assets.py`、`external-assets.json`：外部 Minecraft 构建输入的恢复及像素校验。
- `docs/validation.md`：这份公开快照的验证记录；不把早期测试当作当前客户端验收。

神秘遗物附加、月之石、Goety 等独立模组不在本仓库。此仓库公开源码，尚未代表已经发布到 Steam 创意工坊，也不包含其他模组、游戏二进制或个人存档。
