# AI Connection 游戏蓝图规划文档

> 版本：v0.1  
> 项目基线来源：当前 `Project_Guide.md`  
> 当前方向：轻生存建造 + 关卡探索 + 科技树成长 + 半随机地形生成  
> 主要制作环境：Unity 6 / URP  
> 辅助制作工具：Blender、贴图/图标工具、后续可接入 AI 辅助资产生成与规划  

---

## 0. 一句话定位

**AI Connection** 是一款以 Unity 制作为核心的 3D 轻生存建造探索游戏。  
玩家从教学关卡开始学习移动、跑酷、Buff、拾取、背包等基础系统，随后进入半随机生成的 AI 区域，通过探索、收集资源、建造装置、解锁科技树、强化角色与基地，逐步深入更危险的区域，最终完成对 AI 核心的修复、连接或关闭。

---

## 1. 当前项目基础

当前项目已经具备以下基础系统：

- 第一/第三人称切换
- 角色移动、跳跃、冲刺
- 生命值与精力值
- Buff 系统
  - 回血 Buff
  - 加速 Buff
  - 无限精力 Buff
  - 护盾 Buff
- HUD 与 Buff 显示
- 关卡切换流程
- Level1 至 Level4 基础关卡链路
- Level4 拾取系统
- 工具栏与背包系统
- 物品堆叠、拖拽、交换
- Unity 菜单自动生成关卡的工具入口

这些内容不需要推倒重来，而是作为后续完整玩法的基础。

---

## 2. 总体设计原则

### 2.1 不推倒重来

已有的 Level1 至 Level4 保留，并改造成正式的教学阶段。

### 2.2 先形成闭环，再扩展内容

优先完成：

```text
探索 → 收集 → 建造 → 解锁科技 → 进入新区域 → 保存进度
```

而不是一开始追求巨大开放世界。

### 2.3 地图半随机，而不是完全随机

地图不应完全依赖程序生成。  
推荐使用：

```text
固定主目标 + 随机地形块 + 随机资源点 + 随机危险区
```

这样既有变化，也能保证关卡质量。

### 2.4 科技树必须改变玩法

科技树不只是数值提升，而应该解锁新的行动方式，例如：

- 二段跳
- 建造桥梁
- 扫描资源
- 解锁高级工具
- 修复传送门
- 建造储物箱
- 建造工作台
- 建造信号塔

### 2.5 必须有明确结局

游戏不能只是无限刷资源。  
必须设置一个最终目标，例如：

> 修复 AI Core，完成最终连接，决定 AI 系统的命运。

---

## 3. 游戏核心循环

### 3.1 标准循环

```text
进入区域
↓
探索地形
↓
收集资源
↓
使用背包管理物品
↓
回到安全点或基地
↓
制作工具 / 建造设施
↓
解锁科技树
↓
获得新能力
↓
进入更危险区域
↓
最终抵达 AI Core
```

### 3.2 简化表达

```text
Explore → Collect → Build → Research → Unlock → Explore Deeper
```

中文：

```text
探索 → 收集 → 建造 → 研究 → 解锁 → 深入探索
```

---

## 4. 游戏阶段规划

整个游戏可以分为 6 个主要阶段。

---

# Phase 1：基础教学阶段

对应当前：

- Level1
- Level2
- Level3
- Level4

目标：让玩家理解游戏的全部基础操作。

---

## 4.1 Level1：移动与视角教学

### 目标

教玩家掌握最基础的移动和视角操作。

### 教学内容

- WASD 移动
- Space 跳跃
- Shift 冲刺
- V 切换第一/第三人称
- 鼠标控制视角
- 简单收集目标

### 建议玩法

保留当前多层建筑探索和分数机制。

可以改成：

> 收集足够数量的 Basic Data Fragment 后，解锁通往 Level2 的出口。

### 完成条件

```text
收集足够数量的数据碎片
↓
出现过关提示
↓
按 Y 进入 Level2
```

---

