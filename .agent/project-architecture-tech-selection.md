# WinMachine 项目架构与技术选型

## 1. 项目概览
WinMachine 是一个基于 .NET 10 的现代化工业自动化控制系统框架。该项目采用**DSL（领域特定语言）驱动**的开发模式，强调声明式编程、响应式状态管理以及仿真优先的设计理念。

## 2. 技术选型 (Technology Stack)

| 类别 | 技术/库 | 说明 |
| :--- | :--- | :--- |
| **开发平台** | .NET 10 (Windows) | 利用最新 .NET 平台的性能与特性 |
| **UI 框架** | Windows Forms | 目前用于宿主仿真演示与操作界面 |
| **响应式编程** | System.Reactive (Rx.NET) | 核心驱动机制，用于处理设备状态流、事件总线与异步编排 |
| **函数式编程** | LanguageExt.Core | 提供函数式扩展，增强代码健壮性 |
| **依赖注入** | Microsoft.Extensions.DependencyInjection | 标准 IOC 容器 |
| **测试框架** | xUnit | 单元测试与集成测试 |
| **序列化** | System.Text.Json | 用于配置文件的读写 |

## 3. 核心架构设计 (Architecture Design)

项目遵循**分层架构**与**解释器模式**，将“做什么”（定义）与“怎么做”（执行）严格分离。

### 3.1 架构分层
*   **Core (领域核心层)**: 定义所有基础原语、接口与 DSL 契约。不依赖具体硬件驱动或 UI 实现。
*   **Devices (设备驱动层)**: 提供具体的硬件适配（如 Leadshine, ZMotion）以及仿真器实现。
*   **Interpreters (解释器层)**: 系统的“大脑”。负责遍历 DSL 结构树（AST），将其翻译为运行时的行为或配置。
*   **Visualization (可视化层)**: 负责将抽象的设备状态映射到 UI 组件，支持仿真环境的视觉呈现。
*   **App (应用层 - WinMachine)**: 具体的业务编排，组合上述模块实现生产流程。

### 3.2 关键设计模式
1.  **DSL-First (DSL 优先)**:
    *   **Blueprint DSL**: 用于声明式地定义机器结构（板卡、轴、气缸、物理连接）。
    *   **Flow DSL**: 基于 LINQ 风格的 Fluent API，用于描述异步、并发的业务流程（Step/Task）。
2.  **Interpreter Pattern (解释器模式)**:
    *   DSL 定义本身只产生描述性数据结构（AST）。
    *   `BlueprintInterpreter`: 将机器蓝图转换为 `MachineConfig`。
    *   `SimulationFlowInterpreter`: 执行 Flow DSL，驱动仿真设备。
3.  **Reactive State (响应式状态)**:
    *   设备状态（如轴位置、IO信号）通过 `IObservable<T>` 暴露。
    *   业务逻辑通过订阅这些流来响应变化，而非轮询。

## 4. 目录结构详解 (Directory Structure)

```text
d:\projects\WinMachine\
├── Machine.Framework/              # 核心框架库
│   ├── Core/                       # [核心层]
│   │   ├── Blueprint/              # 机器定义 DSL (Interfaces, Builders)
│   │   ├── Configuration/          # 运行时配置模型 (MachineConfig)
│   │   ├── Flow/                   # 流程控制 DSL (Step, StepDesc)
│   │   ├── Hardware/               # 硬件抽象接口 (IAxis, ICylinder)
│   │   ├── Primitives/             # 基础值类型 (AxisID, DeviceID)
│   │   └── Simulation/             # 仿真物理引擎 (SimulatorAxis, Physics)
│   ├── Devices/                    # [驱动层] 厂商驱动实现
│   ├── Interpreters/               # [解释器层]
│   │   ├── Configuration/          # 配置解释器 (Blueprint -> Config)
│   │   └── Flow/                   # 流程解释器 (Flow -> Runtime)
│   └── Visualization/              # [视觉层] UI 绑定与呈现 DSL
├── WinMachine/                     # [应用层] 主应用程序
│   ├── SimulationFlowScenarios.cs  # 业务场景定义
│   └── SimulatorDemoForm.cs        # 仿真界面
└── tests/                          # 测试项目
```

## 5. 核心模块说明

### 5.1 Machine Blueprint (蓝图)
位于 `Core/Blueprint`。允许开发者使用类似以下的语法定义机器：
```csharp
MachineBlueprint.Define("DemoMachine")
    .AddBoard("Main", 0, b => b
        .UseSimulator()
        .AddAxis(AxisID.X, 0)
        .AddCylinder(CylID.Clamp, 1, 2));
```

### 5.2 Flow DSL (流程)
位于 `Core/Flow`。利用 C# LINQ 语法糖实现可读性极强的业务流程：
```csharp
from _ in Name("移动X轴").Next(Motion(AxisX).MoveTo(100))
from val in Sensor(Pressure).ReadAnalog()
where val > 10
select Unit.Default;
```

### 5.3 Simulation Engine (仿真引擎)
位于 `Core/Simulation`。包含具有物理特性的虚拟设备：
*   **SimulatorAxis**: 模拟加减速曲线、速度、位置反馈。
*   **SimulatorCylinder**: 模拟动作延时、到位信号反馈。
*   支持时间切片或基于 Rx 的时间流驱动。

## 6. 演进方向
*   **DSL 强化**: 引入更强的类型检查与元数据支持。
*   **可视化编辑器**: 基于 Blueprint DSL 开发拖拽式配置工具。
*   **数字孪生**: 增强 Simulation 层的物理真实度，支持与 3D 引擎对接。
