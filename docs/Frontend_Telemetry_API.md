# WinMachine 前端实时遥测 API 文档 (v2.2)

本文档面向 Web 渲染端开发人员，详细定义了从后端获取机器模型（静态）及实时运行数据（动态）的完整协议。
v2.2 更新：引入 **统一坐标系**。场景节点现包含 `rotation` (初始姿态) 和 `stroke` (动作矢量)，前端应基于此进行几何变换，而非硬编码方向。

---

## 1. 通信基础 (Transport)

- **协议**: WebSocket (用于实时数据) & HTTP (用于初始化加载)
- **WebSocket URL**: `ws://{server_address}/ws/telemetry`
- **默认推送频率**: 后端根据变化情况推送 (Quiet Mode)，最高频率 ~30Hz。调试模式下建议设为 1Hz。

---

## 2. 静态模型加载 (Machine Schema)

在建立实时连接前，前端必须先加载静态模型以构建场景树和设备寄存器。

- **接口**: HTTP GET `http://{server}/api/machine/schema?name={scenario}`
- **响应体说明**: 

### 2.1 WebMachineModel
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **machineName** | string | 机器的人类可读名称 |
| **schemaVersion** | string | 模型协议版本（当前为 "1.0"） |
| **scene** | `WebSceneNode` | 根节点模型，包含完整的场景层级树 |
| **deviceRegistry** | `WebDeviceInfo[]` | 设备库，定义了所有可动部件的物理属性和动画参数 |

### 2.1.1 WebSceneNode (场景节点)
每个节点代表一个 3D 容器。前端渲染时，其 Transform 计算公式为：
`LocalMatrix = T(Offset) * R(Rotation) * T(Stroke * ConnectionState)`

| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **name** | string | 节点名称 |
| **offset** | `{x,y,z}` | 相对于父节点的初始位置偏移 (mm) |
| **rotation** | `{x,y,z}` | [v2.2] 初始旋转 (Euler Angles, deg)。顺序建议 Z->Y->X |
| **stroke** | `{x,y,z}` | [v2.2] **动作行程矢量**。当绑定的设备状态从 0->1 时，节点产生的位移。若为空则无位移。 |
| **linkedDeviceId** | string | 绑定的设备 ID。 |
| **children** | `WebSceneNode[]` | 子节点列表 |

### 2.2 WebDeviceInfo (设备详细定义)
用于定义某个 `deviceId` 的物理特征。
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **id** | string | 设备唯一识别符（如 "Cyl_Grips_Right"） |
| **type** | string | 视图类型（如 "Gripper", "SlideBlock", "RotaryTable"） |
| **baseType** | string | **核心逻辑类型**：`Axis` (连续轴) 或 `Cylinder` (二进制气缸) |
| **meta** | object | 设备元数据，具体字段见下表 |

**meta 字段详情 (根据 BaseType 不同)**
- **共通**: `isVertical` (bool), `isReversed` (bool), `board` (string), `channel` (int)
- **BaseType == "Axis"**:
    - `min`: 软限位最小值 (mm/deg)
    - `max`: 软限位最大值 (mm/deg)
    - 其他: `radius` (旋转半径), `length` (导轨长度)
- **BaseType == "Cylinder"**:
    - `moveTime`: **动作设计耗时 (ms)**。前端在收到状态切换指令时，应在此时间内匀速播放动画。
    - `openWidth`: 张开时的物理宽度 (mm)
    - `closeWidth`: 闭合时的物理宽度 (mm)

---

## 3. 客户端指令 (Client -> Server)

指令通过 WebSocket 发送 JSON。

### 3.1 启动 (Start)
```json
{
  "cmd": "Start",
  "scenario": "Complex_Rotary_Assembly"
}
```

### 3.2 停止 (Stop)
```json
{
  "cmd": "Stop"
}
```

---

## 4. 实时遥测数据 (Server -> Client)

### 4.1 遥测包结构 (TelemetryPacket)
| 字段名 | 类型 | 说明 |
| :--- | :--- | :--- |
| **t** | long | 服务器 Unix 时间戳 (ms)。单调递增，用于算 DeltaTime。 |
| **step** | string | 当前业务步骤名称（对应 DSL 中的 `Name("...")`）。 |
| **m** | object | **运动指令图**。Key 为 DeviceID，Value 含义见下节。 |
| **io** | object | 传感器原始状态字典。Key 为信号 ID，Value 为 `bool` 或 `number`。 |
| **e** | object[] | 事件列表，如 `FlowStarted`, `FlowStopped`, `Error`。 |

---

### 4.2 运动指令 (`m` 字段) 的渲染逻辑

前端必须根据设备的 `BaseType` 采用不同的渲染策略：

#### A. 如果 BaseType == "Axis" (如马达、旋转台)
- **Value 含义**: **当前物理绝对坐标** (mm 或 deg)。
- **渲染建议**: 后端会高频推送位置（33ms/帧）。前端直接使用 `Lerp` 或 `RequestAnimationFrame` 平滑移动到该位置即可。

#### B. 如果 BaseType == "Cylinder" (如气缸、夹爪、真空泵)
- **Value 含义**: **目标二值状态** (`0` 代表 Home/Close, `1` 代表 Work/Open)。
- **渲染建议**: 
    1. 前端监听到 `m[id]` 的值从 `0` 变 `1` (或反之)。
    2. 从 `deviceRegistry` 中读取该 ID 的 `moveTime`。
    3. **自启动动画**: 在 `moveTime` 时间内，将该视觉元件从“闭合位置”动画过渡到“张开位置”。
    4. **注意**: 由于气缸没有中间编码器，前端不需要后端发中间位置，只需监听“状态翻转”并本地触发动画。

---

## 5. 状态转换事件 (Events)

### 5.1 FlowStarted
```json
{ "type": "FlowStarted", "payload": { "scenario": "...", "tickBase": 1234567, "schemaVersion": "1.0" } }
```

### 5.2 FlowStopped
```json
{ "type": "FlowStopped", "payload": { "reason": "Complete | Error | UserStop" } }
```

### 5.3 Error
```json
{ "type": "Error", "msg": "错误描述...", "payload": { "code": "ERR_XXX", "source": "设备ID" } }
```

### 5.4 Attach / Detach (物流逻辑)
用于处理夹爪抓取物体后的父子关系变更。
- `Attach`: `{ "child": "物料ID", "parent": "设备ID/挂载点ID" }`
- `Detach`: `{ "child": "物料ID", "newParent": "容器ID" }`
