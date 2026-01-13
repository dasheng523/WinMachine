# WinMachine 项目架构与技术选型全盘总结（2026-01-13）

## 1. 项目概览
- 解决方案：`WinMachine.slnx`
- 形态：Windows 桌面应用（WinForms），通过依赖注入（DI）将“硬件抽象 + 设备实现 + 业务流程/状态机 + UI”组合到一起。
- 目标：围绕“运动控制器（Motion Controller）”与 IO/传感器/气缸等硬件抽象，支持模拟器与多厂商控制卡（正运动 Zaux、雷赛 Leadshine）。
- 测试：以 DSL 形状测试 + 单元测试为主，覆盖 `Common` 的基础类型/DSL、`WinMachine` 的 DSL 入口形状。

## 2. 解决方案结构（工程清单）
`WinMachine.slnx` 当前包含 7 个项目：
- `Common/Common.csproj`（net10.0）
- `Devices/Devices.csproj`（net10.0-windows）
- `Devices.Sample/Devices.Sample.csproj`（net10.0-windows，WinForms 示例/调试入口）
- `WinMachine/WinMachine.csproj`（net10.0-windows）
- `tests/Common.Tests/Common.Tests.csproj`
- `tests/Devices.Tests/Devices.Tests.csproj`
- `tests/WinMachine.Tests/WinMachine.Tests.csproj`

## 3. 分层架构（职责与边界）
整体采用“领域基础设施（Common）→ 硬件适配层（Devices）→ 组合根/应用层（WinMachine）→ tests” 的分层。

### 3.1 Common（基础库 / 领域内核 / DSL 支撑）
- 目标框架：`net10.0`
- 角色定位：领域基础设施与通用机制（可被 WinMachine 与 Devices 复用），偏“函数式/组合式”风格。
- 关键模块（按目录划分）：
  - `Core/`：基础类型与扩展（例如 FinEnumerableExtensions 这类“可组合”的序列扩展）。
  - `Fsm/`：轻量状态机（触发器流 → 状态流），含 Builder/DSL。
  - `Lifecycle/`：机器生命周期状态建模（`MachineState` 等）。
  - `Hardware/`：硬件领域抽象（气缸、传感器、Level/电平、值修正/约束等）。
  - `Steps/`：步骤执行模型（`Step` 等），用于表达“可组合的业务动作”。
  - `Ui/`：配置/界面 DSL（FieldBuilder/Validators/Nodes/Spec/UiDsl 等），用于声明式描述 UI 或配置字段形状。

> 备注：从仓库结构看，Common 其实是“领域+DSL 运行时”，不仅仅是基础类型。

### 3.2 Devices（设备/硬件抽象与实现）
- 目标框架：`net10.0-windows`（驱动/适配层本体，不再承载 UI）
- 角色定位：硬件驱动适配层（厂商 SDK 封装 + 统一接口），为上层提供一致的运动/IO 能力。
- 关键模块：
  - `Motion/Abstractions/`：运动控制抽象 `IMotionController<TAxis,TIn,TOut>` 与运动相关数据结构（`AxisSpeed`、`AxisStatus`、`MotionDirection` 等）。
  - `Motion/Implementations/`：
    - `Simulator/`：纯托管模拟器，离线调试与自动化测试友好。
    - `Zaux/`：对接正运动 SDK（`cszmcaux`），以太网连接（如 `ZAux_OpenEth`）。
    - `Leadshine/`：对接雷赛 SDK（`Leadshine.LTSMC`），以 `smc_*` 为主（如 `smc_board_init`）。
  - `Sensors/`：传感器接入（如 Modbus/Serial 等实现目录）。
  - `Shared/`：共享组件/工具（用于 Devices 内部复用）。

