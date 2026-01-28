# WinMachine 前端实时遥测 API 文档 (v2.3)

本文档面向 Web 渲染端开发人员，详细定义了从后端获取机器模型（静态）及实时运行数据（动态）的完整协议。
v2.3 更新：引入 **物料流状态管理 (Material Flow)**。遥测数据现包含物料实体状态信息，支持物料的生成、传输、状态转换及销毁流程。

---

## 1. 通信基础 (Transport)

- **协议**: WebSocket (用于实时数据) & HTTP (用于初始化加载)
- **WebSocket URL**: `ws://{server_address}/ws/telemetry`
- **默认推送频率**: 后端根据变化情况推送 (Quiet Mode)，最高频率 ~30Hz。

---

## 2. 静态模型加载 (Machine Schema)

在建立实时连接前，前端必须先加载静态模型以构建场景树和设备寄存器。

- **接口**: HTTP GET `http://{server}/api/machine/schema?name={scenario}`

### 2.1 WebMachineModel
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **machineName** | string | 机器的人类可读名称 |
| **schemaVersion** | string | 模型协议版本 |
| **scene** | `WebSceneNode` | 根节点模型，包含完整的场景层级树 |
| **deviceRegistry** | `WebDeviceInfo[]` | 设备库，包含所有可动部件及物料工位的定义 |

### 2.2 WebDeviceInfo (设备/工位定义)
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **id** | string | 设备/工位唯一识别符 |
| **type** | string | 视图类型：`Gripper`, `SlideBlock`, `RotaryTable`, **`MaterialSlot` (新)** |
| **baseType** | string | 逻辑类型：`Axis`, `Cylinder`, **`Station` (新)** |
| **meta** | object | 设备元数据 |

---

## 3. 客户端指令 (Client -> Server)
通过 WebSocket 发送 JSON 指令。支持 `Start`, `Stop`, `Pause`, `Reset` 等。

---

## 4. 实时遥测数据 (Server -> Client)

### 4.1 遥测包结构 (TelemetryPacket)
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **t** | long | 服务器 Unix 时间戳 (ms)。 |
| **step** | string | 当前业务步骤名称。 |
| **m** | object | **运动指令图**。Key: DeviceID, Value: 轴位置或气缸状态。 |
| **mat** | object | **[v2.3] 物料状态图**。Key: StationID, Value: 物料实体对象信息。 |
| **io** | object | 传感器原始状态字典。 |
| **e** | object[] | 事件列表。 |

### 4.2 运动指令 (`m` 字段) 渲染逻辑
- **Axis**: 连续坐标位置，前端平滑插值移动。
- **Cylinder**: 目标二值状态，前端根据 `moveTime` 播放局部动画。

### 4.3 物料实体 (`mat` 字段) 渲染逻辑
如果遥测包包含 `mat` 字典，前端应按以下规则维护物料模型：
1. **空状态**: `mat["Vac1"] == null` -> 该工位当前无物料，隐藏或销毁关联的物料模型。
2. **挂载状态**: `mat["Vac1"] == { "id": "P001", "class": "New" }`
   - 若物料 ID 为首次出现：在 `Vac1` 挂载点位置创建模型。
   - `class` 定义了物料的视觉外观（如 `New` 为亮蓝色，`Old` 为灰色）。
   - **自动跟随**: 物料模型作为 `Vac1` 场景节点的子对象渲染，自动继承滑台或转盘的位移。

---

## 5. 实时事件 (Events)

### 5.1 物料生命周期事件
后端通过事件流精准控制物料的“突变”动作：

- **MaterialSpawn (生成)**
  `{ "type": "MaterialSpawn", "payload": { "id": "P_001", "at": "Source_A", "class": "New" } }`
  前端在指定的源头位置实例化一个新的物料模型。

- **MaterialTransform (变质)**
  `{ "type": "MaterialTransform", "payload": { "id": "P_001", "to": "Old" } }`
  前端修改物料模型的外观或状态标识。

- **MaterialConsume (销毁)**
  `{ "type": "MaterialConsume", "payload": { "id": "P_001" } }`
  物料流程结束，前端销毁或回收该模型。

---

## 6. Changelog (修订记录)

### v2.3 (当前版本)
- **新增**: `TelemetryPacket.mat` 字典，用于同步工位物料占用状态。
- **新增**: `MaterialSlot` 设备类型，用于在架构中标识物料挂载点。
- **新增**: `MaterialSpawn`, `MaterialTransform`, `MaterialConsume` 事件定义。
- **优化**: 调整 `Attach/Detach` 语意，建议优先使用 `mat` 状态图进行声明式同步。

### v2.2
- **新增**: 统一坐标系架构，引入 `rotation` (初始姿态) 和 `stroke` (动作矢量)。
- **新增**: `moveTime` 属性，由后端定义动作预期时长，前端负责插值动画。

### v2.1
- **新增**: 基础 WebSocket 遥测协议，支持 `m` 指令图和 `io` 信号字典。
