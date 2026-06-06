# Level 8 半随机探索地形生成规划

> **文档类型**：Level Design / Procedural Generation Plan（仅规划，不实现）  
> **版本**：v0.1  
> **日期**：2026-06-02  
> **对齐蓝图**：`AI_Connection_Game_Blueprint.md` v0.2 · Phase 3 · §5.1 Level8  
> **关联代码**（现有基础，本阶段不修改）：  
> `ModularLevelAssembler` · `LevelModuleCatalog` · `GeneratedLevelSummary` · `Level8PlaceholderBuilder` · `GameplayFoundationBootstrap`

---

## 0. 文档目的

本文档为 **Level 8 — 半随机探索关（森林 / 数据荒野）** 的专用规划文件，定义：

- 有界地图（约 **300 × 300 × 100**）的半随机组装方式  
- **6–10 个模块化地形 Chunk** 的连接与填充规则  
- 资源点、危险区、地标、目标物（**Data Core ×3**、**Signal Relay**）的放置策略  
- 与现有 **`ModularLevelAssembler` / 存档种子** 的集成方向  
- 占位模型 → 正式资产的替换路径  
- 分阶段实现顺序、测试清单、风险与开放问题  

**本 pass 只写规划，不实现完整生成系统。**

---

## 1. Level 8 关卡目的

| 维度 | 说明 |
|------|------|
| **蓝图定位** | Phase 3 第一次**正式探索**关；Level7 武器教学完成后进入 |
| **主题** | Forest / Data Wilderness（森林 + 数据荒野混合身份，首版可选单一 Biome） |
| **玩家体验** | 在**有限大小**地图内探索、采集、规避危险、收集目标、修复中继、进入 Level9 矿区 |
| **与 Level4–7 区别** | Level4–7 以手工布局 / 教学为主；Level8 起启用 **种子驱动半随机模块组装** |
| **与无限开放世界区别** | 不做无限地形；每次运行生成**一张有边界的探索图** |

**关卡胜利条件（Blueprint）：**

1. 收集 **3 个 Data Core（数据核心）**  
2. **建造或修复 Signal Relay（信号中继器）**  
3. 激活出口 → **进入 Level 9（矿区）**

**Level8 引入的新系统（规划层）：**

- Tech Tree UI 初版（`TechnologyManager` 已有运行时基础，需 UI + `TechNodeData`）  
- 基础科技解锁  
- 简单存档点 / 资源点占位  

---

## 2. 地图尺寸假设

### 2.1 初始假设（可调整）

| 参数 | 值 | 说明 |
|------|-----|------|
| 玩家参考尺寸 | ≈ 1 × 1 × 1 | CharacterController 量级 |
| 地图宽度 X | **300** | 有界探索，非开放世界 |
| 地图深度 Z | **300** | 同上 |
| 高度预算 Y | **100** | 含丘陵/山地边缘的垂直变化上限 |
| 可玩地面 Y 范围 | 约 0–40（首版） | 各 Biome 再细分 |

### 2.2 尺寸推导

- **Chunk 首版推荐**：**50 × 50**（逻辑格）  
- **逻辑网格**：6 × 6 = 36 格 → 覆盖 300 × 300  
- **实际填充**：每局随机激活 **6–10 个 Chunk 槽位**（非全网格填满）  
- **留白**：未使用格位为 void / 低洼边界 / 不可达区，形成自然地图边缘  

### 2.3 调整触发条件

| 现象 | 调整方向 |
|------|----------|
| 跑图时间过长 | 缩小至 240×240 或减少 Chunk 至 6–8 |
| 资源过稀 | 增加 ResourceChunk 权重或 socket 密度 |
| 目标难找 | 缩小地图或增加 landmark 引导 |
| 性能问题 | 减少 prop 密度、降低 Chunk 数量 |

---

## 3. 设计原则

1. **半随机，非纯噪声**：设计师手工 Chunk + Biome 规则 + 程序化放置  
2. **有界地图**：固定出生点 + 固定/半固定终点区域  
3. **种子可复现**：`worldSeed + levelIndex` → `levelSeed`（与现有 `ModularLevelAssembler` 一致）  
4. **连通性优先**：Spawn → 目标区必须存在可导航路径  
5. **目标公平**：3 个 Data Core 分散但可达；危险区不堵死唯一通路  
6. **模块化可替换**：占位 primitive → Prefab → Blender 模型，数据 ID 不变  
7. **复用现有工厂**：`ItemModuleFactory` · `ToolInteractable` · `WorldPickupItem` · `PlacedObjectBuilder`  

