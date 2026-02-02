# REQ-Differential-Feeder-Complex-X-Y-Flip

## 1. 业务意图 (Business Intent)
构建一个符合真实工业逻辑的上下料系统：
- **全球坐标系翻转**：X 为左右横梁，Y 为前后递料（滑台方向）。
- **四吸笔横向阵列 (X-Axis)**：排列顺序为 `[L1]` (上料1), `[L2]` (上料2), `[U1]` (下料1), `[U2]` (下料2)。
- **差分双 Z 轴控制**：
    - `Axis_Z1` 控制 `(L1, U1)` 对；`Axis_Z2` 控制 `(L2, U2)` 对。
    - 差分关系：Z 正向位移 => `U` 下降/`L` 上升；Z 负向位移 => `L` 下降/`U` 上升。
- **X 轴横移对位**：
    - 下料时：Feeder_X 移动至 `Pos_Unload`，使 `(U1, U2)` 对齐滑台双工位。
    - 上料时：Feeder_X 移动至 `Pos_Load`，使 `(L1, L2)` 对齐滑台双工位。

## 2. 逻辑模型 (Logic Model)

```haskell
module WinMachine.Requirements.ComplexFeederMechanics where

-- | 吸笔排列 (Spacing = S, Slide_Pitch = P)
-- L1: x=0, L2: x=S, U1: x=P, U2: x=P+S
-- 其中 P 必须等于滑台（扫码座）两个吸笔位的间距

-- | Feeder 动作流程 (Sequential Control)
workflow "RefinedFeederCycle" = do
    -- 0. 检查初始物料
    Material(L1, L2).EnsureExists(New_Part)

    -- 1. 下料阶段 (Unload)
    Motion(Feeder_X).MoveTo(Pos_Align_U) -- 使 U1/U2 对齐滑台
    Motion(Z1, Z2).MoveTo(50)           -- U 下降
    Material(Slide).AttachTo(U1, U2)     -- 吸走旧料
    Material(Slide).Transform(Empty)
    Motion(Z1, Z2).MoveTo(0)            -- 回零

    -- 2. 上料阶段 (Load)
    Motion(Feeder_X).MoveTo(Pos_Align_L) -- 补偿 X 偏移，使 L1/L2 对齐滑台
    Motion(Z1, Z2).MoveTo(-50)          -- L 下降
    Material(L1, L2).Detach             -- 放置新料
    Material(Slide).Transform(New)
    Motion(Z1, Z2).MoveTo(0)            -- 回零

-- | 物理契约：物料可见性
invariant "MaterialVisibilityContract" = do
    -- 吸笔末端必须显式声明为 MaterialSlot
    -- 所有的 Spawn 动作必须绑定到末端真空 ID
```

## 3. 视觉表现要求 (Visual Specs)
- **节点识别**：所有吸笔节点必须在蓝图中通过特定机制（如设备绑定）被渲染引擎识别为 `SuctionPen` 类型。
- **物料挂载**：上料笔在初始化时必须显示物料块。
- **实时动画**：滑块在 Y 轴滑动，吸笔梁在 X 轴滑动，吸笔在 Z 轴伸缩，必须在 3D 界面清晰可见。

## 4. 验收标准
- [ ] 后端：新增 `Axis_Feeder_X` 轴。
- [ ] 后端：更新 `FeederJob` 逻辑，包含 X 对位偏移。
- [ ] 前端：能看到 4 个独立排列的吸笔。
- [ ] 前端：上料笔初始自带物料。
- [ ] 前端：吸笔伸缩动画正常（非瞬间跳变）。
