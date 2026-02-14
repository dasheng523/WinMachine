# REQ-Physical-Blueprint-Refactoring: 物理蓝图重构需求模型

## 1. 核心目标
将组件的**物理几何属性**（尺寸、朝向、功能语义）从视觉层（Visuals）迁移至蓝图层（Blueprint/Core），以支持：
1.  **无头物理仿真**：在不启动 UI 的情况下支持碰撞检测和运动学校验。
2.  **单一事实源**：物理结构定义即物理行为，避免逻辑与视觉参数不一致。
3.  **视觉层自动化**：Visuals 能够根据物理语义自动推断默认 3D 外观。

## 2. 逻辑模型 (Haskell-style DSL)

```haskell
module Requirements.PhysicalBlueprintRefactoring where

-- | 领域实体定义
-- 将物理属性定义为核心数据结构
data PhysicalType 
    = Box               -- 普通立方体 (通用)
    | SuctionPen Double Double -- ^ 直径 (Diameter), 长度 (Length)
    | Gripper                -- ^ 夹爪 (具备夹取物理特性)
    | RotaryTable Double     -- ^ 半径 (Radius)
    | LinearGuide Double     -- ^ 长度 (Length)

data Anchor 
    = Center           -- ^ 几何中心
    | BottomCenter     -- ^ 底部中心
    | TopCenter        -- ^ 顶部中心
    | Custom Vector3D  -- ^ 自定义相对偏移

data PhysicalProperty = PhysicalProperty
    { _type        :: PhysicalType
    , _size        :: Vector3D       -- 物理包围盒尺寸 (Width, Height, Depth)
    , _orientation :: Orientation    -- 物理朝向 (Horizontal/Vertical)
    , _anchor      :: Anchor         -- 物理原点/锚点位置
    , _isInverted  :: Bool           -- 是否反向安装
    }

-- | 业务流程定义
-- 扩展 IMountPointBuilder 接口以支持物理属性声明
workflow "DefinePhysicalStructure" :: MountPointBuilder -> IO ()
workflow builder = do
    -- 1. 前置条件 (Pre-conditions)
    -- 中文注释: 必须在挂载点名称定义后进行配置
    verify "NodeIdentified" (not $ null $ _name builder)

    -- 2. 物理声明 (Physical Declaration)
    -- 中文注释: 声明该挂载点在物理世界的表现形式
    declare "Geometry"  (builder.AsBox 100 20 20)
    declare "Semantic"  (builder.AsSuctionPen 5 50)
    declare "Direction" (builder.Vertical)

    -- 3. 后置条件 (Post-conditions)
    -- 中文注释: 蓝图数据模型中必须包含对应的物理描述
    ensure "BlueprintUpdated" (hasPhysicalMeta $ getDefinition builder)

-- | 边界情况处理 (Edge Cases)
handleError :: PhysicalError -> Action
handleError ZeroSizeDef = Error "物理尺寸不可为零，否则无法进行碰撞检测"
handleError AlignmentConflict = ValidateError "当 MountPoint 定义为 Vertical 时，其 LinkTo 设备(如轴或气缸)的运动矢量必须与全局 Z 轴平行。检测到冲突方向，编译失败。"
handleError TypeMismatch = Error "动态行程(Stroke)的方向必须与物理朝向(Orientation)一致"
handleError HeadlessCollision = Logic "无头运行时，若发生包围盒重叠，必须抛出物理异常而非静默忽略"

-- | 验收属性 (Properties)
properties = 
    [ "Prop1: 视觉层 (Visuals) 必须能自动从物理蓝图推断默认外观"
    , "Prop2: 逻辑内核 (Core) 在不引用任何渲染库的情况下，应能计算两个 MountPoint 的碰撞"
    , "Prop3: 现有的 Flow 业务逻辑代码不需要因为增加物理属性而修改(保持透明性)"
    , "Prop4: 遥测 API (Schema) 必须携带完整物理参数，使前端具备\"零预设\"渲染能力"
    ]

-- | 5. API 与遥测协议演进 (API & Schema Evolution)
-- 5.1 WebSceneNode 扩展:
--     - 增加 physicalType: 枚举 (Box | Cylinder | SuctionPen | ...)
--     - 增加 size: Vector3D (描述物理碰撞或渲染边界)
--     - 增加 metadata: 存储特定物理参数 (如吸笔直径、旋转台半径)

-- 5.2 前端渲染逻辑重定义:
--     - 模式驱动 (Schema-Driven): 前端应优先基于 WebSceneNode 的物理属性生成几何体。
--     - 姿态推断: 物料挂载位置应根据工位 (Station) 的物理原点自动计算，减少硬编码坐标。

-- ------------------------------------------------------------------
-- 6. 物理基础规范 (Physical Standards)
-- ------------------------------------------------------------------
-- 6.1 坐标系 (Coordinate System):
--     - 规则: 遵循\"右手笛卡尔坐标系\" (Right-hand Rule)。
--     - 轴向: Z 轴恒定向上 (Up)，X 轴向右，Y 轴向前。
--     - 意义: Horizontal/Vertical 的定义均基于此全局 Z 轴参考。

-- 6.2 碰撞体推断规则 (Collision Proxy):
--     - Box: 对应 AABB/OBB 立方体。
--     - SuctionPen: 自动推断为沿有效轴向的\"圆柱体\" (Cylinder) 碰撞体。
--     - 尺寸单位: 全局统一使用毫米 (mm)。

-- 6.3 标准化组件坐标系 (Standardized Component Frames):
--     - 目的: 强制统一物理原点，消除各场景自定义锚点导致的一致性问题。
--     - LinearGuide: 物理原点一律强制定义在 Stroke = 0 的运动起点位置。
--     - RotaryTable: 物理原点一律强制定义在旋转中心顶部 (Top Center of Pivot)。
--     - SuctionPen: 物理原点定义在吸笔与安装板的接合处 (Top Center)。
```

## 3. 自我攻击检查 (Shadow Adversary)
*  **针对本案漏洞**: 如果用户定义了物理尺寸，但没有正确设置 `WithOffset` (Pivot)，碰撞检测可能会基于父节点的 0,0,0 点。
*  **修复建议**: 在 Blueprint 编译器增加验证，强制所有具备 `PhysicalProperty` 的节点必须显式指定其物理锚点及局部坐标偏移。

## 4. 评审点
需求模型已完成第四次迭代 (v1.4)，新增了：
1.  **物理锚点 (PhysicalAnchor)**：引入显式锚点类型（中心、底部、顶部、自定义），彻底解决 3D 模型渲染时的对齐问题。
2.  **运动方向校验 (Alignment Validation)**：增加蓝图编译期校验，若垂直组件关联了水平运动轴，将抛出 `ValidateError`。
3.  **标准化组件坐标系 (Frames)**：为导轨、旋转盘等定义了强制一致的物理原点规范（如导轨原点必在 `Stroke=0` 处）。
4.  **物理仿真增强**：吸笔等组件现在携带完整的几何参数，支持“无头”模式下的高精度碰撞计算。