**避免：**

- 纯 Perlin 噪声无限地形  
- 完全随机撒点导致「无意义重复」  
- 与 `LevelModuleCatalog` 并行的第二套 module ID 体系（应扩展而非重写）

---

## 4. Biome / 环境类别

### 4.1 候选 Biome 模板

| ID | 名称 | 首版优先级 | 简述 |
|----|------|------------|------|
| `plain` | 平原 | ★★★ | 平坦、高可见、低危险，适合首次测试 |
| `grassland` | 草地 | ★★★ | 缓丘、分散树、中等资源 |
| `hill` | 丘陵 | ★★☆ |  rolling terrain、更多石头 |
| `mountain_edge` | 山地边缘 | ★★☆ | 高度差大、窄路、落体风险 |
| `swamp` | 沼泽 | ★☆☆ | 低地、水/泥、减速区（后期） |
| `forest` | 森林 | ★★★ | 多树、木/纤维、低能见度 |
| `data_wilderness` | 数据荒野 | ★★★★ | **Level8 蓝图主身份**：自然+数字腐蚀、Data Core、Signal 废墟 |
| `mixed` | 混合生态 | ★☆☆ | 多 Biome 边界（后期） |

### 4.2 首版 Biome 选择策略

**推荐：每局 Level8 只激活 1 个主 Biome。**

```
levelSeed → BiomePicker → BiomeProfile
```

首版默认 Biome：**`data_wilderness`**（与 Blueprint「数据荒野」一致）。  
测试阶段可 Inspector 强制指定 Biome，跳过随机。

### 4.3 BiomeProfile 字段（规划数据结构）

```csharp
// 规划用 — 实现阶段可 ScriptableObject 或 JSON
BiomeProfile {
    string BiomeId;
    string DisplayNameChinese;
    float MinHeight, MaxHeight;           // 地块高度范围
    Color GroundTint;
    float VegetationDensity;              // 0–1
    ResourceDistribution[] Resources;     // ItemKind + weight + min/max per chunk
    DangerZoneKind[] AllowedHazards;
    string[] LandmarkPrefabIds;
    float EnemyDensity;                   // Level8 首版 = 0
    string[] AllowedChunkTypeIds;
    string[] RequiredChunkTypeIds;        // e.g. Spawn, FinalObjective
    string[] OptionalChunkTypeIds;
    LightingMoodPresetId;                 // 后期
}
```

### 4.4 各 Biome 特征摘要

| Biome | 高度变化 | 主资源 | 危险 | 地标风格 |
|-------|----------|--------|------|----------|
| 平原 | 0–3 | 草、纤维、木 | 极少 | 开阔界标 |
| 草地 | 0–5 | 木、纤维、浆果 | 低 | 散落树丛 |
| 丘陵 | 0–15 | 石、矿碎片 | 中 | 山脊线 |
| 山地边缘 | 5–40 | 石、煤、矿 | 高（落体） | 悬崖、窄道 |
| 沼泽 | -1–3 | 藤、纤维、稀有植物 | 中（慢/毒） | 水池、枯木 |
| 森林 | 0–8 | 木、纤维、浆果 | 低–中 | 密林、倒木 |
| 数据荒野 | 0–8 | Data Shard、Data Core 相关 | 中（腐蚀区） | 发光晶体、破损天线、中继废墟 |

---

## 5. Chunk 系统设计

### 5.1 Chunk 数量与尺寸

| 阶段 | Chunk 数 | 尺寸 | 网格 |
|------|----------|------|------|
| 早期测试 | **6** | 50×50 | 6×6 逻辑格，填 6 槽 |
| 标准 | **8** | 50×50 | 同上 |
| 完整 | **10** | 50×50 | 同上 |

备选尺寸（后期评估）：

- 60×60 → 5×5 网格（300 覆盖）  
- 75×75 → 4×4 网格（300 覆盖，Chunk 更大、数量更少）

### 5.2 Chunk 类型（Level8 扩展）

在现有 `LevelModuleType` 基础上，**规划层**增加 Chunk 语义类型：

