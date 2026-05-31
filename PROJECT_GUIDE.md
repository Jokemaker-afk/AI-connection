# AI Connection 项目学习文档

> 本文档记录本 Unity 项目在 Cursor + MCP 协作下的搭建过程、技术选型、场景结构、脚本模块与关卡生成逻辑，便于理解与后续扩展。

---

## 1. 项目概览

| 项目 | 说明 |
|------|------|
| **名称** | AI connection |
| **引擎** | Unity 6（6000.4.7f1） |
| **渲染** | URP（Universal Render Pipeline） |
| **主场景** | `Assets/Scenes/SampleScene.unity` |
| **玩法原型** | 室内多层建筑中第三人称/第一人称移动、跳跃、冲刺、精力系统 |
| **协作方式** | Cursor IDE ↔ MCP for Unity ↔ Unity Editor 实时控制 |

---

## 2. 技术栈与外部工具

### 2.1 Unity 核心包

| 包名 | 作用 |
|------|------|
| `com.unity.render-pipelines.universal` | URP 渲染管线 |
| `com.unity.inputsystem` | **新 Input System**（WASD、Space、Shift、V、鼠标） |
| `com.unity.ugui` | UI（HP/SP 滑动条） |
| `com.unity.probuilder` | 可选：Editor 内高级几何建模（Deps 中可安装） |
| `com.coplaydev.unity-mcp` | **MCP for Unity**：让 Cursor 通过协议控制 Editor |

### 2.2 Cursor 侧 MCP 配置（一次性）

配置文件：`C:\Users\<你的用户名>\.cursor\mcp.json`

```json
{
  "mcpServers": {
    "unityMCP": {
      "url": "http://127.0.0.1:8080/mcp",
      "type": "http"
    }
  }
}
```

也可在 Unity 中自动写入：**MCP For Unity → Connect → Client 选 Cursor → Configure**。

首次使用或换电脑时还需确认：

- 项目 `Packages/manifest.json` 含 `com.coplaydev.unity-mcp`
- Unity **Deps / Setup** 中 Python、uv 已安装（**Start Server** 失败时按提示安装）