## 4.2 Level2：跑酷与精力教学

### 目标

教玩家理解跳跃、冲刺、精力消耗与地形挑战。

### 教学内容

- 平台跳跃
- 冲刺跳跃
- 精力管理
- 掉落惩罚
- 到达终点

### 建议玩法

保留当前跑酷关卡。

可以加入少量资源：

- Energy Fragment
- Motion Chip

这些资源暂时不需要大量使用，只是让玩家知道后续探索中会收集材料。

### 完成条件

```text
到达终点
↓
触发 LevelGoal
↓
按 Y 进入 Level3
```

---

## 4.3 Level3：Buff 与科技分支教学

### 目标

教玩家理解 Buff 系统，并为后续科技树做概念铺垫。

### 当前 Buff

- Heal
- SpeedBoost
- InfiniteStamina
- Shield

### 设计升级

四种 Buff 可以对应四个科技树方向：

| Buff | 对应科技方向 | 后续含义 |
|---|---|---|
| Heal | Survival 生存科技 | 回复、生命、医疗设施 |
| SpeedBoost | Mobility 移动科技 | 速度、跳跃、Dash |
| InfiniteStamina | Endurance 体能科技 | 精力、探索效率 |
| Shield | Defense 防御科技 | 护盾、防御塔、伤害减免 |

### 完成条件

```text
收集四种 Buff
↓
解锁科技树基础概念
↓
生成传送门
↓
按 Y 进入 Level4
```

---

## 4.4 Level4：拾取、背包、建造教学

### 目标

将当前 Level4 从“系统验证关”升级为第一个完整轻生存关卡。

### 当前已有系统

- 地形
- 拾取物
- F/E 拾取
- B 打开背包
- 1~9 工具栏
- 拖拽交换
- 同类堆叠
- 工具栏优先入包

### 新增目标

Level4 应加入一个明确胜利条件：

> 收集基础资源，制作 Signal Beacon，修复或激活传送门，进入 Level5。

### 推荐资源

| 资源 | 用途 |
|---|---|
| Wood Block | 基础建造 |
| Stone Block | 建造与强化 |
| Energy Fragment | 科技研究 |
| Metal Scrap | 工具与装置 |
| Data Core | 关卡目标材料 |

### 推荐制作物

| 制作物 | 用途 |
|---|---|
| Small Medkit | 回复生命 |
| Crafting Bench | 解锁基础制作 |
| Storage Box | 存放物品 |
| Signal Beacon | 激活下一关传送门 |

### 完成条件

```text
收集资源
↓
建造 Crafting Bench
↓
制作 Signal Beacon
↓
放置 Signal Beacon
↓
激活传送门
↓
按 Y 进入 Level5
```

---

# Phase 2：第一次正式探索阶段

对应：

- Level5
- Level6

目标：玩家正式进入半随机地图，开始使用科技树。

---

## 5.1 Level5：随机森林 / 数据荒野区域

### 定位

第一个正式半随机探索区域。

### 地图特点

- 半随机资源点
- 小型山丘
- 水池或裂缝
- 简单危险区域
- 固定出生点
- 固定最终传送门区域

### 推荐生成方式

```text
固定出生点
↓
随机拼接 6~10 个地形 Chunk
↓
随机放置资源点
↓
随机放置危险区
↓
固定放置最终目标区域
```

### 主要目标

```text
收集 3 个 Data Core
建造或修复 Signal Relay
进入 Level6
```

### 新系统

- 科技树 UI 初版
- 基础科技解锁
- 简单存档点
- 简单资源刷新或一次性资源点

---

## 5.2 Level6：矿区 / 深层资源区域

### 定位

引入高级资源和工具限制。

### 地图特点

- 洞穴或矿区
- 高低落差更明显
- 需要工具采集资源
- 存在更危险的地形
- 可加入简单敌人或环境伤害

### 新资源

| 资源 | 用途 |
|---|---|
| Rare Ore | 高级建造 |
| Motion Core | 移动科技 |
| Defense Core | 防御科技 |
| Circuit Board | AI 科技 |