| Chunk 类型 | 用途 | 首版必需 |
|------------|------|----------|
| `SpawnChunk` | 固定出生点 | ✅ |
| `PathChunk` | 连接通路、低资源 | ✅ |
| `ResourceChunk` | 资源密集 | ✅ ≥1 |
| `HazardChunk` | 危险区 | ✅ 1–3 |
| `LandmarkChunk` | 导航地标 | 推荐 |
| `DataCoreChunk` | 含 1 个 Data Core 槽 | ✅ ×3 |
| `SavePointChunk` | 存档点 | 推荐 ×1 |
| `SignalRelayChunk` | 中继器目标区 | ✅ ×1 |
| `FinalObjectiveChunk` | 终点平台 + 传送门 | ✅ ×1 |

可与现有 Catalog 模块 **一对多映射**（一个 Chunk Prefab 内可 spawn 多个 `LevelModuleDefinition` 实例）。

### 5.3 LevelChunkData（规划结构）

```csharp
LevelChunkData {
    string ChunkId;                       // 稳定 ID，写入 GeneratedLevelSummary
    string BiomeId;
    Vector2Int GridSize;                // 默认 (1,1) = 50×50；可支持 2×2 大 Chunk
    ChunkConnector[] Connectors;          // N/E/S/W 或 8 方向 snap 点
    HeightProfileKind HeightProfile;    // Flat / Rolling / Cliff / SwampBasin
    TransformSocket[] ResourceSockets;
    TransformSocket[] HazardSockets;
    TransformSocket[] LandmarkSockets;
    TransformSocket[] EnemySpawnSockets;  // 后期
    NavMeshBounds NavigationArea;
    bool IsRequired;
    int DifficultyRating;               // 1–5
    string[] Tags;                      // "data_core", "dense_wood", ...
}
```

```csharp
ChunkConnector {
    ConnectorDirection Direction;
    Vector3 LocalPosition;
    int ConnectorWidth;                 // 对齐另一 Chunk 的入口
}
```

### 5.4 与现有 `LevelModuleDefinition` 的关系

| 现有 | Level8 扩展方向 |
|------|-----------------|
| `LevelModuleCatalog` | 继续注册 pickup / interact / exit 模块 |
| `LevelModuleDefinition` | 增加 `BiomeTags[]`, `ChunkId`, `SocketBindings[]` |
| `ModularLevelAssembler` | Level8 专用 `Level8GenerationProfile` 驱动 Chunk 网格布局，再 spawn 模块 |
| `GeneratedLevelSummary` | 扩展：`BiomeId`, `ChunkPlacements[]`, `ObjectivePlacements[]` |

**原则：扩展 `GeneratedLevelSummary`，避免另建平行存档格式。**

---

## 6. Chunk 组装规则

### 6.1 布局策略（首版）

采用 **有向连通图**，而非自由撒 Chunk：

```
[Spawn] → [Path|Resource] → … → [DataCore]* → [SignalRelay] → [FinalObjective/Portal]
                ↓
           [Hazard side branch]（可选支路，非阻断主路）
```

### 6.2 生成流程

1. **初始化**：读取 `worldSeed`，计算 `levelSeed = hash(worldSeed, 8)`  
2. **选 Biome**：`BiomePicker(levelSeed)` → `BiomeProfile`  
3. **固定 Spawn**：`SpawnChunk` 放在网格 `(0, 2)` 或地图西南角固定世界坐标  
4. **固定 Final**：`FinalObjectiveChunk` 放在远离 Spawn 的网格（如 `(5, 5)`），仍在 300×300 内  
5. **主路径**：A* / BFS 在 6×6 网格上找 Spawn → Final 路径，长度 4–6 格  
6. **填充 Chunk 类型**：沿主路径分配 Path / Resource / DataCore；支路分配 Hazard / Landmark  
7. **随机选具体 Prefab**：从该 Biome 的 Chunk Pool 按权重抽取 `ChunkId`  
8. **World 对齐**：网格坐标 × 50 → 世界 XZ；Y 由 HeightProfile 决定  
9. **槽位填充**：在每个 Chunk 内按 Biome 规则 spawn 资源 / 危险 / 目标  
10. **验证**（见 §6.3）  
11. **写入** `GeneratedLevelSummary`；下次进关 **RebuildFromSummary**

