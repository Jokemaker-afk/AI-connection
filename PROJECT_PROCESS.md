# AI Connection 项目总览（用于 AI 游戏大纲输入）

> 本文档是当前项目的**最新玩法与实现基线**。可直接提供给 AI，用于产出：游戏大纲、功能拆解、技术方案、任务排期。
>
> **最近同步**：2026-06-02 · 分支 `work` · 已含 Level1→Level6 链路、准星拾取、制造/放置、Level5 进度信标。

---

## 1. 项目定位

- **项目名**：AI connection
- **引擎版本**：Unity 6 (`6000.4.7f1`)
- **渲染管线**：URP
- **当前类型**：第三/第一人称切换的关卡闯关 + 生存制造教学原型
- **核心体验**：移动与平台挑战 + Buff 教学 + 准星拾取/背包 + 随身/工作台制造 + 热键栏放置建造 + 关卡进度信标

---

## 2. 当前可玩主流程（关卡链路）

### 2.1 关卡顺序

1. `SampleScene`（Level1）
2. `Level2`
3. `Level3`（Buff 教学 Hub）
4. `Level4`（拾取 + 背包 + 收集进度 → 传送门）
5. `Level5`（制造与放置教学 → 信标）
6. `Level6`（占位场景，尚无玩法目标）

以上场景均已写入 `EditorBuildSettings`。

### 2.2 通关逻辑

- **Level1**：分数达标后，按 `Y` 进入 Level2
- **Level2**：到达终点后，按 `Y` 进入 Level3
- **Level3**：四路 Buff 全收集后弹出进入下一关提示，按 `Y` 进入 Level4；场景中也会生成传送门
- **Level4**：拾取地图上散布物品，**收集率达到 30%** 后在地图中央生成通往 Level5 的传送门；走进传送门加载 Level5
- **Level5**：
  1. 收集基础材料（木材、石头等）
  2. 按 `E` 打开制造 UI → 随身制作 **4 木板 → 工作台**（仅需 `BasicSurvival` 科技）
  3. 热键栏选中工作台 → 准星对准地面 → **左键** 放置
  4. 靠近已放置工作台按 `E` → 在工作台配方中制作 **熔炉**（需 `Stonework`，放置工作台后自动解锁）
  5. 放置熔炉 → **信标激活**（一次性锁定，收回物品不影响）→ 走进信标传送门进入 Level6
- **Level6**：仅占位地板与欢迎标签，无胜利条件

### 2.3 Level5 运行时特殊处理

- `Level5ProgressionBootstrap.RemovePreplacedWorkstations()` 会在 Play 时销毁场景中预置的 `CraftingTableStation` / `Stations`，强制玩家自行制造放置
- Level5.unity 场景文件里可能仍 baked 旧工作台对象；需 Play 验证或重跑 `Tools → Setup Level 5` 后保存以永久清理

---

## 3. 操作说明（玩家输入）

### 3.1 角色移动与视角

- `W A S D`：移动
- `Space`：跳跃（消耗精力）
- `Shift`：冲刺（持续消耗精力）
- `V`：第一/第三人称切换
- 鼠标：视角控制

### 3.2 关卡与 Buff 过渡（Level1~3）

- `Y`：确认进入下一关（关卡提示出现时）
- `R`：在 Level3 的返回区回到 Hub 原点

### 3.3 拾取、交互、制造、放置（Level4 起全局生效）

| 按键 | 功能 |
|------|------|
| `F` | 拾取准星识别且在**玩家拾取距离内**的世界物品 / 已放置可收回物 |
| `E` | 打开制造 UI：无目标时 = 随身制造；准星对准已放置工作站 = 该站上下文制造 |
| `B` | 打开/关闭背包 |
| `1~9` | 切换工具栏选中槽位 |
| 左键 | 热键栏选中**可放置物**时确认放置 |
| `Esc` | 关闭制造面板 |

**注意**：Level5 世界提示文字仍写「C 随身制作」，实际按键为 **`E`**（以代码为准，待统一文案）。

### 3.4 准星与第三人称

- 所有世界交互（拾取、工作站、放置射线）均从**屏幕准星**发出，**禁止**用 `Input.mousePosition` / 鼠标 hover 做拾取判定
- 第三人称采用 Fortnite 式过肩视角（右肩偏移、碰撞缩短臂长、准星 UI 微偏移）
- 第三人称目标辅助偏角约 **15°**（`PlayerGameplayTargeting.thirdPersonTargetAssistAngle`）

