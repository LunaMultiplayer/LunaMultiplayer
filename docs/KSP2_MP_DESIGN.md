# KSP2 联机 Mod 设计文档（基于 LunaMultiplayer 适配）

> 目标：把 KSP1 的联机 Mod **LunaMultiplayer** 适配为 **坎巴拉太空计划 2（KSP2）** 的联机 Mod。
> 仓库：`LiuFenCN/LunaMultiplayer`（已从 `LunaMultiplayer/LunaMultiplayer` fork）
> 工作区：`ksp2_mp/`（本项目）
> 生成日期：2026-08-05

---

## 0. 结论先行

**可行，且比我最初担心的容易得多。** KSP2 不是"没有 modding API 的死库"，你的环境已经把联机 mod 需要的一切铺好了：

| 前提条件 | 实测状态 | 证据 |
|---|---|---|
| 运行时可反编译 | ✅ **Mono**（非 IL2CPP） | `KSP2_x64_Data/Managed/` 存在，`Assembly-CSharp.dll` 是标准 .NET DLL |
| modding 框架 | ✅ SpaceWarp2 已装 | `SpaceWarp2.dll` + `SpaceWarp2.Game.dll` |
| 资源热补丁 | ✅ PatchManager 已打 85 补丁 / 0 错误 | `pm_summary.log` |
| 调试/接入接口 | ✅ ReduxLib + DebugTools.UI | `Managed/ReduxLib.dll`、`DebugTools.UI.dll`；热键 `LeftAlt+F12`（SpaceWarp 调试 UI） |
| 可挂钩的飞船 API | ✅ `KSP.Sim` 命名空间完整 | 详见第 2 节（已用反射测绘） |
| 权威/所有权模型 | ✅ 游戏自带 `IsLocallyOwned`/`IsLocallyAuthorized` | `VesselComponent` / `PartComponent` 均有此标记 |

> ⚠️ 旧结论修正：目录里那个 `BepInEx/plugins/LunaMultiplayer/` **是 KSP1 的 LMP v0.29.0（`KSP_VERSION: 1.12`）残骸，对 KSP2 无效**，且 `Ksp2.log` 中无任何 LMP 加载记录——它只是被人误丢进去了。本项目的代码不依赖它。

---

## 1. 工作机制总览

采用 **"网络层复用 + 同步层重写"** 策略：

- **网络层（直接复用 LMP 设计与代码）**：Lidgren 可靠 UDP、NTP 时间同步、Subspace 时间扭曲隔离、客户端插值防抖、Master Server + NAT 穿透。这部分与游戏无关，搬过来即可。
- **同步层（必须重写）**：LMP 里"读取/写入飞船位置、零件、燃料、对接"的代码全部挂钩 KSP1 内部 API；KSP2 的仿真在 `KSP.Sim` 命名空间（`VesselComponent`/`PartComponent`/`SpaceSimulation`…），结构不同，需用 **Harmony** 针对 KSP2 重新实现。

---

## 2. 核心 API 测绘结果（已用 .NET 反射实测）

> 全部来自 `F:\Program Files\Epic Games\Kerbal.Space.Program.2\KSP2_x64_Data\Managed\Assembly-CSharp.dll`（Mono 元数据反射，非猜测）。

### 2.1 中央入口 `KSP.Game.GameInstance`
联机 mod 从这里拿到全局仿真、时间、玩家、事件系统：

- `SpaceSimulation SpaceSimulation` ← **全局仿真管理器（最重要）**
- `UniverseModel UniverseModel` / `UniverseView UniverseView`
- `LocalPlayer LocalPlayer` ← 本地玩家身份
- `MessageCenter Messages` ← 游戏事件总线（可挂钩）
- `MPMonoBehaviour MPMonoBehaviour` ← **游戏内已存在 MP 相关基类，重要信号**
- `OnlineServicesFramework OnlineServices` ← 在线服务框架
- `KSP2ModManager KSP2ModManager`

> 访问方式：`Game.Instance.SpaceSimulation`（需确认 `Game` 静态访问器；`GameInstance` 本身已含全部属性）。

### 2.2 `KSP.Sim.impl.SpaceSimulation`（全局仿真）
- `ICollection<IGGuid> GetVesselGuids()` ← 枚举所有飞船 ✅
- `SimulationObjectModel FindSimObject(IGGuid)` / `FindSimObjectByNameKey(String)`
- `T GetSimulationObjectComponent<T>(IGGuid)` ← 取某飞船的 `VesselComponent` ✅
- `ICollection<...> GetAllSimulationObjectsWithComponent<T>()`
- `Void SubmitViewAction(IViewAction)` / `SubmitViewActions(...)` ← **远端输入可作为 ViewAction 注入仿真** ✅
- `Void OnFixedUpdate(Single)` ← 仿真 tick（挂钩点）
- `Void TeleportSimObjectToOrbit(...)` / `TeleportSimObjectToSurface(...)` ← 传送/出生点

