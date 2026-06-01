# AI Connection 项目总览（用于 AI 游戏大纲输入）

> 本文档是当前项目的**最新玩法与实现基线**。可直接提供给 AI，用于产出：游戏大纲、功能拆解、技术方案、任务排期。

---

## 1. 项目定位

- **项目名**：AI connection
- **引擎版本**：Unity 6 (`6000.4.7f1`)
- **渲染管线**：URP
- **当前类型**：第三/第一人称切换的关卡闯关原型
- **核心体验**：移动与平台挑战 + Buff 教学 + 物品拾取/背包系统

---

## 2. 当前可玩主流程（关卡链路）

### 2.1 关卡顺序

1. `SampleScene`（Level1）
2. `Level2`
3. `Level3`（Buff 教学 Hub）
4. `Level4`（地形 + 拾取 + 背包）

### 2.2 通关逻辑

- **Level1**：分数达标后，按 `Y` 进入 Level2
- **Level2**：到达终点后，按 `Y` 进入 Level3
- **Level3**：四路 Buff 全收集后弹出进入下一关提示，按 `Y` 进入 Level4；场景中也会生成传送门
- **Level4**：当前为自由拾取与背包验证关（尚无最终胜利条件）

---

## 3. 操作说明（玩家输入）

### 3.1 角色移动与视角

- `W A S D`：移动
- `Space`：跳跃（消耗精力）
- `Shift`：冲刺（持续消耗精力）
- `V`：第一/第三人称切换
- 鼠标：视角控制

### 3.2 关卡与交互

- `Y`：确认进入下一关（关卡提示出现时）
- `R`：在 Level3 的返回区回到 Hub 原点
- `F`（并兼容 `E`）：拾取面前/近处物品（Level4）
- `B`：打开/关闭背包（Level4）
- `1~9`：切换工具栏选中槽位（Level4）

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

## 4.4 Level4（拾取与背包）

- 56x56 的占位地形（路径、丘陵、岩石、水池）
- 散布多种可拾取色块（`WorldPickupItem`）
- 重点验证：F 拾取、物品栏与背包、拖拽交换、堆叠

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

- `BuffNotificationOverlay`：顶部信息条（获得 Buff、关键提示）
- `LevelTransitionOverlay`：关卡切换中央信息

---

## 7. Level4 拾取与背包系统（Minecraft 风格）

## 7.1 物品定义

- `ItemKind`：多种颜色方块
- 每类可堆叠，默认最大堆叠为 `64`

## 7.2 拾取规则

- `PlayerPickupInteractor` 负责寻找目标并响应 `F`/`E`
- 拾取成功后，地面物体会被销毁（消失）

## 7.3 入包优先级（已实现）

按以下顺序填充：

1. 工具栏同类堆叠
2. 背包同类堆叠
3. 工具栏空位
4. 背包空位

## 7.4 背包交互（已实现）

- 工具栏 9 格（屏幕底部）
- 背包 27 格（3x9）
- 打开背包后可鼠标拖拽：
  - 空格：移动
  - 同类：合并堆叠
  - 异类：交换

## 7.5 图标显示（已实现）

- 占用槽位显示物品图标（运行时生成“方块截面风格” sprite）
- 拖拽时也显示同一图标与数量

---

## 8. Level3 -> Level4 过渡系统

核心对象：

- `BuffCollectionTracker`：统计四种 Buff 收集情况
- `HubAdvancePortal`：集齐后生成传送门，并触发进入下一关提示
- `LevelManager`：统一处理 `Y` 键确认后的场景加载

兜底策略：

- 集齐 4 Buff 时，直接调用 `LevelManager.NotifyGoalReached()`
- 传送门出现时也会触发一次 `NotifyGoalReached()`

---

## 9. 编辑器菜单与自动化入口

位于 `Tools` 菜单：

- `Setup Level 1 (Score Win)`
- `Setup Level 2 (Parkour -> Level 3)`
- `Create Level 2 Scene (Parkour)`
- `Create Level 3 Scene (Buff Hub)`
- `Setup Level 3 (Buff Hub -> Level 4)`
- `Create Level 4 Scene`

推荐重建流程（当场景配置错乱时）：

1. `Create Level 3 Scene`
2. `Setup Level 3`
3. `Create Level 4 Scene`

---

## 10. 关键脚本地图（给 AI 用）

## 10.1 关卡与流程

- `LevelManager.cs`：关卡过渡
- `LevelGoal.cs`：到点触发过关
- `Level3SceneAutoSetup.cs`：Level3 运行时自动挂载
- `Level4SceneAutoSetup.cs`：Level4 运行时 HUD/背包保障
- `Level3BuffHubBuilder.cs`：第三关地形与 Buff 路径
- `Level4PlaceholderBuilder.cs`：第四关地形与拾取物散布

## 10.2 玩家与战斗资源

- `PlayerController.cs`
- `PlayerStats.cs`
- `PlayerBuffController.cs`
- `PlayerShieldVisual.cs`

## 10.3 Buff 与 UI

- `BuffType.cs`
- `BuffBubble.cs`
- `BuffCollectionTracker.cs`
- `HubAdvancePortal.cs`
- `BuffNotificationOverlay.cs`
- `PlayerBuffHud.cs`
- `ActiveBuffDisplay.cs`

## 10.4 拾取与背包

- `WorldPickupItem.cs`
- `PlayerPickupInteractor.cs`
- `PlayerInventory.cs`
- `InventorySlotData.cs`
- `ItemKind.cs`
- `PlayerInventoryHud.cs`

---

## 11. 当前已知风险（喂 AI 时建议注明）

1. Level3/Level4 多次重建后，场景内旧对象与自动挂载对象可能重复，需要菜单重建一次校正
2. 图标为运行时生成 sprite（非美术资源），适合原型，不适合最终表现
3. Level4 目前无完整“使用物品”机制，仅实现拾取、存储、显示、拖拽
4. 一些脚本仍有 Unity 6 API 废弃警告（不影响运行）

---

## 12. 建议给 AI 的任务方向（用于下一阶段）

1. **玩法向**：为 Level4 加入可消耗道具与快捷栏使用逻辑（按数字键使用）
2. **关卡向**：补齐 Level4 的目标、敌人/机关、结算和进入 Level5
3. **体验向**：拾取成功浮字、音效、背包开关动画、拖拽吸附反馈
4. **系统向**：存档（关卡进度、背包、玩家状态）
5. **内容向**：将色块替换为真实物品资产与图标 Atlas

---

## 13. 一句话摘要（可作为 AI Prompt 开头）

这是一个 Unity 6 的第三/第一人称闯关原型游戏，已实现 Level1→Level4 流程、Level3 四 Buff 教学与过渡、Level4 的 F 拾取 + 工具栏优先入包 + 背包拖拽交换与堆叠系统，下一步需要在现有系统上扩展完整玩法闭环与内容表现。

---

*如与场景实际状态不一致，请以 `Assets/Scripts` 和当前 Unity Hierarchy 为准。*