---

## 4. 关卡设计现状

## 4.1 Level1（SampleScene）

- 多层建筑探索 + 分数机制
- 达到阈值触发过关提示
- 主要用于基础移动、相机、冲刺跳跃手感验证

## 4.2 Level2（跑酷）

- 平台跳跃与障碍
- 到达终点触发过关

## 4.3 Level3（Buff 教学 Hub）

四个方向道路，每条路放置一种教学 Buff：

1. **回血 Buff**（配合必中扣血区演示）
2. **加速 Buff**（15 秒）
3. **无限精力 Buff**（7 秒）
4. **护盾 Buff**（可抵挡伤害，破盾后短暂无敌）

并包含：

- Buff 顶部提示 Overlay
- Buff 动态 HUD（最多 6 格）
- 集齐 4 Buff 后触发进入 Level4 的提示链路

## 4.4 Level4（拾取、背包与收集进度）

- 56×56 占位地形（路径、丘陵、岩石、水池）
- 散布多种可拾取物（Legacy 色块 + 扩展物品类型）
- **CollectibleManager**：统计拾取进度，默认阈值 **30%**
- 达标后 **PortalUnlockManager** 在地图中央 `(0, 0.65, 0)` 生成传送门 → `Level5`
- **CollectibleProgressHud**：顶部显示收集百分比

## 4.5 Level5（制造与放置教学）

- 44×44 盆地 + 四向材料垫、岩石装饰
- 约 30 个随机散布的基础材料拾取 + 两处材料环
- **无预置工作台**（运行时删除）；玩家必须：材料 → 木板 → 工作台 → 熔炉
- **SceneProgressionManager** 要求至少放置一次：`CraftingTable` + `Furnace`
- 条件满足后 **SceneBeacon** 在 `(0, 0.65, 12)` 激活传送门 → `Level6`
- **SceneProgressionHud**：中文目标条（「制造并放置工作台」→「使用工作台制造并放置熔炉」→「前往信标」）

## 4.6 Level6（占位）

- 24×24 地板 + 「第六关占位场景」标签
- 无进度系统、无敌人、无制造教学目标

---

## 5. 角色与数值系统

## 5.1 PlayerController

- CharacterController 驱动移动
- 支持 coyote time / jump buffer（跳跃手感）
- 支持冲刺倍率与重力参数

## 5.2 PlayerStats

- 生命值 / 精力值基础系统
- 精力消耗（冲刺、跳跃）与延迟恢复
- Buff 相关状态：
  - 护盾
  - 无敌
  - 无限精力

## 5.3 PlayerBuffController

- 处理 Buff 应用
- 维护加速持续时间
- 提供 Buff 显示数据供 HUD 使用

---

## 6. Buff 与 HUD 系统

## 6.1 Buff 类型

- `Heal`
- `SpeedBoost`
- `InfiniteStamina`
- `Shield`

## 6.2 Buff 可视化

- `PlayerBuffHud` 在主 HUD 下方显示 Buff 槽位
- 持续型 Buff 显示动态衰减（线性下降）
- 护盾与无敌有独立显示标签

## 6.3 提示层

- `BuffNotificationOverlay`：顶部信息条（获得 Buff、传送门/信标激活等）
- `LevelTransitionOverlay`：关卡切换中央信息

## 6.4 新增 HUD 组件

- `GameplayCrosshairHud`：屏幕准星（供 `CrosshairRayUtility` 对齐）
- `CollectibleProgressHud`：Level4 收集进度
- `SceneProgressionHud`：Level5 放置目标
- `CraftingHud`：多列制造面板（分类筛选、Shift+滚轮横向滚动）

---

## 7. 准星拾取与背包系统

## 7.1 架构（调度链）

```
CrosshairRayUtility（准星 ScreenPointToRay，禁止鼠标拾取）
  → PlayerPickupTargeting（三阶段检测 + 距离分级 + 粘性目标）
  → PlayerGameplayTargeting（总调度：拾取 / 放置物 / 工作站 + 高亮）
  → PlayerInventoryHud（提示文案）/ PlayerPickupInteractor（F/E）
```