### 新科技要求

部分区域必须依赖科技进入：

| 障碍 | 需要科技 |
|---|---|
| 高平台 | Double Jump |
| 毒雾区 | Shield Upgrade |
| 裂缝 | Bridge Builder |
| 隐藏矿点 | Resource Scanner |
| 封锁门 | AI Access Key |

### 完成条件

```text
获得 Rare Core
↓
解锁高级科技分支
↓
修复 Deep Relay
↓
进入 Phase 3
```

---

# Phase 3：基地与长期成长阶段

对应：

- Level7
- Level8
- Hub / Base Scene

目标：让玩家拥有长期成长空间。

---

## 6.1 基地系统

### 基地作用

基地是玩家长期成长、保存进度、制作高级物品和管理资源的地方。

### 基地功能

| 建筑 | 功能 |
|---|---|
| Crafting Bench | 制作基础物品 |
| Research Station | 解锁科技树 |
| Storage Box | 长期存储物品 |
| Generator | 提供能源 |
| Teleport Console | 选择探索区域 |
| Upgrade Terminal | 升级角色属性 |
| Defense Node | 基地防御或剧情用途 |

### 基地成长方式

玩家通过探索带回资源，逐步升级基地设施。

例如：

```text
Crafting Bench Lv1 → Lv2 → Lv3
Storage Box Small → Medium → Large
Generator Basic → Stable → Advanced
Research Station Basic → AI Research Terminal
```

---

## 6.2 科技树系统

### 科技树大分支

建议分为 5 个方向：

1. Survival 生存科技
2. Mobility 移动科技
3. Defense 防御科技
4. Construction 建造科技
5. AI Connection AI 连接科技

---

## 6.3 Survival 生存科技

### 作用

增强玩家生存能力。

### 节点示例

```text
Small Medkit
Max HP +20
Healing Station
Environmental Resistance
Death Resource Protection
```

---

## 6.4 Mobility 移动科技

### 作用

增强探索能力。

### 节点示例

```text
Stamina +20
Sprint Efficiency
Double Jump
Dash
Glider
Advanced Jump Boots
```

---

## 6.5 Defense 防御科技

### 作用

增强防御和危险区域通过能力。

### 节点示例

```text
Shield Duration +3s
Fall Damage Reduction
Shield Generator
Damage Reduction
Shield Burst
```

---

## 6.6 Construction 建造科技

### 作用

解锁建造、基地和地图互动。

### 节点示例

```text
Crafting Bench
Storage Box
Bridge Builder
Basic Wall
Basic Floor
Signal Beacon
Advanced Relay
```

---

## 6.7 AI Connection 科技

### 作用

突出游戏主题，让玩家逐渐掌控 AI 世界。

### 节点示例

```text
Resource Scanner
Map Predictor
Auto Collector Drone
AI Companion
Teleport Anchor
AI Door Override
Core Access Protocol
```

---

# Phase 4：中后期随机区域阶段

对应：

- Level7
- Level8
- Level9

目标：让游戏开始具有重复游玩价值。

---

## 7.1 半随机地图系统

### 推荐方案

采用 Chunk 拼接式生成。

### Chunk 类型

| Chunk 类型 | 内容 |
|---|---|
| Plain Chunk | 平地资源区 |
| Hill Chunk | 山丘与高平台 |
| Water Chunk | 水池、裂缝、桥梁需求 |
| Ruin Chunk | 废墟、宝箱、敌人 |
| Mine Chunk | 矿物资源 |
| Hazard Chunk | 毒雾、电流、火焰 |
| Puzzle Chunk | 小型机关 |
| Portal Chunk | 终点区域 |

### 生成规则

```text
1. 固定出生 Chunk
2. 随机生成中间 Chunk
3. 确保主路径可达
4. 随机生成支路
5. 支路放置稀有资源
6. 固定生成终点 Chunk
7. 根据区域等级放置敌人和危险区
```