### 6.3 验证清单（生成器内置）

| 检查 | 失败处理 |
|------|----------|
| 所有 Required Chunk 已放置 | 重试生成（最多 N 次）或 fallback 固定布局 |
| 3× Data Core 槽位已绑定 | 同上 |
| Signal Relay 槽位存在 | 同上 |
| Spawn ↔ Final NavMesh 连通 | 降低 Hazard 或改路径 |
| 所有 Chunk AABB 在 300×300×100 内 | 缩小偏移或换网格 |
| Chunk 无严重重叠 | Snap 规则 + 碰撞 AABB 检测 |
| 主路径不被 Hazard 完全阻断 | Hazard 仅放支路或留安全通道 |

### 6.4 与 `ModularLevelAssembler` 集成点

当前 Assembler 使用 **线性 offset 放置**（`GetPlacementOffset`）。Level8 需要：

- 新增 `Level8ChunkLayoutBuilder`（规划名）负责网格坐标  
- Assembler 在 `levelIndex >= 8` 时调用 LayoutBuilder，而非简单 index offset  
- 保留 `GeneratedLevelSummary.ModuleIds` 向后兼容，并增加 `ChunkLayout` 字段  

---

## 7. 地形生成策略

### 7.1 首版策略（推荐）

**模块化 Chunk Prefab + 轻量高度变化**，不做复杂 sculpt terrain。

每个 Chunk Prefab 包含：

- 基础地面 Mesh（Plane 或低 poly grid）  
- Biome 材质色（URP Lit 占位色）  
- 可选：Chunk 内 Perlin 高度偏移（±Biome 幅度）  
- 碰撞体（MeshCollider 或 Box 组合）  
- 空 Transform 作为 Resource / Hazard / Landmark **Socket**  

### 7.2 高度范围（Y，单位：米）

| Biome | 局部高度 | 备注 |
|-------|----------|------|
| 平原 | 0–3 | 几乎平坦 |
| 草地 | 0–5 | 缓坡 |
| 丘陵 | 0–15 | rolling |
| 山地边缘 | 5–40 | 需测 CharacterController 可爬性 |
| 沼泽 | -1–3 | 水面/泥面独立 collider |
| 数据荒野 | 0–8 | 腐蚀台地、数据裂隙 |

**全图 Y 保持在 0–100 预算内**；Chunk 间接缝需 **Edge Height Matching**（首版可强制相邻 Chunk 共享边高度）。

### 7.3 首版不做的地形特性

- 实时 LOD 无限地形  
- 复杂 erosion / hydraulic  
- 可破坏地形  
- 洞穴体素  

---

## 8. 资源节点放置

### 8.1 可用资源（对接现有 ItemKind）

| 资源 | 来源系统 | Biome 倾向 |
|------|----------|------------|
| Wood / 木材 | Pickup 或 ToolInteractable | 森林、草地 |
| Stone / 石头 | Pickup / 采矿交互 | 丘陵、山地 |
| Grass / Fiber / 纤维 | Pickup | 平原、草地、沼泽 |
| Vine / 藤 | Pickup | 沼泽 |
| Berry / 浆果 | Pickup | 森林、草地 |
| Ore Fragment / Coal | Pickup / Tool | 山地、数据荒野 |
| Data Shard | Pickup（后期） | 数据荒野 |
| **Data Core** | **目标物**（§10） | 数据荒野 / 专用 Chunk |

### 8.2 放置规则

1. 每个 Chunk Prefab 预置 **ResourceSocket**（空 Transform + `SocketTag`）  
2. `BiomeProfile.ResourceDistribution` 决定每个 Socket 抽什么  
3. 密度：`minPerChunk`–`maxPerChunk`，按 `levelSeed` 随机  
4. **可采集物必须走 gameplay 模块**：  
   - 地面拾取 → `ItemModuleFactory.SpawnWorldPickup`  
   - 可重复采集 → `ToolInteractable` + `ToolReward`  
5. 禁止「看起来能采但不能交互」的装饰物（除非明确标为 Decoration）  

### 8.3 Socket 填充伪代码

```
for each chunk in layout:
  for each socket in chunk.ResourceSockets:
    item = WeightedPick(biome.Resources, rng)
    if item != None:
      ItemModuleFactory.SpawnAt(socket.worldPos, item)
```