**日常每次开发时的完整步骤见 [2.4 启动流程](#24-启动流程每次使用)** 与 **[2.5 关闭流程](#25-关闭流程)**。**

---

### 2.3 在 Cursor 中曾完成的典型操作

| 操作类型 | MCP 工具 / 方式 | 说明 |
|----------|-----------------|------|
| 创建物体 | `manage_gameobject` | Cube、Player、建筑块等 |
| 批量搭建 | `batch_execute` | 一次创建多个墙体/地板 |
| 挂组件 | `manage_components` | CharacterController、脚本等 |
| 写脚本 | 直接编辑 `Assets/Scripts/*.cs` + `refresh_unity` | Cursor 改文件，Unity 编译 |
| 执行 Editor 逻辑 | `execute_code` | 运行时加组件、改材质、生成关卡 |
| 保存场景 | `manage_scene` (save) | 持久化 Hierarchy |
| 查场景 | `manage_scene` (get_hierarchy) | 确认结构 |

---

### 2.4 启动流程（每次使用）

按顺序执行，可避免 Cursor 显示 `unityMCP` 报错或 AI 无法操作场景。

```
① 打开 Unity Hub → 打开本项目「AI connection」
        ↓
② Unity：Window → MCP For Unity（Ctrl+Shift+M）
        ↓
③ Connect：Transport = HTTP Local，URL = http://127.0.0.1:8080
        ↓
④ 点击 Start Server（保持弹出的黑色终端窗口运行，可最小化，不要关）
        ↓
⑤ 点击 Start Session → 显示绿点 Session Active (AI connection)
        ↓
⑥（若 Cursor 未配置过）Client 选 Cursor → Configure → Configured
        ↓
⑦ 完全退出 Cursor（含系统托盘）→ 重新打开本项目文件夹
        ↓
⑧ Cursor：Settings → MCP → 确认 unityMCP 为绿色 Connected
        ↓
⑨ 新建 Chat，开始让 AI 操作 Unity（MCP 弹窗选 Allow / Run）
```

#### Unity 侧操作表

| 步骤 | 位置 | 操作 | 成功标志 |
|------|------|------|----------|
| 1 | Unity | 打开项目 | 场景、Hierarchy 正常 |
| 2 | `Ctrl+Shift+M` | 打开 MCP 窗口 | 出现 Connect 页 |
| 3 | Connect → Server | **Start Server** | 终端显示 `Uvicorn running on http://127.0.0.1:8080` |
| 4 | Connect → Session | **Start Session** | 绿点 **Session Active** |
| 5 | Connect → Client | Cursor → **Configure**（首次） | **Configured** |
| 6 | Cursor | 完全重启后打开项目 | — |
| 7 | Cursor Settings → MCP | 查看 unityMCP | **Connected**（有工具数量） |

#### Cursor 侧操作表

| 步骤 | 操作 | 说明 |
|------|------|------|
| 1 | 用 Cursor 打开仓库根目录 `AI connection` | 确保读到 `.cursor/mcp.json` |
| 2 | 完全退出再启动 Cursor | 修改 mcp 配置后必须重启 |
| 3 | Settings → MCP | 确认 **unityMCP** 已连接 |
| 4 | 新建对话 | 旧对话可能在 MCP 未连上时创建，工具列表不全 |
| 5 | 允许 MCP 工具执行 | 勿点 Skip，否则 AI 无法改场景 |

#### 连接结构（便于理解）

```
Cursor（AI 对话）
    ↓  HTTP   http://127.0.0.1:8080/mcp
MCP Server（本机终端进程，端口 8080）
    ↓  Unity Bridge
Unity Editor（须 Session Active）
    ↓
场景 / 脚本 / 组件 / 编译 等
```

#### 启动前 30 秒检查清单

- [ ] Unity 已打开 **AI connection** 项目（非仅 Hub）
- [ ] MCP 终端窗口仍在运行（Start Server 未关）
- [ ] Unity 显示 **Session Active**
- [ ] Cursor 中 **unityMCP = Connected**
- [ ] 需要 AI 改场景时使用**新对话**（推荐）

#### 仅改脚本、不连 Editor 时

若只在 Cursor 里编辑 `Assets/Scripts/*.cs`，**可以不启动 MCP**；保存后 Unity 会自动编译。  
需要 **创建物体、改 Hierarchy、挂组件、保存场景** 时，必须完成本节启动流程。

---

### 2.5 关闭流程

结束开发时建议按顺序断开，避免端口占用、Session 残留或下次启动异常。

```
① Cursor：结束当前 Chat（可选）→ 完全退出 Cursor
        ↓
② Unity：若正在 Play → 点击 Stop
        ↓
③ Unity MCP 窗口：Session 为 Active 时 → 点击 End Session
        ↓
④ Unity MCP 窗口：Server 在运行时 → 点击 Stop Server
        ↓
⑤ 关闭 MCP 弹出的黑色终端窗口（若未随 Stop Server 自动关闭）
        ↓
⑥ Unity：File → Save / Save Project（有改动时）
        ↓
⑦ 关闭 Unity Editor（可选，视你是否继续其他项目）
```

#### 关闭操作表

| 顺序 | 位置 | 操作 | 目的 |
|------|------|------|------|
| 1 | Cursor | 退出 Cursor | 释放 MCP 客户端连接 |
| 2 | Unity | **Stop** Play 模式 | 避免 Editor 在运行中关 Bridge |
| 3 | MCP → Session | **End Session** | 断开 Unity Bridge |
| 4 | MCP → Server | **Stop Server** | 结束 8080 端口 HTTP 服务 |
| 5 | 终端 | 关闭 MCP Server 窗口 | 确保进程结束 |
| 6 | Unity | **Save** | 保存场景与项目 |
| 7 | Unity | 关闭项目 / Editor | 完全退出（可选） |

#### 临时离开（不关 Unity）

若只暂停几分钟、Unity 保持打开：

- 可只 **End Session**，Cursor 关掉即可  
- 下次用 Cursor 前：再点 **Start Session**（Server 仍可保持运行）  

若 **Stop Server** 或关了终端，下次必须重新 **Start Server → Start Session**，并**重启 Cursor**。

#### 关闭后如何确认已干净

| 检查项 | 正常状态 |
|--------|----------|
| 任务管理器无残留 `uv` / `python` MCP 进程（可选查） | 无占 8080 的僵尸进程 |
| 下次 **Start Server** 能正常监听 8080 | 无「端口被占用」 |
| Cursor 下次打开后 unityMCP 可再次 **Connected** | 配置仍有效 |

---

### 2.6 连接问题速查

| 现象 | 处理 |
|------|------|
| Cursor 只有 wildfire-mcp，没有 unityMCP | 检查 `mcp.json` 是否含 `unityMCP`；重启 Cursor |
| unityMCP 红色 Error | Unity：**Start Server** + **Start Session** → 重启 Cursor |
| Start Server 失败 | 安装 Python/uv；看 Unity Console 的 `MCP-FOR-UNITY` 日志 |
| AI 说连不上 Unity | 确认 Session Active；勿长期停在 Play 模式调 MCP |
| MCP 工具被 Skip | 对话中允许 Run；换新 Chat |
| 关终端后 Cursor 断了 | 重新 Start Server，必要时重启 Cursor |
| 重启电脑后 | 从 [2.4 启动流程](#24-启动流程每次使用) 完整做一遍 |

---

## 3. 场景 Hierarchy 与脚本对应关系

当前 `SampleScene` 根级物体：

```
SampleScene
├── Main Camera          ← 相机 + 视角控制
├── Directional Light    ← 定向光（无自定义脚本）
├── Global Volume        ← URP 后处理体积（无自定义脚本）
├── Player               ← 玩家控制 + 数值
├── GameplayHUD          ← 屏幕 UI
└── Building_Large       ← 程序化生成的建筑（无脚本，纯几何体）
```

---

### 3.1 Main Camera

| 组件 | 类型 | 说明 |
|------|------|------|
| Transform | 内置 | 位置由相机脚本驱动 |
| Camera | 内置 | 主摄像机 |
| AudioListener | 内置 | 音频监听 |
| UniversalAdditionalCameraData | URP | URP 相机扩展数据 |
| **PlayerCameraController** | **自定义** | 见下文 |

**对应代码：** `Assets/Scripts/PlayerCameraController.cs`

**职责：**

- **第三人称**：绕玩家轨道旋转（鼠标控制 yaw/pitch），相机在后方跟随
- **第一人称**：相机贴在眼睛偏移位置，鼠标控制视角，隐藏玩家网格
- **V 键**：在两种模式间切换
- 第一人称时锁定鼠标光标

**与 Player 的关系：** `PlayerController` 读取相机朝向计算移动方向；第一人称时角色朝向与相机 yaw 同步。

---

### 3.2 Player

| 组件 | 类型 | 说明 |
|------|------|------|
| Transform | 内置 | 世界位置/旋转 |
| MeshFilter + MeshRenderer | 内置 | 胶囊体外观（蓝色材质） |
| CharacterController | 内置 | 碰撞与移动（非 Rigidbody） |
| **PlayerController** | **自定义** | 移动、跳跃、冲刺 |
| **PlayerStats** | **自定义** | 生命、精力数据 |

**对应代码：**

| 脚本 | 路径 |
|------|------|
| PlayerController | `Assets/Scripts/PlayerController.cs` |
| PlayerStats | `Assets/Scripts/PlayerStats.cs` |

#### PlayerController — 输入与移动

- 使用 **Unity Input System**（`Keyboard.current`），不用旧版 `Input` 类
- **WASD**：相对相机水平方向的移动
- **Shift**：按住且移动时冲刺（速度 × `sprintSpeedMultiplier`），并消耗精力
- **Space**：跳跃（Coyote Time + Jump Buffer，提高手感）
- 地面检测：`CharacterController.isGrounded` + 向下 `SphereCast` 双保险
- 依赖同物体上的 `PlayerStats` 判断能否跳/能否冲刺

#### PlayerStats — 数值模块（数据与规则分离）

- **生命值**：`maxHealth` / `currentHealth`（当前仅存储与 UI 显示，受伤逻辑未实现）
- **精力**：
  - 按住 Shift → `DrainSprint` 线性减少
  - 跳跃成功 → `TryConsumeJumpStamina` 固定扣除
  - 松开 Shift/Space 超过 `regenDelay`（默认 0.5s）→ 线性回复
- 所有阈值可在 Inspector 的 **Player Stats** 中调整

---

### 3.3 GameplayHUD

| 组件 | 类型 | 说明 |
|------|------|------|
| Transform | 内置 | 根节点 |
| **PlayerHUD** | **自定义** | UI 逻辑 |
| Canvas / CanvasScaler / GraphicRaycaster | 运行时或预制 | 屏幕空间 UI（若 `buildUiIfMissing` 为 true 会自动创建） |

**对应代码：** `Assets/Scripts/PlayerHUD.cs`

**职责：**

- 每帧读取 `PlayerStats` 的 `HealthNormalized`、`StaminaNormalized`
- 更新左上角 **HP**（红）、**SP**（黄）滑动条
- **不创建 EventSystem / StandaloneInputModule**（避免与新 Input System 冲突导致 Console 刷屏报错）

**注意：** `PlayerStats` 在 **Player** 上，`PlayerHUD` 通过 `FindFirstObjectByType<PlayerStats>()` 或 Inspector 引用获取。

---

### 3.4 Building_Large（图形 / 建模区块）

**无挂载脚本。** 全部由 `MultiFloorLevelBuilder.Generate()` 在 Editor 中生成的静态 Cube 组成，带 `BoxCollider`，`isStatic = true`。

#### 分区结构（Hierarchy 子节点）

```
Building_Large/
├── Floors/          【地板层】每层楼板 + 楼梯井开洞
├── Walls/           【墙体层】外墙 + 各层内隔墙
├── Stairs/          【楼梯层】折返楼梯 + 平台
└── Props/           【装饰层】桌椅、柱子等占位物体
```

#### 各分区作用

| 分区 | 作用 | 主要内容 |
|------|------|----------|
| **Floors** | 承载玩家行走的水平面 | `Floor_1` ~ `Floor_3`：在楼梯位置挖空的复合楼板 |
| **Walls** | 限制空间、划分房间 | `Walls_F*` 四面外墙；`Interior_F*` 各层内部分隔墙 |
| **Stairs** | 连接楼层 | `Stair_F1_F2_Up`（南→北上楼）、`Landing_F2`、`Stair_F2_F3_Down`（北→南上楼）、`Landing_F3` |
| **Props** | 视觉参考与障碍 | 桌子、沙发、立柱等简单立方体 |

#### 楼梯设计（折返式）

- **层高**：`FloorHeight = 6m`（可在生成器顶部常量修改）
- **1→2 层**：从楼梯井**南侧**向北拾级而上
- **2 层平台**：在**北侧**转弯
- **2→3 层**：从**北侧**向南拾级而上（与下段错开，避免「上楼被上层楼梯底板卡头」）
- **3 层出口**：**南侧**平台

#### 建筑整体尺寸（当前常量）

- 占地约 **40m × 32m**
- 3 层，总高度约 **18m**

**对应代码：**

| 文件 | 说明 |
|------|------|
| `Assets/Scripts/MultiFloorLevelBuilder.cs` | 静态类，核心生成逻辑 |
| `Assets/Scripts/Editor/MultiFloorLevelBuilderMenu.cs` | 菜单 `Tools → Generate Multi-Floor Level` |

---

### 3.5 无自定义脚本的物体

| 物体 | 作用 |
|------|------|
| **Directional Light** | 场景主光源 |
| **Global Volume** | URP 后处理（SampleScene Profile） |
| **TutorialInfo**（若未删除） | Unity 模板自带说明，与玩法无关 |

---

## 4. 模块化代码架构

```
┌─────────────────────────────────────────────────────────┐
│                    输入层 (Input System)                 │
│              Keyboard / Mouse → PlayerController         │
│              Keyboard V / Mouse → PlayerCameraController │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│                    玩法层 (Gameplay)                     │
│  PlayerController ──读取──► PlayerStats (精力/生命)      │
│       │                        ▲                         │
│       │ 相机朝向               │ 归一化数值               │
│       ▼                        │                         │
│  PlayerCameraController        PlayerHUD (UI 显示)       │
└──────────────────────────┬──────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│                    世界层 (World)                        │
│  MultiFloorLevelBuilder → Building_Large (静态碰撞几何)   │
│  CharacterController 与 BoxCollider 发生物理阻挡          │
└─────────────────────────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────┐
│                 工具层 (Editor / MCP)                    │
│  MultiFloorLevelBuilderMenu / execute_code / MCP 工具    │
└─────────────────────────────────────────────────────────┘
```

### 4.1 模块职责一览

| 模块 | 文件 | 职责 | 依赖 |
|------|------|------|------|
| **数值** | `PlayerStats.cs` | 生命/精力存取与回复规则 | 无 |
| **移动** | `PlayerController.cs` | 输入、移动、跳、冲刺 | PlayerStats、CharacterController、相机 Transform |
| **相机** | `PlayerCameraController.cs` | 第一/第三人称、鼠标视角 | Player Transform |
| **UI** | `PlayerHUD.cs` | 血条/精力条显示 | PlayerStats |
| **关卡生成** | `MultiFloorLevelBuilder.cs` | Editor 内程序化建筑 | 仅 Editor / 菜单调用 |
| **菜单入口** | `Editor/MultiFloorLevelBuilderMenu.cs` | 一键重建关卡 | MultiFloorLevelBuilder |

### 4.2 设计原则（便于你扩展）

1. **数据与行为分离**：精力逻辑在 `PlayerStats`，`PlayerController` 只问「能不能跳/能不能跑」。
2. **相机与移动解耦**：移动看相机水平方向；第一人称时旋转由相机主导。
3. **关卡与玩法解耦**：建筑是静态几何，改关卡只需重新 `Generate`，不必改 Player 脚本。
4. **UI 只读数据**：`PlayerHUD` 不修改数值，避免双份逻辑。

---

## 5. 操作与参数调整指南

### 5.1 运行时操作

| 按键 | 功能 |
|------|------|
| W A S D | 移动 |
| Space | 跳跃（消耗精力） |
| Shift | 冲刺（消耗精力） |
| V | 切换第一人称 / 第三人称 |
| 鼠标 | 转动视角 |

### 5.2 Inspector 调参位置

| 想改的内容 | 选中对象 | 组件 |
|------------|----------|------|
| 移动速度、跳跃高度、冲刺倍率 | Player | Player Controller |
| 生命、精力、消耗、回复 | Player | Player Stats |
| 视角、灵敏度、切换键 | Main Camera | Player Camera Controller |
| 建筑层高、楼梯尺寸 | 无（改代码常量后重新生成） | `MultiFloorLevelBuilder.cs` 顶部 `const` |

### 5.3 重新生成建筑

Unity 菜单：**Tools → Generate Multi-Floor Level**

会删除 `Building` / `Building_Large` 并按当前代码重新创建。

---

## 6. 项目目录（与玩法相关部分）

```
AI connection/
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity          # 主场景
│   ├── Scripts/
│   │   ├── PlayerController.cs        # 玩家移动
│   │   ├── PlayerStats.cs             # 生命/精力
│   │   ├── PlayerCameraController.cs  # 相机
│   │   ├── PlayerHUD.cs               # UI
│   │   ├── MultiFloorLevelBuilder.cs  # 关卡生成
│   │   └── Editor/
│   │       └── MultiFloorLevelBuilderMenu.cs
│   ├── Settings/                      # URP 资源配置
│   └── InputSystem_Actions.inputactions # 项目自带 Input Actions（当前脚本用 Keyboard.current 直连）
├── Packages/
│   └── manifest.json                  # 含 unity-mcp、URP、Input System、ProBuilder 等
└── PROJECT_GUIDE.md                   # 本文档
```

---

## 7. 已知问题与修复记录（学习参考）

| 问题 | 原因 | 处理 |
|------|------|------|
| Cursor 连不上 Unity | MCP Server / Session 未启动 | Connect 页 Start Server + Start Session |
| Console 999+ Input 报错 | `StandaloneInputModule` 调用旧 Input API | PlayerHUD 不再创建 EventSystem |
| 跳跃时好时坏 | `isGrounded` 不稳定 + 分两次 Move | Coyote Time、Jump Buffer、合并位移 |
| 上楼卡头 | 上下楼梯重叠 + 楼板开洞过小 | 折返楼梯 + 加高层高 + 扩大楼梯井 |
| MCP 找不到 PlayerController 类型 | 脚本未编译完成 | `refresh_unity` 或等编译后再 `AddComponent` |

---

## 8. 后续可扩展方向

- **生命值**：在 `PlayerStats` 增加 `TakeDamage()`，触发条件后扣血
- **交互键**：射线检测 + `IInteractable`，门、拾取物等
- **更大场景**：Asset Store 模块化包 / Blender 导出 FBX，替换或补充 `Building_Large`
- **ProBuilder**：安装后可用 MCP `manage_probuilder` 做更精细几何
- **Input Actions 资产**：将 `InputSystem_Actions.inputactions` 与 `PlayerController` 绑定，替代硬编码 `Keyboard.current`

---

## 9. 参考链接

- [MCP for Unity 文档](https://coplaydev.github.io/unity-mcp/)
- [Unity Input System 手册](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/index.html)
- [URP 文档](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.4/manual/index.html)

---

*文档随项目迭代更新；若场景 Hierarchy 与脚本有增删，请以 Unity Editor 与 `Assets/Scripts` 实际内容为准。*