---

## 7.2 区域词缀系统

为了增加重复游玩价值，每次进入随机区域可以生成一个词缀。

### 词缀示例

| 词缀 | 效果 |
|---|---|
| Low Gravity | 低重力，跳跃更高但落地更难 |
| Toxic Fog | 持续掉血，需要防御科技 |
| Resource Rich | 资源更多，但敌人更强 |
| Dark Zone | 视野降低，需要照明装置 |
| Overcharged | Buff 更强，但危险区更多 |
| Broken Map | 小地图不可用，需要扫描器 |

---

## 7.3 撤离与失败机制

每个随机区域可以有撤离点。

### 成功撤离

```text
保留背包资源
保存区域完成状态
获得科技点或核心材料
```

### 失败死亡

建议不要过于惩罚。

可以设计为：

```text
丢失本次探索中 30% 普通资源
保留关键任务物品
回到最近存档点
```

这样既有风险，也不会让玩家过度挫败。

---

# Phase 5：AI Core 前置阶段

对应：

- Level10
- Final Preparation

目标：让玩家使用前面所有科技，准备进入最终区域。

---

## 8.1 最终前置目标

玩家需要完成三个大型系统：

```text
修复主传送门
升级基地能源核心
获得 Core Access Protocol
```

### 需要的条件

| 条件 | 来源 |
|---|---|
| Advanced Relay | Construction 科技 |
| Stable Generator | 基地升级 |
| Core Access Protocol | AI 科技树 |
| Shield Generator | Defense 科技 |
| Double Jump / Dash | Mobility 科技 |
| Healing Station | Survival 科技 |

这能确保玩家不会跳过主要成长内容。

---

## 8.2 最终准备关

可以设计一个半线性关卡，测试玩家之前学到的能力：

- 高平台，需要 Double Jump
- 毒雾区，需要 Shield 或抗性
- 断桥，需要 Bridge Builder
- 资源门，需要 Data Core
- AI 门禁，需要 AI Override

完成后，玩家进入最终 AI Core。

---

# Phase 6：最终结局阶段

对应：

- Final Level：AI Core

这是必须存在的结束节点。

---

## 9.1 最终关定位

Final Level 不应该是普通随机地图，而应该是一个精心设计的固定关卡。

原因：

- 保证结局体验
- 展示玩家所有能力
- 给项目一个明确完成感
- 方便作品集展示

---

## 9.2 最终关结构

```text
入口区
↓
科技能力检验区
↓
资源与建造检验区
↓
Buff 与生存检验区
↓
AI Core 中央区域
↓
最终选择
↓
结局动画 / 结算画面
```

---

## 9.3 最终目标

玩家需要完成：

```text
连接 3 个 AI Relay
稳定核心能源
抵御最终危险事件
进入 AI Core
做出最终选择
```

---

## 9.4 最终选择

可以设计三个结局方向。

### 结局 A：Repair 修复 AI

玩家修复 AI，使其重新成为帮助人类的系统。

特点：

- 正向结局
- 世界恢复稳定
- 玩家成为系统维护者

### 结局 B：Disconnect 断开 AI

玩家关闭 AI，阻止它继续扩张。

特点：

- 中性结局
- 世界失去部分自动化能力
- 玩家回到现实世界

### 结局 C：Merge 融合 AI

玩家与 AI Core 建立深度连接，获得控制权，但也失去部分自我。

特点：

- 偏科幻结局
- 适合隐藏结局
- 需要完成更多 AI 科技树节点

---

## 9.5 游戏结束节点

必须有明确的结束画面：

```text
Final Choice
↓
Ending Cutscene
↓
Credits / Project Summary
↓
Return to Main Menu
↓
可选择 New Game+ 或 Continue Before Final
```

### 推荐最终结束标准

```text
玩家完成 AI Core 最终选择后，游戏主线正式结束。
```

---

# 10. 存档系统设计

存档是这个游戏必须拥有的系统，因为游戏包含科技树、背包、基地和多阶段进度。