---

## 9. 危险区放置

### 9.1 DangerZoneKind（规划枚举）

| Kind | 首版行为 | 视觉占位 |
|------|----------|----------|
| `DamageZone` | `PlayerStats` 持续伤害 | 红色半透明地面圆/方 |
| `SlowZone` | 移速倍率（后期） | 黄色半透明 |
| `BlockedCorruption` | 不可通过 collider | 紫色/黑色 corruption 平面 |
| `HazardWater` | 伤害或减速 | 蓝色平面 |
| `EnemySpawnZone` | 后期启用 | 橙色框 |

### 9.2 DangerZoneData（规划）

```csharp
DangerZoneData {
    DangerZoneKind Kind;
    float DamagePerSecond;
    float SlowMultiplier;
    float Radius;
    Vector3 HalfExtents;              // Box 区域时用
    string[] CompatibleBiomeIds;
    int DifficultyRating;
    GameObject WarningVisualPrefab;     // 占位：红圈
}
```

### 9.3 放置规则

- 每局 **1–3 个**危险区（300×300 地图）  
- **不得阻断** Spawn → Final 的唯一主路径（可放支路、资源区边缘）  
- HazardChunk 上优先使用 HazardSocket  
- 与 Data Core 距离 ≥ `minSafeDistance`（建议 8–12m）  

---

## 10. 地标 / 建筑放置

### 10.1 模块化景观 Prop 类型

| Prop | 类型标签 | 交互 |
|------|----------|------|
| TreeCluster | Decoration / Resource | 可挂 Wood pickup |
| RockCluster | Decoration / Resource | 可挂 Stone |
| FallenLog | Decoration | 无 |
| SwampPool | Hazard | 踩入减速/伤害 |
| HillRidge | Decoration | 无 |
| RuinedWall | Landmark | 无 |
| BrokenAntenna | Landmark | 无 |
| DataCrystal | Landmark / Resource | 后期 Data Shard |
| AbandonedStation | Landmark | 后期 E 交互 |
| SignalRelayRuin | **Objective** | §11 |
| SavePointMarker | **SavePoint** | §13 |
| ResourceDepot | ResourceStation | 展示性资源堆 |

### 10.2 标签体系（生成器 / AI 共用）

```
PropTag: Decoration | ResourceNode | Hazard | Interactable | Objective | SavePoint | Workstation | EnemySpawn
```

每个 Prefab 根节点挂 `LevelPropMetadata`（规划组件）声明 tag，供验证器扫描。

---

## 11. 终点目标区域（Final Objective Area）

### 11.1 要求

- 位置：**固定或半固定**（如始终在网格东北角，或距 Spawn 最远的连通格）  
- 含 **Signal Relay 平台** + **Level9 传送门**（初始锁定）  
- 进入条件：3 Data Core 已收集 + Signal Relay 已修复/建造  

### 11.2 首版占位

```
FinalObjectiveChunk
├── Platform (cube, 灰色, 20×20)
├── SignalRelayRuin (见 §12)
├── PortalBeacon (SceneBeacon, 锁定)
└── Label: "前往第九关 · 矿区"
```

### 11.3 进度逻辑（规划，实现于 Phase 6）

组件：`Level8ProgressionManager` + `Level8TaskProgressTracker`

| 任务 ID | 条件 |
|---------|------|
| `collect_data_core_1/2/3` | 拾取 3 个 Data Core |
| `repair_signal_relay` | 交互完成修复（或放置建造） |
| `unlock_level9_portal` | 上述完成后激活 |

---

## 12. Data Core 放置逻辑

### 12.1 规则

- **恰好 3 个**必需 Data Core  
- 分属 **不同 Chunk**（理想情况）  
- 距 Spawn **≥ 40m**（可调）  
- 不得落在不可达 Hazard 内  
- 建议分布：**资源区 1 + 地标区 1 + 支路/轻度危险区 1**  

### 12.2 对象规格

| 属性 | 值 |
|------|-----|
| 显示名 | 数据核心 |
| 类型 | `WorldPickupItem` 或 `ToolInteractable`（二选一，见开放问题） |
| ItemKind | 新增 `DataCore`（规划）或任务专用 pickup |
| 视觉 | 发光球体 / 立方体（青蓝色） |
| 追踪 | `Level8TaskProgressTracker.RegisterTaskComplete` |

