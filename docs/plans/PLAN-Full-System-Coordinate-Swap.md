# PLAN-Differential-Feeder-Complex-X-Y-Flip

## 1. 技术路径 (Technical Path)
执行全系统的坐标轴深度重构，重点实现“X轴平移 + 4笔差分双Z轴”的复合上下料机构，并将全局 X/Y 轴向映射进行对调。

## 2. 任务拆解 (Task Decomposition)

### 2.1 坐标轴全系统互换 (X <-> Y Swap)
- [ ] **蓝图 Offset/Stroke 置换**：将所有组件的 `WithOffset(x, y, z)` 修改为 `WithOffset(y, x, z)`，`WithStroke(x, y, z)` 修改为 `WithStroke(y, x, z)`。
- [ ] **滑台 (Middle Slide)**：主轴方向设定为 **Y 轴**。
- [ ] **模组 (Assembly Modules)**：沿 Y 轴分布，Y 坐标分别为 -370 和 +370。

### 2.2 Feeder 机构深度重构
- [ ] **硬件定义**：
    - 新增 `Axis_Feeder_X`：行程 200mm，控制整个 Feeder 机构左右平移。
    - 定义 4 个吸笔 ID: `vacFeederL1, L2, U1, U2`。
    - 定义 2 个 Z 轴 ID: `axisFeederZ1, Z2`。
- [ ] **物理布局 (Blueprint)**：
    - 在 `Feeder_Bridge` 下挂载 `Axis_Feeder_X`。
    - 在 `Axis_Feeder_X` 下挂载两组吸笔机架。
    - **吸笔排列**（从左到右，X 轴负到正）：
        - `L1` (X=-120), `L2` (X=-40) -> 属于 `Z1_Group`, `Z2_Group` 的 L 侧。
        - `U1` (X=40), `U2` (X=120)  -> 属于 `Z1_Group`, `Z2_Group` 的 U 侧。
    - **差分逻辑**：
        - `Pen_Ux` 使用 `WithStroke(0, 0, -60)` (Z+ 为降)。
        - `Pen_Lx` 使用 `WithStroke(0, 0, 60)` (Z- 为降)。
- [ ] **类型标记**：在蓝图中通过 `WithType("SuctionPen")` 或 `WithType("MaterialSlot")` 为节点注入元数据（或使用 ID 匹配逻辑）。

### 2.3 业务流程优化 (Flow Job)
- [ ] **FeederJob 重写**：
    1. `Feeder_X` 移动至 **对齐位 A** (使 U1/U2 对齐滑台位置)。
    2. 执行下料吸取逻辑。
    3. `Feeder_X` 移动至 **对齐位 B** (使 L1/L2 对齐滑台位置)。
    4. 执行上料放置逻辑。
    5. 重置 `Feeder_X` 到安全中点。
- [ ] **物料初始化**：在主流程 `cycle` 开始前，显式为 `L1, L2` 产生初始 `New` 物料。
- [ ] **安全屏障升级**：`SafetyBarrier` 需要等待 `Feeder_X` 回到原点且 Z 轴回到 0。

## 3. 风险点 (Risks)
- **R-1: 视觉丢失**：若吸笔 ID 没能正确对应到具有 `SuctionPen` 样式的节点，前端仍不显示吸笔几何体。
- **R-2: 坐标解析**：差分行程正负反转后，物理碰撞风险增加，需严格校验 `SafetyBarrier`。

## 4. 评审建议
请确认：吸笔之间的 X 间距（40mm, 120mm 等）是否需要根据扫码座的实际孔位间距进行参数化调整？目前计划按 80mm 进行对齐。