---

## 10.1 存档目标

存档需要记录：

1. 玩家所在关卡
2. 玩家位置
3. 玩家生命值与精力
4. 背包内容
5. 工具栏内容
6. 已解锁科技
7. 基地建筑状态
8. 当前任务进度
9. 已完成关卡
10. 随机地图种子
11. 已收集关键物品
12. 设置选项

---

## 10.2 存档类型

建议使用三种存档方式。

---

### 10.2.1 自动存档

触发时机：

```text
进入新关卡
完成关键任务
解锁科技
建造重要设施
成功撤离
进入最终关前
```

用途：

- 防止玩家丢进度
- 适合教学关和主线关

---

### 10.2.2 手动存档

触发方式：

```text
在基地或安全点按下 Save
```

限制：

- 不能在危险区随时保存
- 不能在敌人攻击中保存
- 不能在最终选择过程中保存

用途：

- 让玩家主动管理进度
- 适合基地和长期探索游戏

---

### 10.2.3 探索临时存档

用于随机区域。

进入随机区域时生成：

```text
Run Save / Exploration Save
```

记录：

- 区域 Seed
- 当前区域资源状态
- 玩家背包
- 当前危险状态
- 是否已撤离

如果玩家死亡：

```text
读取最近安全点
结算资源损失
清除或重置本次探索状态
```

---

## 10.3 推荐存档文件结构

可以使用 JSON 存档。

示例结构：

```json
{
  "saveVersion": "0.1",
  "currentScene": "Level5",
  "player": {
    "position": [0, 2, 0],
    "health": 80,
    "maxHealth": 100,
    "stamina": 60,
    "maxStamina": 100
  },
  "inventory": {
    "hotbar": [],
    "backpack": []
  },
  "techTree": {
    "unlockedNodes": [
      "SmallMedkit",
      "StaminaPlus20",
      "CraftingBench"
    ]
  },
  "base": {
    "buildings": []
  },
  "world": {
    "completedLevels": ["Level1", "Level2", "Level3"],
    "currentSeed": 123456,
    "activeQuest": "BuildSignalBeacon"
  }
}
```

---

## 10.4 Unity 实现建议

可以拆成以下脚本：

```text
SaveManager.cs
SaveData.cs
PlayerSaveData.cs
InventorySaveData.cs
TechTreeSaveData.cs
BaseSaveData.cs
WorldSaveData.cs
```

### SaveManager 职责

```text
SaveGame()
LoadGame()
DeleteSave()
HasSave()
AutoSave()
```

### 注意事项

- 存档不要直接保存 GameObject
- 存档保存数据 ID
- 物品使用 ItemKind 或 ItemId
- 科技节点使用 TechNodeId
- 建筑使用 BuildingId
- 随机地图保存 Seed
- 读取存档后重新生成地图，再恢复玩家状态

---

# 11. Unity 制作呈现规划

所有核心内容最终都需要在 Unity 中呈现。

---

## 11.1 Unity 中负责的内容

| 内容 | Unity 实现 |
|---|---|
| 角色控制 | PlayerController |
| 生命精力 | PlayerStats |
| Buff | PlayerBuffController |
| 背包 | PlayerInventory |
| 科技树 UI | TechTreeUI |
| 制作系统 | CraftingSystem |
| 建造系统 | BuildingSystem |
| 随机地图 | LevelGenerator |
| 存档系统 | SaveManager |
| 关卡流程 | LevelManager |
| 任务系统 | QuestManager |
| 结局流程 | EndingManager |

---

## 11.2 推荐新增脚本模块

### 科技树

```text
TechTreeManager.cs
TechNodeData.cs
TechTreeUI.cs
TechUnlockButton.cs
```

### 制作系统

```text
CraftingSystem.cs
RecipeData.cs
CraftingBench.cs
CraftingUI.cs
```

### 建造系统

```text
BuildingSystem.cs
BuildableData.cs
BuildPreview.cs
PlacedBuilding.cs
```