## 7.2 三阶段拾取检测（PlayerPickupTargeting）

1. **精确 Raycast**：准星射线命中 `WorldPickupItem` 碰撞体
2. **SphereCast**：辅助宽容命中（半径默认 1，可配置）
3. **锥形 Overlap 评分**：角度 + 距离加权，配合 **粘性目标**（0.2s 内不轻易切换）

## 7.3 双距离模型（第一/第三人称一致）

| 参数 | 默认值 | 含义 |
|------|--------|------|
| `maxCameraTargetDistance` | 9 | 准星可**识别**物品、显示中文名 |
| `playerPickupReach` | 4 | 真正可拾取、显示「按 F 拾取」、显示地面圆环/高亮 |

**状态分级**：

- **无目标**：无提示
- **状态 B（识别但太远）**：仅显示中文名（如「木材」），无 F 提示、无高亮、无圆环
- **状态 C（范围内）**：`物品名\n按 F 拾取`；启用 emission 高亮 + 地面圆环

## 7.4 Layer 与碰撞

- `GameplayLayers.PickupItem`（Layer 6）：拾取物专用层
- `GameplayLayers.Player`（Layer 7）：玩家层，射线忽略
- `PickupDetectionMask`：PickupItem + Default 回退（兼容旧场景）
- `WorldPickupItem`：含 `InteractionPoint`、实心 Raycast 碰撞体；拾取层自动设置

## 7.5 地面拾取圆环（PickupGroundRingMarker）

- 从物品上方 RaycastAll **向下**，跳过物品自身碰撞体，贴地放置半透明圆环
- 仅 `CanPickupSelected`（在 `playerPickupReach` 内）时显示（`showPickableMarkerOnlyInRange`）
- 已移除 `PlayerPickupTargeting` 上的第二套头顶标记，避免双圆环悬空

## 7.6 物品定义（ItemKind 扩展）

- **Legacy**：Level4 十色方块（仍可用于测试）
- **Level5 基础材料**：Wood, Stone, Grass, Fiber, Vine, Flint, Clay, OreFragment, Coal, Berry
- **中间材料**：Plank, Stick, Rope, Cloth, Brick, MetalIngot
- **功能/工具**：CraftingTable, StoneAxe, StonePickaxe, SimpleBackpack, Campfire, Furnace, Bandage, BasicSword, KeyFragment
- **可放置建筑**：WoodWall, StoneWall, WoodFloor, StoneFloor
- **工作站**：Forge, Loom, AlchemyTable, ScienceLab
- **储物**：WoodChest, LargeChest
- 中文显示名：`ItemCatalog` + `ItemKindUtility.GetDisplayName`；UI 字体：`GameplayChineseText`（OS 动态字体预烘焙）

## 7.7 入包优先级（已实现）

按以下顺序填充：

1. 工具栏同类堆叠
2. 背包同类堆叠
3. 工具栏空位
4. 背包空位

## 7.8 背包交互（已实现）

- 工具栏 9 格（屏幕底部）
- 背包 27 格（3×9）
- 打开背包后可鼠标拖拽：空格移动、同类合并、异类交换
- 图标为运行时生成的截面风格 sprite（非美术 Atlas）

## 7.9 已放置物交互

- `PlacedPickupable`：准星对准已放置物可收回入包（F）
- 收回不影响 Level5 信标（`SceneProgressionManager.latchBeaconOnce = true`）

---

## 8. 制造系统（Crafting）

## 8.1 核心组件

- `CraftingRecipeDatabase`：静态配方注册（随身 / 工作台 / 熔炉 / 锻造 / 织布 / 炼金 / 建筑 / 储物）
- `CraftingManager`：消耗材料、产出入包、触发科技
- `CraftingUnlockEvaluator`：材料 / 科技 / 工作站 / 已放置建筑 四重校验
- `CraftingHud`：多工作站并列列 UI + 分类 Tab + 锁定原因中文提示
- `WorkstationDetector`：以玩家位置 4m 内已放置 `CraftingStation` 决定制造上下文

## 8.2 制造入口

- **随身**（`WorkstationKind.None`）：按 `E`，始终可用列「随身可建造」
- **工作站**：准星对准已放置工作台/熔炉等 → 按 `E`；或站在范围内按 `E`（`WorkstationDetector` 合并上下文）