### 2.3 `KSP.Sim.impl.VesselComponent`（飞船状态——同步核心）
**变换 / 速度：**
- `ITransformModel transform` / `ITransformModel ControlTransform`（位置 + 旋转）
- `Position CenterOfMass` / `Position LabelPosition`
- `Velocity Velocity` / `AngularVelocity AngularVelocity`
- `Vector OrbitalVelocity` / `Vector SurfaceVelocity`

**轨道 / 姿态：**
- `PatchedConicsOrbit Orbit` / `OrbiterComponent Orbiter`
- `VesselSituations Situation` / `Boolean Landed` / `Splashed` / `IsFlying`

**资源：**
- `Double FuelPercentage` / `Double StageFuelPercentage`
- `Double StoredElectricityPercentage`

**控制 / 权威：**
- `FlightCtrlState flightCtrlState`
- `VesselAutopilot Autopilot`
- `Boolean IsLocallyOwned` / `Boolean IsLocallyAuthorized` ← **权威模型天然基础** ✅
- `IGGuid GlobalId` / `String Name` / `String DisplayName`
- `Object GetState()` / `Void SetState(Object, ISimulationModelMap)` ← 可序列化状态

### 2.4 `KSP.Sim.impl.VesselBehavior`（飞船视图 MonoBehaviour）
- `VesselComponent Model` / `SimulationObjectView ViewObject`
- `Transform transform` / `Transform ControlTransform`（Unity 世界变换，远端状态写这里）
- `IEnumerable<PartBehavior> parts`
- `Void SyncTo(VesselComponent)` ← 仿真→渲染同步（挂钩点）
- `FlightCtrlState flightCtrlState`

### 2.5 `KSP.Sim.impl.PartComponent`（零件级）
- `ITransformModel transform` / `ControlTransform`
- `ResourceContainer PartResourceContainer` ← **逐零件燃料容器** ✅
- `IResourceContainer[] Containers`
- `PartStatus Status` / `Boolean IsDamaged`
- `Boolean IsLocallyOwned` / `IsLocallyAuthorized`
- `Boolean TryGetModule<T>(T&)` ← 取引擎/燃料/对接口等模块 ✅
- `Object GetState()` / `SetState(...)`

### 2.6 `KSP.Sim.ResourceSystem.ResourceContainer`（资源读写）
- `Double GetResourceStoredUnits(ResourceDefinitionID)`
- `Double SetResourceStoredUnits(ResourceDefinitionID, Double)` ← **设置燃料量** ✅
- `Double AddResourceUnits/RemoveResourceUnits(ResourceDefinitionID, Double)`
- `Double GetResourceCapacityUnits/SetResourceCapacity(...)`
- `ResourceDefinitionID` 标识燃料/氧化剂/电力等资源类型

### 2.7 `KSP.Sim.impl.TimeWarp`（时间扭曲 / Subspace）
- `Single CurrentRate` / `Int32 CurrentRateIndex`
- `Boolean IsWarping` / `Boolean IsPhysicsTimeWarp`
- `Void SetRateIndex(Int32, Boolean)` / `IncreaseTimeWarp()` / `DecreaseTimeWarp()`
- `Void SetIsPaused(Boolean)` / `WarpTo(Double, ...)`
- `TimeWarpLevel[] GetWarpRates()`

> 访问器待确认：`Game.Instance.SpaceSimulation.UniverseModel` 或静态 `TimeWarp.Instance`。

---

## 3. 同步状态清单（要同步什么）

| 类别 | 字段（KSP.Sim 来源） | 同步方式 |
|---|---|---|
| 飞船变换 | `VesselComponent.transform`(Pos/Rot)、`Velocity`、`AngularVelocity` | 每 tick 发送 + 远端插值写入 `VesselBehavior.transform` |
| 轨道 | `Orbit`(PatchedConicsOrbit)、`Orbiter` | 轨道根数同步；落地/起飞用 `Situation` |
| 姿态/资源 | `Situation`、`Landed/Splashed`、`FuelPercentage`、`StoredElectricityPercentage` | 周期性 + 事件触发 |
| 零件级 | `PartComponent.PartResourceContainer`(各资源 `SetResourceStoredUnits`)、`Status`、`transform` | 拥有者变化时全量，之后增量 |
| 控制输入 | `flightCtrlState`、动作组(`KSPActionGroup`)、分级(`ActivateNextStage`)、`Autopilot` | 远端输入→`SubmitViewAction` 注入 |
| 时间/扭曲 | `TimeWarp.CurrentRate` / `SetRateIndex` / `IsPaused` | Subspace 隔离（见第 4 节） |
| 玩家/出生 | `LocalPlayer`、飞船 `GlobalId` 归属 | 加入/离开广播 |

---

## 4. 权威模型（Authority）

KSP2 的 `VesselComponent` / `PartComponent` **自带 `IsLocallyOwned` / `IsLocallyAuthorized`**——这比 KSP1 的 LMP 从头造权威模型省事：

