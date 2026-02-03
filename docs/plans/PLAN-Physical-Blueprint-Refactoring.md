# PLAN-Physical-Blueprint-Refactoring: 物理蓝图重构计划 (v1.4)

## 1. 目标概述
将组件的物理几何属性（尺寸、朝向、锚点、解析语义）集成到 `MachineBlueprint` 中，并建立编译期物理校验机制，实现高保真的数字孪生底座。

## 2. 模块化实施路径

### 阶段 A: Machine.Framework 核心层升级 (Core Blueprint)
1.  **定义数据结构**: 在 `Machine.Framework.Core` 相关命名空间定义：
    *   `PhysicalType` 枚举 (Box, SuctionPen, RotaryTable, etc.) 及其关联参数。
    *   `PhysicalAnchor` 枚举 (Center, BottomCenter, TopCenter, Custom)。
    *   `PhysicalProperty` 记录类。
2.  **扩展 DSL 接口**: 修改 `IMachineBlueprint.cs` 中的 `IMountPointBuilder`。
    *   `AsBox(double x, double y, double z)`
    *   `AsSuctionPen(double diameter, double length)`
    *   `AsRotaryTable(double radius)`
    *   `AsLinearGuide(double length)`
    *   `AsGripper()`
    *   `AsMaterialSlot(double width, double height)` -- 为物料托盘预留物理边界
    *   `WithAnchor(PhysicalAnchor anchor)`
    *   `Vertical()`, `Horizontal()`, `Inverted()`
3.  **实现深度校验逻辑**: 修改 `MachineBlueprintBuilders.cs`。
    *   **标准化原点**: 在构造逻辑中强制执行标准化 Frames（如导轨原点在 Stroke=0）。
    *   **AlignmentConflict 强化版**: 
        *   不仅验证 `Orientation` 与 `LinkTo` 轴向的关系。
        *   **增强验证**: 如果节点标记为 `Vertical()`，则其 `WithStroke(x, y, z)` 的矢量必须具有显著的 Z 分量。若只有 X/Y 分量（水平位移），编译阶段抛出异常。
4.  **模型持久化**: 更新 `MachineConfig.cs` 相关的 DTO，确保物理元数据可序列化。

### 阶段 B: 遥测与模型映射层升级 (Telemetry & Mapping)
1.  **更新 Web 模型**: 修改 `WebMachineModel.cs`。
    *   `WebSceneNode` 增加 `PhysicalType`, `Size`, `Anchor`, `IsVertical` 字段。
2.  **更新映射器**: 在 `WebMachineModelMapper.cs` 中实现从 `PhysicalProperty` 到 `WebSceneNode` 的转换逻辑。

### 阶段 C: 场景配置迁移 (WinMachine.Server)
1.  **重构试点**: 修改 `ComplexRotaryMachine.cs`。
    *   按照 v1.4 规范声明吸笔、大盘、导轨的物理属性及锚点。
    *   移除 `Visuals.Start()` 中冗余的几何定义。

## 3. 验证计划
1.  **编译验证**: 确保全解编译通过。
2.  **单元测试**: 运行 `ComplexRotaryAssemblyScenarioTests`，确保重构不影响原有业务流。
    *   **注意**: 本次重构包含破坏性变更 (Breaking Changes)。若发现旧版测试用例与新的物理约束冲突且无法适配，允许直接移除过时测试代码，不要求向下兼容。
3.  **静态校验测试**: 编写一个故意的“错误配置”蓝图，验证 `AlignmentConflict` 校验器是否正常抛出异常。
4.  **无头碰撞预研 (可选)**: 在单元测试中尝试计算两个 MountPoint 的包围盒交集。

## 4. 评审点
- **计划确认**: 开发计划已根据 v1.4 需求同步更新。确认请回复：**计划确认**。