## 8.3 Level5 教学关键配方

| 配方 ID | 产出 | 材料 | 上下文 | 科技 |
|---------|------|------|--------|------|
| `plank` | 木板 ×2 | 木材 ×1 | 随身 | BasicSurvival |
| `crafting_table` | 工作台 ×1 | 木板 ×4 | 随身 | BasicSurvival |
| `furnace` | 熔炉 ×1 | 石头 ×6, 黏土 ×2, 煤炭 ×2 | **已放置工作台** | Stonework |

- 放置工作台 → 自动解锁 `Woodworking` + `Stonework`
- 放置熔炉 → 自动解锁 `Smelting`
- 熔炉配方必须在**已放置且注册**的工作台附近打开制造 UI 才可见

## 8.4 科技树（TechnologyManager）

- 初始解锁：`BasicSurvival`
- 放置建筑触发：`Woodworking` / `Stonework` / `Smelting` / `Metalworking` / `Storage` / `BasicCombat`
- 配方可声明 `RequiredTechnologies[]`；未解锁时制造 UI 显示锁定原因

## 8.5 已注册但未在 Level5 强制的扩展内容

- 锻造台、织布台、炼金台、科技研究台及对应产物链
- 石斧/石镐、营火、绷带、剑、墙/地板、木箱/大箱等
- 可作为 Level6+ 内容直接复用

---

## 9. 放置建造系统（Placement）

## 9.1 流程

1. 工具栏选中 `ItemKindUtility.IsPlaceable` 物品
2. `PlayerPlacementController` 用准星射线求放置点（`PlacedObjectBuilder.TryGetPlacementPosition`）
3. 半透明预览：合法 = 绿色材质，非法 = 红色
4. **左键**确认 → 消耗热键栏 1 个 → `PlacedObjectBuilder.SpawnPlacedObject`

## 9.2 放置规则（PlacedObjectBuilder）

- 1m 网格对齐（`GridSize = 1`）
- 表面法线向上阈值（`MinUpNormal = 0.65`）
- 重叠检测（`IsPlacementValid` + `PlacementCheckPadding`）
- 支持 **垂直堆叠**：`PlacementSurface.canSupportPlacedObjects`（可放置物可叠在其它可放置物上）
- 最大放置距离默认 **8m**（与 `PlayerGameplayTargeting.maxPlacementDistance` 一致）

## 9.3 放置物组成

- `ItemVisualBuilder` 视觉
- `PlacedBuilding` → 注册 `BuildingRegistry`、上报 `RequiredPlacedObjectTracker`、解锁科技
- `CraftingStation`（若为该物品的工作站）
- `PlacedPickupable`（可收回）
- `ItemWorldLabel` 中文名牌

---

## 10. 关卡进度与传送系统

## 10.1 Level4：收集解锁传送门

- `CollectibleManager` + `PortalUnlockManager`
- 阈值 30%，一次性生成 `PortalVisualBuilder` 传送门
- `ScenePortal` 加载目标场景

## 10.2 Level5：放置解锁信标

- `RequiredPlacedObjectTracker`：全局单例，记录「至少放置过一次」的 `ItemKind`
- `SceneProgressionManager`：监听放置事件 → 全部满足 → `SceneBeacon.Activate()`
- `SceneBeacon`：生成视觉 + `ScenePortal.SetPortalEnabled(true)` + Buff 通知
- **Latch 行为**：信标激活后即使收回工作台/熔炉也不撤销

## 10.3 通用传送组件

- `PortalVisualBuilder`：运行时生成门柱 + 标牌
- `ScenePortal`：触发器进入加载；支持 `SetPortalEnabled` / `Configure`

---

## 11. Level3 → Level4 过渡系统（Buff Hub）

核心对象：

- `BuffCollectionTracker`：统计四种 Buff 收集情况
- `HubAdvancePortal`：集齐后生成传送门，并触发进入下一关提示
- `LevelManager`：统一处理 `Y` 键确认后的场景加载

兜底策略：

- 集齐 4 Buff 时，直接调用 `LevelManager.NotifyGoalReached()`
- 传送门出现时也会触发一次 `NotifyGoalReached()`

---

## 12. 编辑器菜单与自动化入口

位于 `Tools` 菜单：