- **本地拥有（IsLocallyOwned=true）的飞船**：本地是权威，按 LMP 方式采集状态广播给其他人；物理在本地跑。
- **远端飞船（IsLocallyOwned=false）**：本地为非权威，禁用本地物理控制，将收到远端状态插值应用到 `VesselBehavior.transform`（与 LMP 的 "ghost vessel" 一致）。
- **时间扭曲（Subspace）**：沿用 LMP 思路——不同步物理时间，各客户端按 `TimeWarp` 各自推进，用 NTP 对齐"宇宙时间"；当两船接近时，低速方自动降到物理时间（参考 LMP subspace 算法）。

---

## 5. Fork 内工程结构（KSP2 适配骨架）

```
LunaMultiplayer.KSP2/                 # BepInEx 插件工程（新）
├── LunaMultiplayer.KSP2.csproj
├── Directory.Build.props              # 指向 KSP2 Managed DLL（不入库，本地配置）
├── Network/                           # ★ 复用 LMP 网络层
│   ├── Lidgren/                       #   Lidgren.Network（KSP1 版可直搬）
│   ├── MasterServer.cs
│   ├── Subspace.cs                    #   时间扭曲隔离
│   └── MessageSystem.cs
├── Sync/                              # ★ KSP2 专属（重写）
│   ├── VesselSync.cs                  #   挂钩 SpaceSimulation 收发飞船状态
│   ├── PartSync.cs                    #   零件资源/状态
│   ├── ResourceSync.cs               #   燃料同步（ResourceContainer）
│   └── TimeSync.cs                    #   TimeWarp / NTP
├── Patches/                           # ★ Harmony 挂钩点
│   ├── SpaceSimulationPatch.cs        #   OnFixedUpdate 后发送/接收
│   ├── VesselBehaviorPatch.cs         #   SyncTo 时应用远端状态
│   └── TimeWarpPatch.cs
├── UI/                                # 联机菜单（SpaceWarp UI）
└── Plugin.cs                          # BepInEx 入口
```

> 引用程序集（**不入库**，用 `Directory.Build.props` 指向游戏目录）：
> `Assembly-CSharp.dll`、`SpaceWarp2.dll`、`SpaceWarp2.Game.dll`、`ReduxLib.dll`、`BepInEx.dll`、`0Harmony.dll`、`UnityEngine*.dll`。

---

## 6. Harmony 挂钩点（初步）

1. `SpaceSimulation.OnFixedUpdate(Single)` → 若本地是 host/server，采集所有 `GetVesselGuids()` 的 `VesselComponent` 状态并广播；若为客户端，将收包队列应用到对应 `VesselBehavior`。
2. `VesselBehavior.SyncTo(VesselComponent)` → 对非本地拥有的飞船，用远端插值结果覆盖 `transform`。
3. `TimeWarp.SetRateIndex` / `SetIsPaused` → 广播时间扭曲变化，触发 Subspace 重算。
4. `GameInstance` 初始化完成事件 → 启动联机服务（或挂到 SpaceWarp 生命周期）。

---

## 7. 构建与运行

- 目标框架：`.NET Framework 4.x`（与 KSP2 Mono 一致）。
- 产物：`LunaMultiplayer.KSP2.dll` + 依赖，放入游戏 `BepInEx/plugins/LunaMultiplayer.KSP2/`。
- 调试：用 Redux 的 `LeftAlt+F12` 调试 UI + `ReduxLib` API 实时查看飞船/零件状态，验证同步。

---

## 8. 风险与待确认

| 项 | 说明 | 状态 |
|---|---|---|
| TimeWarp 访问器 | 确认是 `SpaceSimulation.UniverseModel` 还是静态 `TimeWarp.Instance` | 待反射确认 |
| `MPMonoBehaviour` 是什么 | 游戏内已有 MP 基类，可能可复用或需规避 | 待反编译确认 |
| 物理权威 | KSP2 物理本身不稳定，netcode 同步需谨慎（尤其坠落/碰撞） | 已知风险 |
| `VesselState` 不透明 | 状态经 `SetState(Object, ISimulationModelMap)` 走模型映射，逐字段同步更稳 | 已决策：走逐字段 |
| 游戏已停更 | 无官方支持，但 Mono + 社区框架已足够 | 接受 |
| 现有 KSP1 LMP 残骸 | `BepInEx/plugins/LunaMultiplayer/` 需移出，避免干扰加载 | 待用户确认后处理 |

---

## 9. 下一步

1. ✅ fork 完成；clone 进行中（gh-proxy 限速，后台）。
2. 建 `ksp2` 分支，按第 5 节搭工程骨架。
3. 先实现 `VesselSync`：挂钩 `SpaceSimulation.OnFixedUpdate`，打印本机所有飞船 `GlobalId` + `transform`，验证能读到。
4. 接 LMP 的 Lidgren 收发，做单向广播（host→client）。
5. 加 `TimeSync` + Subspace。
6. 清理游戏目录里的 KSP1 LMP 残骸。