### 3.3 WinMachine（主应用 / 组合根 + UI + 机器服务）
- 目标框架：`net10.0-windows`（WinForms 主程序）
- 角色定位：应用层与组合根（Composition Root）。负责：读取配置、选择硬件实现、组合机器服务、启动 UI。
- 关键模块：
  - `Program.cs`：唯一组合根入口（加载配置、注册 DI、按配置选择 Motion 实现）。
  - `Configuration/`：配置模型与 DSL（例如 `SystemOptions`、`MachineMapDsl`、`SingleStepOptions`）。
  - `Services/`：应用服务层（`MachineManager`、`HardwareFacade`、MotionSystem、Axis/Cylinder/IO Resolver、SensorMapResolvers 等），典型职责是把“抽象硬件 + 业务步骤/状态”粘合成可运行的机器。
  - `ConfigUi/`：配置界面相关（例如 LeadshineInitUi、SystemOptionsUi、WinForms 子目录）。
  - WinForms UI：`Form1`、`ZControllerView`、`SingleStep` 等。

### 3.4 tests（测试与 DSL 形状约束）
### 3.5 Devices.Sample（WinForms 示例/调试入口）
- 目标框架：`net10.0-windows` + WinForms
- 角色定位：给硬件适配层提供一个最小可运行的手工调试入口（例如快速验证控制卡连接、轴/IO 基本能力），避免把 UI 混进驱动项目本体。

- `Common.Tests`：覆盖 Common 的基础行为（例如 ValueCoercer/相关测试）。
- `Devices.Tests`：面向设备抽象/实现的单元测试（当前目录存在）。
- `WinMachine.Tests`：包含多项“DSL shape tests”（如 HardwareDslTests、MachineMapDslShapeTests、StepDslShapeTests），确保 DSL 的语法外观与可组合性不被破坏。

## 4. 依赖方向与边界规则
推荐/当前依赖方向（与仓库现状一致）：
- `Common`：不依赖 `Devices` 与 `WinMachine`
- `Devices`：依赖 `Common`（主要是 Core/基础类型；也可能复用 Hardware 抽象）
- `WinMachine`：依赖 `Common` + `Devices`
- 组合根唯一入口：`WinMachine/Program.cs`

## 5. 关键运行时流程
### 5.1 启动与装配（从启动到主窗体）
1. WinForms 启动（`ApplicationConfiguration.Initialize()`）。
2. `ConfigurationBuilder` 从输出目录加载 `appsettings.json`。
3. DI 注册：
   - Options：`SystemOptions` 等。
   - UI：主窗体与各 View。
   - 应用服务：`MachineManager` / Facade / Resolvers 等。
   - Motion：按配置注册 `IMotionController<...>` 的实现（Simulator / Zaux / Leadshine）。
4. 解析主窗体并 `Application.Run(...)`。

### 5.2 业务流程（设备物料流的“现实世界”视角）
结合当前机器描述，流程核心是：上料盘→扫码座→转移/翻转→测试座→下料盘/回盘。概念上可拆为三个协作子系统：
- 上下料机构（X/Y/Z 轴 + 吸笔真空）：负责取放料与位姿移动。
- 扫码座 + 转移机构（气缸/夹爪/旋转）：负责左右侧工位交换与翻转。
- 测试机构：产生高度值，按配置判定 OK/NG，并驱动后续分拣。

在代码落地上，这些动作通常会被表达为：
- `Common.Steps` 中的“步骤”（可组合动作）
- `Common.Fsm` 中的“状态机状态/触发器”
- `WinMachine.Services` 中的 Facade/Resolver 把抽象硬件映射到具体轴/IO/气缸/传感器

## 6. 技术选型（What & Why）
### 6.1 运行框架与 UI
- .NET：
  - `WinMachine`/`Devices`：`net10.0-windows` + `UseWindowsForms=true`
  - `Common`：`net10.0`
- UI：WinForms（工业设备桌面应用：部署简单、厂商 SDK/Win32 生态兼容、调试成本低）。

### 6.2 依赖注入（DI）
- `Microsoft.Extensions.DependencyInjection (10.0.1)`
- 价值：将“厂商控制卡/模拟器”的选择下沉到 Composition Root，UI 与业务只面向抽象接口。