- `Setup Level 1 (Score Win)`
- `Setup Level 2 (Parkour -> Level 3)`
- `Create Level 2 Scene (Parkour)`
- `Create Level 3 Scene (Buff Hub)`
- `Setup Level 3 (Buff Hub -> Level 4)`
- `Create Level 4 Scene`
- `Setup Level 4 (Collectibles -> Level 5 Portal)`
- `Create Level 5 Scene (Crafting Tutorial Placeholder)`
- `Setup Level 5 (Crafting Tutorial)`
- `Create Level 6 Scene (Placeholder)`

推荐重建流程（当场景配置错乱时）：

1. `Create Level 4 Scene` → `Setup Level 4`
2. `Create Level 5 Scene` → `Setup Level 5`
3. `Create Level 6 Scene`（如需重置第六关占位）

运行时自动挂载：

- `Level4SceneAutoSetup` / `Level5SceneAutoSetup` / `Level6SceneAutoSetup`（`RuntimeInitializeOnLoadMethod`）
- `GameplayRigBuilder.EnsureCoreGameplayObjects`：Player + 全套交互组件 + HUD
- `GameplayProgressionBootstrap`：TechnologyManager + BuildingRegistry + RequiredPlacedObjectTracker

---

## 13. 关键脚本地图（给 AI 用）

## 13.1 关卡与流程

- `LevelManager.cs`：关卡过渡（Level1~3 Y 键）
- `LevelGoal.cs`：到点触发过关
- `Level3SceneAutoSetup.cs` / `Level4SceneAutoSetup.cs` / `Level5SceneAutoSetup.cs`
- `Level3BuffHubBuilder.cs` / `Level4PlaceholderBuilder.cs` / `Level5PlaceholderBuilder.cs` / `Level6PlaceholderBuilder.cs`
- `Level5ProgressionBootstrap.cs`：Level5 进度 + 删预置工作台
- `CollectibleManager.cs` / `PortalUnlockManager.cs`
- `SceneProgressionManager.cs` / `SceneBeacon.cs` / `RequiredPlacedObjectTracker.cs`
- `ScenePortal.cs` / `PortalVisualBuilder.cs`

## 13.2 玩家、相机与 targeting

- `PlayerController.cs` / `PlayerStats.cs` / `PlayerCameraController.cs`
- `CrosshairRayUtility.cs`：准星射线唯一入口
- `PlayerPickupTargeting.cs`：拾取三阶段 + 双距离
- `PlayerGameplayTargeting.cs`：拾取/工作站/放置物总调度 + 高亮
- `PlayerPickupInteractor.cs`：F 拾取 / E 交互入口
- `PickupGroundRingMarker.cs`

## 13.3 制造与放置

- `CraftingRecipeDatabase.cs` / `CraftingRecipe.cs` / `CraftingManager.cs`
- `CraftingUnlockEvaluator.cs` / `CraftingHud.cs` / `CraftingStation.cs`
- `PlayerCraftingInteractor.cs` / `WorkstationDetector.cs`
- `PlayerPlacementController.cs` / `PlacedObjectBuilder.cs` / `PlacedBuilding.cs`
- `PlacedPickupable.cs` / `PlacementSurface.cs` / `BuildingRegistry.cs`
- `TechnologyManager.cs` / `TechnologyKind.cs`

## 13.4 物品与 UI

- `ItemKind.cs` / `ItemCatalog.cs` / `ItemData.cs` / `ItemVisualBuilder.cs`
- `WorldPickupItem.cs` / `ItemWorldLabel.cs` / `BillboardLabel.cs`
- `PlayerInventory.cs` / `PlayerInventoryHud.cs` / `InventorySlotData.cs`
- `GameplayHudBootstrap.cs` / `GameplayUiUtility.cs` / `GameplayChineseText.cs`
- `GameplayLayers.cs` / `GameplayCursorPolicy.cs` / `GameplayCrosshairHud.cs`
- `CollectibleProgressHud.cs` / `SceneProgressionHud.cs`

## 13.5 Buff 与战斗（Level1~3）

- `PlayerBuffController.cs` / `BuffType.cs` / `BuffCollectionTracker.cs`
- `HubAdvancePortal.cs` / `BuffNotificationOverlay.cs` / `PlayerBuffHud.cs`

---