### 12.3 生成器步骤

1. 从主路径+支路候选 Chunk 中抽 3 个不同 Chunk  
2. 各 Chunk 内选 **DataCoreSocket**（无则 fallback 到 LandmarkSocket）  
3. 写入 `GeneratedLevelSummary.ObjectivePlacements[]`  
4. Rebuild 时在相同 socket 位置 spawn  

---

## 13. Signal Relay 目标逻辑

### 13.1 Blueprint 要求

- **建造或修复** Signal Relay  
- 完成后允许进入 Level9  

### 13.2 首版推荐：修复交互（复用 Level6 模式）

复用现有 **`ToolInteractable`** + 任务 ID 模式（Level6 已修复 Signal Relay）：

| 字段 | 值 |
|------|-----|
| 显示名 | 信号中继器 |
| 交互 | E / 工具或空手修复（首版可免材料） |
| 任务 | `repair_signal_relay_level8` |
| 前置 | 3 Data Core 已收集 |
| 完成 | 播放占位动画/日志 → 解锁传送门 |

**备选（后期）**：用 `PlacedObjectBuilder` 放置 Signal Relay 建筑；首版优先 **固定场景交互物**，降低系统耦合。

### 13.3 位置

- 位于 `SignalRelayChunk` 或 `FinalObjectiveChunk` 中心  
- 与 Level9 Portal 相距 5–10m，玩家可清晰看到流程  

---

## 14. 科技树集成（规划）

### 14.1 现有基础

- `TechnologyManager` · `TechnologyKind` 已存在于 `_Project`  
- `CraftingUnlockEvaluator` 已可检查科技解锁  
- **缺**：Tech Tree UI · 结构化 `TechNodeData` · Level8 内解锁触发点  

### 14.2 Level8 科技树范围（首版 4–6 节点）

| 节点 ID | 中文名 | 解锁触发 | 效果 |
|---------|--------|----------|------|
| `basic_gathering_2` | 基础采集 II | 采集 N 个资源 | 提高采集效率（占位） |
| `signal_repair` | 信号修复 | 修复 Signal Relay | 允许 Level9 传送 / 配方 |
| `resource_scanner` | 资源扫描 | 收集 1 Data Core | HUD 显示附近资源（后期） |
| `basic_storage` | 简易存储 | 到达 SavePoint | 解锁储物配方 |
| `weapon_handling_2` | 武器熟练 II | 击败首个敌人（Level9） | 占位 |
| `map_marker` | 地图标记 | 完成 Level8 | 占位 |

### 14.3 与地形生成分离

- **TechTreeManager** 不参与 Chunk 布局  
- Level8 场景内放置 **ResearchTerminal** 占位（SavePointChunk 或 Final 区）  
- Data Core 收集可奖励 **科技点** 或直接解锁节点（设计待定）  

---

## 15. 存档点 / 资源点

### 15.1 SavePoint（存档点）

| 属性 | 首版 |
|------|------|
| 数量 | 1（主路径中段 LandmarkChunk） |
| 交互 | E |
| 效果 | 日志「存档点已激活」+ 更新 runtime checkpoint |
| 磁盘存档 | 复用 `PlayerProgressionState` / `SaveManager`（Phase 7 实现） |
| 占位 | 蓝色柱体 + 标签「存档点」 |

### 15.2 ResourceStation（资源点）

- 可选 Landmark：展示性资源堆 + 标签「资源点」  
- 后期：一次性补给或无限采集点  

### 15.3 ResearchTerminal（研究终端）

- 与 Tech Tree UI 绑定  
- 首版：E 打开占位 UI「科技树（开发中）」  

---

## 16. 占位模型计划

### 16.1 形状与颜色规范

| 对象 | 占位形状 | 颜色 | 中文标签 |
|------|----------|------|----------|
| 地形 Chunk | Plane / 低 poly mesh | Biome GroundTint | — |
| 树 | 圆柱 + 球 | 绿 | — |
| 岩石 | 缩放 Cube/Sphere | 灰 | — |
| 沼泽水面 | 扁 Plane | 蓝绿半透明 | — |
| Data Core | Sphere | 青蓝 + 自发光 | 数据核心 |
| Signal Relay | 柱 + 横杆 Cube | 灰蓝 | 信号中继器 |
| 危险区 | 扁 Cylinder/Plane | 红半透明 | 危险区域 |
| 存档点 | Cube 柱 | 蓝 | 存档点 |
| 资源点 | 小 Cube 堆 | 黄 | 资源点 |
| 废墟墙 | Cube 组合 | 灰 | — |
| 传送门 | 现有 `SceneBeacon` | 紫 | 前往第九关 |

