---
doc_id: TEST-PB-001
title: 物理蓝图重构测试规格
author: Antigravity
status: DRAFT
date: 2026-02-03
---

# TEST-Physical-Blueprint-Refactoring: 物理蓝图重构测试规格

## 1. 测试目标
验证物理属性在蓝图中的正确声明、深度校验逻辑的有效性，以及 API 映射的准确性。

## 2. 测试场景定义

### 场景 1: 正向物理声明 (Positive: Basic Physical Attributes)
- **描述**: 构建一个包含所有新增物理类型的合法蓝图，覆盖多种设备类型。
- **输入**:
    ```csharp
    // Suction Pen
    .Mount("Cyl_Z", m => m
        .Vertical()
        .WithAnchor(PhysicalAnchor.TopCenter)
        .AsSuctionPen(5, 50)
        .WithStroke(0, 0, 50)
    )
    // Rotary Table
    .Mount("TurnTable", m => m
        .Horizontal()
        .AsRotaryTable(radius: 150)
        .WithAnchor(PhysicalAnchor.Center)
    )
    ```
- **验证点**:
    - [ ] `MountPointDefinition` 中的 `PhysicalMeta` 不为空。
    - [ ] 类型识别为 `SuctionPen`，直径 5，长度 50。
    - [ ] 类型识别为 `RotaryTable`，半径 150。
    - [ ] 锚点设置为 `TopCenter` 和 `Center`。

### 场景 2: 运动对齐校验 - 静态矢量冲突 (Negative: Static Alignment Conflict)
- **描述**: 验证 `Vertical()` 标记与 `WithStroke` 矢量的冲突。
- **输入**:
    ```csharp
    .Mount("Error_Node")
        .Vertical()          -- 声明为垂直方向 (Z向上)
        .WithStroke(100, 0, 0) -- 却定义了水平移动向量 (X方向)
    ```
- **预期结果**: 编译阶段抛出 `ValidateError`，信息包含 "AlignmentConflict"。

### 场景 3: 运动对齐校验 - 动态轴方向冲突 (Negative: Dynamic Link Conflict)
- **描述**: 验证 `Orientation` 声明与关联物理轴运动方向的冲突。
- **前提**: `Axis_X` 在逻辑层定义为水平运动。
- **输入**:
    ```csharp
    .Mount("Error_Link")
        .Vertical()         -- 声明为垂直方向
        .LinkTo(Axis_X)     -- 关联了一个水平轴
    ```
- **预期结果**: 在 `Blueprint.Build()` 阶段检测出对齐异常并阻断。

### 场景 4: 标准化坐标系验证 (Property: Frame Standardization)
- **描述**: 验证组件原点是否按照规范自动归位。
- **测试用例**: 定义一个 `AsLinearGuide(500)`。
- **验证点**: 验证其内部计算的几何原点（Effective Origin）是否严格位于 `Stroke=0` 处。

### 场景 5: API 映射完整性 (Integration: Telemetry Schema)
- **描述**: 验证物理元数据是否能穿透到 Web API 层。
- **动作**: 执行 `WebMachineModelMapper.Map(blueprint)`。
- **验证点**:
    - [ ] 生成的 `WebSceneNode` 的 `physicalType` 为字符串 "SuctionPen"。
    - [ ] `size` 字段被正确填充为 `(5, 5, 50)`。
    - [ ] 所有的 `Anchor` 枚举被转换为字符串 (如 "TopCenter")。

### 场景 6: 物料槽几何 (Feature: Material Slot Geometry)
- **描述**: 验证物料托盘/槽位的物理边界定义，用于后续的物料状态感知。
- **输入**:
    ```csharp
    .Mount("Tray_Slot_1", m => m
        .Horizontal()
        .AsMaterialSlot(width: 40, height: 40) // 定义了一个 40x40 的接收区
        .WithAnchor(PhysicalAnchor.Center)
    )
    ```
- **验证点**:
    - [ ] 物理类型被记录为 `MaterialSlot`。
    - [ ] 尺寸被记录为 `(40, 40, 0)` (2D 区域) 或 `(40, 40, 1)` (如果默认为薄片)。
    - [ ] 用于无头仿真时，能正确计算物料是否处于该矩形范围内。

## 3. 验收标准
1. 所有 `Positive` 场景 100% 通过。
2. 所有 `Negative` 场景能准确触发指定的异常类型，且错误提示包含关键词。
3. `WinMachine.Server` 启动后，控制台无 Blueprint 加载相关的警告。

## 4. 评审点
- **测试确认**: 请评审上述测试场景。确认请回复：**测试确认**。
