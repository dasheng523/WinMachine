# WinMachine Blueprint DSL 技术文档

## 1. 概述 (Overview)
`Blueprint DSL` 是 WinMachine 架构中用于定义“机器灵魂”的核心契约。它通过强类型、流式接口 (Fluent Interface) 的方式，描述了设备的硬件组成、拓扑结构以及机械运动参数。

## 2. 核心结构 (Core Structures)

### 2.1 MachineBlueprint (根容器)
所有定义均从 `MachineBlueprint.Define("MachineName")` 开始，返回 `IMachineBlueprintBuilder`。

### 2.2 Board (控制器/背板)
描述逻辑背板，可以映射真实的硬件驱动（如 Leadshine, ZMotion）或模拟器。
- **`UseSimulator()`**: 启用模拟模式，无需真实硬件即可运行。
- **`AddAxis(id, channel, config)`**: 定义伺服轴或步进轴。
- **`AddCylinder(id, doOut, doIn, config)`**: 定义电磁阀控制的气缸。

### 2.3 MountPoint (安装点/机械拓扑)
描述机械部件之间的父子层级关系，支持无限嵌套。
- **`WithOffset(x, y, z)`**: 设置相对于父节点的初始安装偏移量。
- **`WithRotation(x, y, z)`**: 设置欧拉角旋转（单位：度）。
- **`LinkTo(deviceID)`**: **关键操作**。将当前的机械节点绑定到某个硬件设备（气缸或轴）。
- **`WithStroke(x, y, z)`**: 定义当关联的气缸从原位动作到动位时，该节点在空间中的位移向量。

## 3. 设备定义详情

### 3.1 轴 (Axis) 定义
通过 `IAxisBuilder` 配置运动特性：
- `WithRange(min, max)`: 物理软限位。
- `WithKinematics(vel, acc)`: 默认运动学参数。
- `Vertical()` / `Horizontal()`: 确定主运动方向。

### 3.2 气缸 (Cylinder) 定义
通过 `ICylinderBuilder` 配置动态特性：
- `WithDynamics(actionTimeMs)`: 定义气缸动作的期望/平均耗时（用于前端平滑动画插值）。
- `WithFeedback(diOut, diIn)`: 映射到位信号反馈端口。

---

# WinMachine Visuals DSL 技术文档

## 1. 概述 (Overview)
`Visuals DSL` 用于定义硬件在 3D 场景中的视觉呈现方式。它将 Blueprint 中定义的抽象设备关联到具体的 3D 组件（即 Web 端的 React-Three-Fiber 组件）。

## 2. 核心组件 (Components)

### 2.1 轴视觉 (Axis Visuals)
- **`AsLinearGuide(length, sliderWidth)`**: 渲染为直线导轨。
- **`AsRotaryTable(radius)`**: 渲染为圆盘转塔。
- **`WithPivot(x, y)`**: 设置旋转中心或缩放中心（0.0 ~ 1.0 比例）。

### 2.2 气缸视觉 (Cylinder Visuals)
- **`AsSlideBlock(size)`**: 渲染为简单的滑动块。
- **`AsGripper(open, close)`**: 渲染为平移夹爪，定义开合的间距（单位：mm）。
- **`AsSuctionPen(diameter)`**: 渲染为真空吸笔。
- **`AsCustom(modelPath)`**: 绑定外部定义的自定义 3D 模型。

## 3. 方向与动态控制
- **`Vertical()` / `Horizontal()`**: 锁定组件的渲染轴向。
- **`Reversed()`**: 翻转渲染方向。
- **`WithSize(w, h)`**: 手动调整组件的基础包围盒大小。

---

# WinMachine Flow DSL 技术文档

## 1. 概述 (Overview)
`Flow DSL` 是基于 LINQ 风格实现的函数式流程描述语言。它允许开发者像编写 C# 查询一样编写工业自动化的逻辑，天然具备“可组合性”和“异步非阻塞”特性。

## 2. 核心语法 (Core Syntax)

### 2.1 基础动作
- **`Name(string)`**: 为当前步骤命名，该名称会实时推送到前端 Telemetry 状态栏。
- **`Cylinder(id).FireAndWait(bool)`**: 发送气缸动作指令。`true` 对应动位，`false` 对应复位。
- **`Motion(id).MoveToAndWait(posFunc)`**: 控制轴移动。通过函数指定目标位置，增强了逻辑灵活性。

### 2.2 链式编排 (LINQ Monad)
使用 `from...select` 语法实现顺序执行：
```csharp
var myFlow = 
    from _1 in Cylinder(c1).FireAndWait(true)
    from _2 in Motion(a1).MoveToAndWait(100)
    select Unit.Default;
```

### 2.3 高级控制
- **`Loop(count)`**: 循环执行。设置 `-1` 为无限循环。
- **`InParallel(Step[])`**: 并发执行多个子流程。
- **`Step.Throw(message)`**: 逻辑异常抛出，会触发前端的错误报警状态。
- **`Retry(count)`**: 自动重试机制。
- **`WithTimeout(ms)`**: 为单个步骤设置硬性超时时间。

## 3. 执行上下文 (Context)
Flow 的执行依赖于 `FlowContext`，它通过 `config` 感知硬件状态，并驱动 `IScenarioFactory` 环境。