### 16.2 标签

统一使用 `ItemWorldLabel` 或 `BillboardLabel` 显示中文，便于 Play Mode 测试识别。

### 16.3 Prefab 路径（规划）

```
Assets/_Project/Prefabs/Level8/
├── Chunks/
│   ├── L8_Spawn_DataWilderness.prefab
│   ├── L8_Path_DataWilderness.prefab
│   └── ...
├── Props/
│   ├── L8_Prop_TreeCluster.prefab
│   └── ...
└── Objectives/
    ├── L8_DataCore.prefab
    └── L8_SignalRelay.prefab
```

---

## 17. 未来模型 / 渲染需求

| 类别 | 未来资产 | 优先级 |
|------|----------|--------|
| 地面 | Biome 地表纹理、混合 shader | P1 |
| 植被 | 树、灌木、草 billboards | P1 |
| 岩石 | 模块化 rock kit | P2 |
| 水体 | 沼泽 shader、浅水 | P2 |
| 地形 mesh | 丘陵/悬崖专用 Chunk mesh | P2 |
| 科技建筑 | Signal Relay、破损天线、数据门 | P1 |
| Data Core | 发光核心模型 + VFX | P1 |
| 危险区 | 粒子/边缘 glow | P3 |
| 存档/研究站 | 终端模型 | P3 |
| 光照 | Biome 后处理、雾、定向光配色 | P2 |
| UI | Tech Tree 面板美术 | P2 |

**原则：** 占位 Prefab 的 **根 Transform / Socket 命名 / ChunkId** 保持不变，仅替换子 Mesh/Material。

---

## 18. 实现阶段（Implementation Phases）

### Phase 1 — 文档与数据设计 ✅（本文件）

- [x] Level8 生成规划文档  
- [ ] 评审 BiomeProfile / LevelChunkData 字段  
- [ ] 扩展 `GeneratedLevelSummary` 设计评审  

### Phase 2 — 基础 Chunk 原型

- 手工搭建 3–5 个 Chunk Prefab（Spawn / Path / Resource / Hazard / Final）  
- **固定布局**测试：不随机，验证可玩性  
- Editor 菜单：`LevelSetupMenu.CreateLevel8Scene` 已有占位  

### Phase 3 — 随机 Chunk 组装

- `Level8ChunkLayoutBuilder`：6×6 网格、连通路径  
- 随机选 6–10 Chunk  
- 边界 300×300 校验  

### Phase 4 — Biome 选择

- 实现 `BiomeProfile` 数据（ScriptableObject）  
- 首版仅 `data_wilderness` 一套 Pool  
- 按 Biome 切换地面色 / prop 权重  

### Phase 5 — 资源 / 危险 / 目标放置

- Socket 驱动 spawn  
- 3× Data Core + 1× Signal Relay + 1–3 Hazard  
- 对接 `ItemModuleFactory`  

### Phase 6 — Level8 目标逻辑

- `Level8TaskProgressTracker` / `Level8ProgressionManager` / HUD  
- 收集核心 → 修复中继 → 激活 Level9 Portal  

### Phase 7 — 科技树 / 存档点原型

- Tech Tree 占位 UI  
- SavePoint 交互 + runtime checkpoint  
- 对接 `TechnologyManager`  

### Phase 8 — 打磨与资产替换

- 更多 Chunk 变体（8–10 模块）  
- 第二 Biome（forest / grassland）  
- NavMesh 烘焙 / 连通性自动测试  
- 敌人 spawn（可推迟到 Level9）  
- 美术替换占位模型  

---

## 19. 测试清单

### 19.1 生成测试

- [ ] 同 `worldSeed` 两次进入 Level8，布局一致  
- [ ] 不同 seed 布局不同  
- [ ] Chunk 数量在 6–10 范围内  
- [ ] 无 Chunk 超出 300×300 边界  
- [ ] 无 Chunk 严重重叠  