### 6.3 配置系统（Configuration + Options）
- `Microsoft.Extensions.Configuration.Json (10.0.1)`：读取 `appsettings.json`
- `Microsoft.Extensions.Options.ConfigurationExtensions (10.0.1)`：Options Pattern（`services.Configure<TOptions>`）
- 价值：运行时可切换 Simulator/真实控制卡、设备 IP、以及后续可扩展的站位/盘参数。

### 6.4 响应式编程（Rx）
- `System.Reactive`：
  - `WinMachine`/`Devices`：`6.1.0`
  - `Common`：`6.0.1`
- 用途：
  - 状态机用 `BehaviorSubject` 暴露状态流。
  - 服务层用 `Observable.Timer/Interval` 处理轮询/超时/心跳（当前已有轮询形态）。

### 6.5 状态机与 DSL
- `Common.Fsm.StateMachine`：触发器流（`Subject<...>`）→ 状态流（`BehaviorSubject<...>`），便于 UI 订阅、日志、以及流程可视化。
- DSL：
  - 状态机构建 DSL（Builder）
  - UI/配置 DSL（`Common/Ui` 与 `WinMachine/Configuration`）
  - 步骤 DSL（`Common/Steps`）
这些 DSL 的“语法外观”由 `tests/WinMachine.Tests` 中的 shape tests 做回归保护。

### 6.6 厂商 SDK 封装（硬件适配）
- Zaux：SDK 返回码封装为异常（如 `ZauxException`），以太网句柄 `_handle`。
- Leadshine：以 `smc_*` API 为主，返回码封装为 `LeadshineException`。
- Simulator：托管实现，通常用 Task/延迟模拟运动完成。

## 7. 扩展点（未来新增硬件/流程的落点）
- 新增控制卡：在 `Devices/Motion/Implementations/<Vendor>/` 添加实现，并在 `WinMachine/Program.cs` 的 DI 选择处接入。
- 新增传感器协议：在 `Devices/Sensors/` 下扩展实现，并通过 `WinMachine/Services/SensorMapResolvers.cs` 之类的映射层接入业务。
- 新增“机器动作/工艺流程”：优先落到 `Common/Steps`（动作可组合）、`Common/Fsm`（状态驱动），由 `WinMachine/Services` 负责绑定到具体轴/IO。
- 新增配置与 UI：配置模型落到 `WinMachine/Configuration`，字段与校验/展示形状落到 `Common/Ui` 与 `WinMachine/ConfigUi`。

## 8. 当前实现特征与可改进点（中立记录）
- 类型安全：Motion 泛型参数当前主程序常用 `int,int,int`（轴/IO 号位用 int 表示）；后续可用 enum/强类型 id 增强可读性。
- 可控性：`MachineManager` 中基于 Rx 的轮询/订阅目前未系统性引入取消/超时兜底（例如回零永不完成）。
- 依赖一致性：`System.Reactive` 版本在 `Common` 与其它项目不一致（`6.0.1` vs `6.1.0`），建议统一以减少 binding/依赖冲突风险。
- 部署风险：厂商 SDK 可能包含 native DLL/驱动依赖，需要明确 x86/x64、运行时文件、权限与目标机器环境。

## 9. 快速定位清单（常用入口）
- 组合根/装配：`WinMachine/Program.cs`
- 配置模型与 DSL：`WinMachine/Configuration/*`
- 机器服务层：`WinMachine/Services/*`（MachineManager/Facade/Resolvers）
- 状态机与生命周期：`Common/Fsm/StateMachine.cs` + `Common/Lifecycle/*`
- 硬件抽象（气缸/传感器/电平/值修正）：`Common/Hardware/*`
- UI DSL：`Common/Ui/*` + `WinMachine/ConfigUi/*`
- 运动抽象与实现：`Devices/Motion/Abstractions/*` + `Devices/Motion/Implementations/*`
- DSL 形状测试：`tests/WinMachine.Tests/*ShapeTests.cs`