## 14. 当前已知风险与待验证项

1. **Level5 场景资产**：预置工作台 Play 时删除，但 `.unity` 文件可能仍含旧对象；建议 Editor 重跑 Setup 后保存
2. **地面圆环贴地**：逻辑已改为从高处 Raycast 跳过自身碰撞体，需在第三人称 Play Mode 实测是否仍悬空
3. **文案不一致**：世界 Hint 写「C 随身制作」，实际按键为 `E`
4. **世界标签英文残留**：部分 Legacy 拾取物或旧标签可能仍显示 "Stone" 等；需验证 `ItemWorldLabel` / `UpgradeLegacyLabels`
5. **Level6 空白**：仅占位，无进度/教学/敌人
6. **中文渲染**：未迁移 TMP；依赖 `GameplayChineseText` + OS 字体预烘焙，新增汉字需补充 `CommonChineseCharacters`
7. **场景重复挂载**：多次重建后 Level3~5 可能出现重复 Systems/HUD 对象，需菜单重建校正
8. **图标与模型**：仍为运行时 primitive / 程序 sprite，非最终美术
9. **Unity 6 API 废弃警告**：部分脚本仍有警告，不影响当前运行
10. **无存档**：关卡进度、背包、科技、放置物状态均不持久化

---

## 15. 建议给 AI 的任务方向（下一阶段可选扩展）

### 15.1 玩法向

1. 为 Level6 设计具体目标（例如：熔炼金属锭 → 制作剑 → 击败占位敌人）
2. 工具栏 **使用** 逻辑（石斧砍树、石镐采矿、绷带回血等），而不只是放置
3. 已放置 **储物箱** 交互 UI（Chest inventory）
4. 营火/熔炉 **燃料** 与持续加工队列

### 15.2 关卡向

1. 将 Level4 收集阈值、Level5 放置条件配置化（ScriptableObject / 每关 JSON）
2. Level5.unity **永久** 移除 baked 工作台并保存
3. 添加 Level7+ 或 Hub 关，串联已注册但未使用的配方链（织布、炼金、科技台）
4. 关卡内敌人 / 陷阱 / 限时挑战

### 15.3 体验向

1. 统一 E/C 文案；拾取成功浮字、音效、放置吸附反馈
2. 修复/增强地面圆环与物品高亮在斜坡、台阶上的贴地表现
3. 制造 UI 配方搜索、收藏、材料不足高亮
4. 第一/第三人称准星校准可视化调试开关（已有 `showCrosshairPickupDebug`）

### 15.4 系统向

1. **存档**：关卡解锁、背包、科技、已放置建筑序列化
2. 多人 / 分屏（若需要）需重构 `RequiredPlacedObjectTracker` 与进度单例
3. 迁移 **TextMeshPro** + 中文字体 Asset，替代 `GameplayChineseText` 预烘焙方案
4. 将 `CraftingRecipeDatabase` 迁到 ScriptableObject 或外部表，便于策划迭代

### 15.5 内容向

1. 色块 / 方块 primitive 替换为真实模型与图标 Atlas
2. 生物群系、昼夜、天气与材料刷新点
3. 任务/日志 UI（配合 `SceneProgressionHud` 扩展为通用任务系统）

### 15.6 技术债清理

1. 合并重复的 targeting 距离字段（`PlayerGameplayTargeting.maxPickupDistance` vs `PlayerPickupTargeting.playerPickupReach`）
2. 为 `CrosshairRayUtility` / 三阶段拾取编写 Edit Mode 或 Play Mode 自动化测试
3. CI：Unity batchmode 编译 + 关键场景 smoke load

---

## 16. 一句话摘要（可作为 AI Prompt 开头）

这是一个 Unity 6 的第三/第一人称闯关 + 生存制造原型，已实现 Level1→Level6 场景链路、Level3 四 Buff 教学、Level4 准星拾取与 30% 收集传送门、Level5 随身/工作台制造与放置工作台+熔炉后激活信标进 Level6；核心交互一律基于屏幕准星射线（非鼠标 hover），下一步可在 Level6 落地战斗/熔炼闭环、工具使用、存档与美术替换。

---

*如与场景实际状态不一致，请以 `Assets/Scripts`、当前 Unity Hierarchy 及 git 分支 `work` 最新提交为准。*