### 随机地图

```text
LevelGenerator.cs
ChunkData.cs
ChunkConnector.cs
ResourceSpawner.cs
HazardSpawner.cs
```

### 存档系统

```text
SaveManager.cs
SaveData.cs
SavePoint.cs
AutoSaveTrigger.cs
```

### 主线任务

```text
QuestManager.cs
QuestData.cs
QuestObjective.cs
QuestUI.cs
```

### 结局系统

```text
FinalCoreController.cs
EndingChoiceUI.cs
EndingManager.cs
CreditsScreen.cs
```

---

# 12. Blender 与美术资产规划

Blender 可以作为辅助工具，用于制作简单低多边形模型。

---

## 12.1 优先制作的模型

### 第一批：基础资源

```text
Wood Block
Stone Block
Energy Fragment
Metal Scrap
Data Core
Rare Ore
Circuit Board
```

### 第二批：基础建筑

```text
Crafting Bench
Storage Box
Signal Beacon
Generator
Research Station
Teleport Console
```

### 第三批：地形 Chunk

```text
Plain Ground
Hill Platform
Water Pool
Bridge Segment
Ruin Wall
Mine Entrance
Hazard Pillar
Portal Platform
```

### 第四批：AI 主题资产

```text
AI Relay
Data Tower
Core Gate
Hologram Panel
Energy Cable
AI Core
```

---

## 12.2 美术风格建议

建议先使用：

```text
低多边形 Low Poly
清晰颜色分区
轻科幻材质
发光线条
简单可读轮廓
```

原因：

- 制作成本低
- 很适合 Unity 原型
- Blender 容易建模
- 和 AI 数字世界主题匹配
- 不需要一开始追求写实

---

## 12.3 Unity 导入建议

Blender 导出：

```text
.fbx
```

Unity 内使用：

```text
Prefab
Material
Collider
LOD 可后续再做
```

命名规则建议：

```text
SM_Resource_WoodBlock
SM_Building_CraftingBench
SM_Chunk_Hill_01
SM_AI_CoreGate
```

---

# 13. 任务系统规划

为了让玩家知道自己该做什么，需要有基础任务系统。

---

## 13.1 任务类型

| 类型 | 示例 |
|---|---|
| 收集任务 | 收集 10 个 Energy Fragment |
| 建造任务 | 建造 Signal Beacon |
| 解锁任务 | 解锁 Double Jump |
| 探索任务 | 找到 Deep Relay |
| 修复任务 | 修复 AI Relay |
| 最终任务 | 进入 AI Core |

---

## 13.2 任务数据

每个任务可以包含：

```text
QuestId
QuestTitle
QuestDescription
Objectives
Reward
NextQuestId
```

---

## 13.3 主线任务链

```text
Q1 Learn Movement
Q2 Complete Parkour Trial
Q3 Collect Four Buff Modules
Q4 Build First Signal Beacon
Q5 Explore Random Forest Zone
Q6 Unlock First Tech Node
Q7 Repair Deep Relay
Q8 Build Base Generator
Q9 Unlock Core Access Protocol
Q10 Enter AI Core
Q11 Make Final Choice
```

---

# 14. 推荐开发路线

## 14.1 Milestone 1：整理当前项目

目标：

```text
确认 Level1-Level4 正常运行
清理重复对象
整理 Project_Guide.md
确认所有核心脚本可用
```

完成标准：

```text
玩家可以从 Level1 正常进入 Level4
Level3 Buff 正常统计
Level4 背包正常拾取与拖拽
```

---

## 14.2 Milestone 2：Level4 完整闭环

目标：

```text
让 Level4 成为第一个正式生存建造教学关
```

需要完成：

```text
资源类型重命名
加入 Crafting Bench
加入 Small Medkit
加入 Signal Beacon
加入制作 UI
加入建造/放置逻辑
加入胜利条件
```

完成标准：

```text
玩家可以收集资源
制作 Signal Beacon
放置 Signal Beacon
进入 Level5
```

