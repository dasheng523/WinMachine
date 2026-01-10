# WinMachine 项目框架与技术选型总结（2026-01-10）

## 1. 项目概览
- 解决方案：`WinMachine.slnx`
- 形态：Windows 桌面应用（WinForms），通过依赖注入组合硬件驱动与业务状态机。
- 目标：围绕“运动控制器（Motion Controller）”进行设备抽象，支持模拟器与多厂商控制卡（正运动 Zaux、雷赛 Leadshine）。

## 2. 工程结构（分层/职责）
### 2.1 Common（基础库 / 领域内核）
- 目标框架：`net10.0`
- 主要内容：
  - `Common.Core`：基础类型与设备约定（如 `Level`、`IDevice`）。
  - `Common.Lifecycle`：机器生命周期状态与触发器（`MachineState` / `MachineTrigger`）。
  - `Common.Fsm`：轻量状态机实现 + Builder DSL（基于 Rx 的事件流驱动）。
- 角色定位：**领域基础设施与通用机制**（可被上层 WinForms 与 Devices 复用）。

### 2.2 Devices（设备/硬件抽象与实现）
- 目标框架：`net10.0-windows`（WinExe；当前项目里也包含 WinForms 的 `Form1` 示例）
- 主要内容：
  - `Devices.Motion.Abstractions`：运动控制抽象 `IMotionController<TAxis,TIn,TOut>` 与运动相关数据结构（`AxisSpeed`、`AxisStatus`、`MotionDirection`）。
  - `Devices.Motion.Implementations`：
    - `Simulator`：纯托管模拟器实现，便于离线调试。
    - `Zaux`：对接正运动 SDK（`cszmcaux`），走以太网连接（`ZAux_OpenEth`）。
    - `Leadshine`：对接雷赛 SDK（`Leadshine.LTSMC`），初始化 `smc_board_init` 等。
- 角色定位：**硬件驱动适配层**（厂商 SDK 封装 + 统一接口）。

### 2.3 WinMachine（主应用 / 组合根 + UI）
- 目标框架：`net10.0-windows`，WinForms 主程序。
- 主要内容：
  - `Program.cs`：Composition Root（加载配置、注册 DI、选择 Motion 实现、启动 WinForms 主窗体）。
  - `Configuration/SystemOptions.cs`：配置模型（UseSimulator、ControllerType、DeviceIp）。
  - `Services/MachineManager.cs`：机器服务（状态机 + Motion 控制器的“缝合层”）。
  - WinForms：`Form1`、`ZControllerView`（示例 UI：JOG、IO 读写、位置刷新等）。

## 3. 技术选型（What & Why）
### 3.1 运行框架
- .NET：
  - `WinMachine`/`Devices`：`net10.0-windows` + `UseWindowsForms=true`
  - `Common`：`net10.0`
- UI 框架：WinForms（适合工业/设备类桌面应用：部署简单、与厂商 SDK/Win32 生态兼容）。

### 3.2 依赖注入（DI）
- `Microsoft.Extensions.DependencyInjection (10.0.1)`
- 用法：在 `WinMachine/Program.cs` 注册窗体、服务、以及按配置选择 `IMotionController<int,int,int>` 的具体实现。
- 特点：以 “Composition Root” 模式集中装配；UI 通过构造函数注入依赖。

### 3.3 配置系统（Configuration + Options）
- `Microsoft.Extensions.Configuration.Json (10.0.1)`：读取 `appsettings.json`
- `Microsoft.Extensions.Options.ConfigurationExtensions (10.0.1)`：Options Pattern（`services.Configure<SystemOptions>`）
- 当前配置：`System.UseSimulator=true`、`System.ControllerType=Simulator`、`System.DeviceIp=192.168.0.11`

### 3.4 响应式编程（Rx）
- `System.Reactive`：
  - `WinMachine`/`Devices`：`6.1.0`
  - `Common`：`6.0.1`
- 用途：
  - 状态机用 `BehaviorSubject` 暴露状态流。
  - `MachineManager` 中使用 `Observable.Timer/Interval` 模拟/轮询连接与回零完成。

### 3.5 状态机与生命周期建模
- `Common.Fsm.StateMachine`：触发器流（`Subject<MachineTrigger>`）→ 状态流（`BehaviorSubject<MachineState>`）。
- `StateMachineBuilder`：用 DSL 方式声明转移与 entry actions。
- `Common.Lifecycle.MachineState/MachineTrigger`：定义机器运行状态集合。

### 3.6 硬件 SDK 封装（厂商适配）
- 正运动（Zaux）：依赖 `cszmcaux`，以太网连接句柄 `_handle`，对 SDK 返回码封装为 `ZauxException`。
- 雷赛（Leadshine）：依赖 `Leadshine.LTSMC`，以 `smc_*` API 调用为主，返回码封装为 `LeadshineException`。
- Simulator：托管模拟器，用 Task 延迟模拟运动完成。

## 4. 关键运行流程（从启动到运行）
1. WinForms 启动（`ApplicationConfiguration.Initialize()`）。
2. `ConfigurationBuilder` 从输出目录加载 `appsettings.json`。
3. DI 注册：
   - Options：`SystemOptions`
   - UI：`Form1`、`ZControllerView`
   - 业务服务：`IMachineService -> MachineManager (Singleton)`
   - Motion：按配置注册 `IMotionController<int,int,int>` 的实现（Simulator / Zaux / Leadshine）。
4. 从容器解析 `Form1` 并 `Application.Run(mainForm)`。

## 5. 模块边界与依赖方向（建议视图）
- `Common`：不依赖 `Devices` 与 `WinMachine`（当前满足）。
- `Devices`：依赖 `Common.Core`（当前满足）。
- `WinMachine`：依赖 `Common` + `Devices`（当前满足）。
- 组合根唯一入口：`WinMachine/Program.cs`。

## 6. 当前实现特征与可改进点（中立记录）
- Motion 泛型参数目前在主程序中使用 `int,int,int`（轴/IO 号位以 int 表示）；后续可用枚举类型增强可读性与安全性。
- `MachineManager` 的 Rx 轮询与订阅目前未做取消/超时控制（例如回零永不完成时的兜底）。
- `Common.csproj` 与其它项目的 `System.Reactive` 版本不一致（`6.0.1` vs `6.1.0`）；一般建议统一以减少潜在 binding/依赖冲突。
- 硬件 SDK 可能涉及 native DLL/驱动依赖（仓库中存在 `SDK/` 目录），部署时需要明确 x86/x64、运行时文件与权限要求。

## 7. 快速定位清单（常用入口）
- 组合根/装配：WinMachine/Program.cs
- 配置模型：WinMachine/Configuration/SystemOptions.cs
- 机器状态机：Common/Fsm/StateMachine.cs + Common/Lifecycle/MachineState.cs
- 机器服务：WinMachine/Services/MachineManager.cs
- 运动抽象：Devices/Motion/Abstractions/IMotionController.cs
- 运动实现：Devices/Motion/Implementations/(Simulator|Zaux|Leadshine)