### 19.2 可达性测试

- [ ] Spawn 可走到 Final Objective  
- [ ] 3 个 Data Core 均可到达  
- [ ] Signal Relay 可交互  
- [ ] Hazard 不阻断唯一主路  

### 19.3 Gameplay 测试

- [ ] 资源可拾取/可采集  
- [ ] 收集 3 Core 后任务更新  
- [ ] 修复 Relay 后门开  
- [ ] Y 键进入 Level9 场景  

### 19.4 性能测试

- [ ] 300×300 地图 FPS 可接受（目标 ≥ 60 on dev PC）  
- [ ] Prop 数量在预算内（建议首版 < 500 colliders）  

### 19.5 回归测试

- [ ] Level4–7 手工布局未被模块化逻辑影响  
- [ ] `ModularLevelAssembler` levelIndex < 8 仍 early return  

---

## 20. 风险与开放问题

### 20.1 风险

| 风险 | 影响 | 缓解 |
|------|------|------|
| 随机布局导致目标不可达 | 卡关 | 连通性验证 + 重试 + fallback 固定图 |
| 地形高度差过大 | 移动/相机异常 | Biome 高度上限 + 斜坡角度限制 |
| Chunk 接缝可见 |  immersion 差 | 共享边高度 / 过渡 prop |
| 资源分布不均 | 体验差 | Biome 权重 + minPerChunk |
| Hazard 堵路 | 软锁 | 主路径禁放 Hazard |
| Prop 过多 | 性能下降 | 每 Chunk 上限 |
| 纯随机无手工质量 | 地图无意义 | 设计师 authored Chunk pool |

### 20.2 开放问题

| # | 问题 | 首版建议 |
|---|------|----------|
| 1 | 每局单 Biome 还是混合？ | **单 Biome**（data_wilderness） |
| 2 | 是否需要小地图？ | **否**；用地标 + 目标 HUD |
| 3 | Data Core 是 pickup 还是 interactable？ | **WorldPickupItem** + 任务计数 |
| 4 | Signal Relay 放置还是修复？ | **固定点 ToolInteractable 修复** |
| 5 | 生成地图是否存 seed？ | **是**，已有 `GeneratedLevelSummary.LevelSeed` |
| 6 | Level8 是否出敌人？ | **首版无**；战斗在 Level9 |
| 7 | 300×300 用多少 Chunk？ | **6 测试 → 8 标准 → 10 完整** |
| 8 | Chunk 50 / 60 / 75？ | **首版 50×50** |
| 9 | 扩展 Summary 还是新 Save DTO？ | **扩展 GeneratedLevelSummary** |
| 10 | NavMesh 动态烘焙还是 CharacterController 纯物理？ | 首版 **平面 + 台阶限制**；Phase 8 评估 NavMesh |

---

## 21. 与现有文档 / 代码索引

| 文档 / 代码 | 关系 |
|-------------|------|
| `AI_Connection_Game_Blueprint.md` §5.1 | 蓝图需求来源 |
| `LEVEL_MODULE_WORKFLOW.md` | 模块化存档 workflow |
| `ModularLevelAssembler.cs` | Level8+ 生成入口（需扩展布局） |
| `LevelModuleCatalog.cs` | 模块 ID 注册 |
| `Level8PlaceholderBuilder.cs` | 当前空场景占位 |
| `LevelSetupMenu.CreateLevel8Scene` | Editor 创建场景 |
| `TechnologyManager.cs` | 科技解锁运行时 |
| `Level6 SignalRelay` | 修复交互参考实现 |

---

## 22. 一句话摘要

Level8 是一张 **300×300×100 有界地图**，由 **6–10 个 50×50 手工 Chunk** 经 **种子驱动连通布局** 拼成，在 **单一 Biome（首版：数据荒野）** 规则下填充资源、危险与地标，固定收集 **3 个数据核心**、修复 **信号中继器** 后进入 Level9；地形与 prop 先用 **占位几何体**，通过 **Socket + Catalog + GeneratedLevelSummary** 与现有模块化架构对接，分 **8 个 Phase** 逐步实现。

---

*本文档仅规划。实现时请同步更新 `LEVEL_MODULE_WORKFLOW.md` 与 `PROJECT_PROCESS.md`。*