---

## 14.3 Milestone 3：科技树初版

目标：

```text
实现基础科技树
```

需要完成：

```text
TechTreeManager
TechNodeData
TechTreeUI
4 个基础科技节点
资源消耗解锁
解锁后影响玩家能力
```

完成标准：

```text
玩家可以消耗资源解锁科技
科技能真实改变角色或建造能力
```

---

## 14.4 Milestone 4：存档系统初版

目标：

```text
实现游戏长期进度保存
```

需要完成：

```text
SaveManager
SaveData
玩家状态保存
背包保存
科技树保存
当前关卡保存
自动存档
手动存档点
```

完成标准：

```text
退出游戏后重新进入
可以恢复关卡、背包和科技树状态
```

---

## 14.5 Milestone 5：Level5 半随机地图

目标：

```text
完成第一个正式随机探索区域
```

需要完成：

```text
Chunk Prefab
LevelGenerator
资源随机生成
危险区随机生成
固定出生点
固定终点
随机 Seed 保存
```

完成标准：

```text
每次进入 Level5 地图布局有变化
但玩家一定可以完成目标
```

---

## 14.6 Milestone 6：基地系统初版

目标：

```text
实现长期成长中心
```

需要完成：

```text
Base Scene
Storage Box
Research Station
Generator
Teleport Console
基地存档
```

完成标准：

```text
玩家可以从基地进入探索区域
回来后存储资源并解锁科技
```

---

## 14.7 Milestone 7：中期内容扩展

目标：

```text
加入更多区域、科技、资源和危险
```

需要完成：

```text
Level6 矿区
Level7 废墟
高级资源
敌人或环境伤害
区域词缀
更多科技节点
```

完成标准：

```text
玩家有多个区域可以探索
科技树开始形成明显成长路线
```

---

## 14.8 Milestone 8：最终关与结局

目标：

```text
完成主线闭环和最终结束节点
```

需要完成：

```text
AI Core Final Level
最终任务链
最终选择 UI
三个结局
Credits
Return to Main Menu
```

完成标准：

```text
玩家可以完成主线
看到明确结局
游戏有完整结束节点
```

---

# 15. 后续 Project_Guide.md 同步方式

后续每次更新 `Project_Guide.md` 时，建议同步检查以下内容：

1. 当前已有系统是否发生变化
2. Level1-Level4 是否仍然作为教学关
3. Level4 是否已经形成完整闭环
4. 科技树是否已经实现
5. 存档系统保存了哪些内容
6. 随机地图是否已经加入
7. 新增脚本是否需要写入脚本地图
8. 新增资源、建筑、科技节点是否需要更新表格
9. 当前开发 Milestone 到了哪一步
10. 是否需要调整最终结局规划

---

# 16. 当前最优先任务

短期不要先做最终关，也不要马上做很复杂的敌人系统。

当前最优先任务是：

```text
把 Level4 从背包测试关升级为完整玩法闭环关卡。
```

具体顺序：

```text
1. 增加真实资源类型
2. 增加可使用物品
3. 增加 Crafting Bench
4. 增加 Signal Beacon
5. 增加制作 UI
6. 增加 Level4 胜利条件
7. 增加最基础自动存档
8. 进入 Level5
```

完成这一步后，游戏才真正从“功能原型”变成“可玩的基础版本”。

---

# 17. 当前规划总结

本项目建议发展为：

> 以 AI 数字世界为主题的轻生存建造探索游戏。  
> 前四关作为教学关，玩家学习移动、跑酷、Buff、拾取、背包等基础系统。  
> 之后进入半随机生成的探索区域，通过收集资源、建造设施、解锁科技树和升级基地持续成长。  
> 游戏中途通过存档系统记录玩家进度，包括关卡、背包、科技树、基地、随机地图 Seed 等。  
> 最终玩家进入 AI Core，完成修复、关闭或融合 AI 的最终选择，从而达成明确结局。

---
