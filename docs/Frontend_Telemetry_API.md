# WinMachine 前端实时遥测 API 文档 (v2.0)

本文档定义了 WinMachine 仿真后端与前端可视化界面之间的通信协议。

## 1. 连接协议 (Connection)

- **协议**: WebSocket
- **URL**: `ws://{server_address}/ws/telemetry`
- **频率**: 后端主动推送，约 30Hz (每 ~33ms 一帧)
- **时钟同步**: 后端使用统一服务器时间 (Unix Timestamp Milliseconds)。前端需根据首帧 `t` 值建立本地时间偏移。

## 2. 客户端指令 (Client -> Server)

前端通过 WebSocket 发送 JSON 格式指令控制流程。

### 2.1 启动流程 (Start)
- **说明**: 幂等操作。如果已有流程在运行，后端会先自动 `Stop` 再启动新流程。
- **响应**: 成功启动后，后端会在首个数据包中发送 `FlowStarted` 事件。
```json
{
  "cmd": "Start",
  "scenario": "Complex_Rotary_Assembly" // 必须与后端 ScenarioRegistry 中注册的名称一致
}
```

**错误语义 (Unknown Scenario)**

- 如果 `scenario` 不存在，后端不会断开连接；会返回一个数据包，其中 `e` 里按顺序包含：
  1) `Error(code="ERR_SCENARIO_NOT_FOUND", source=<scenario>)`
  2) `FlowStopped(reason="Error")`

### 2.2 停止/中断 (Stop)
- **说明**: 幂等操作。无论当前是否运行，均可发送。
- **响应**: 必然触发 `FlowStopped` 事件（Reason="UserStop"）。
- **注意**: 文档约定 Stop 后可能会收到少量残余运动帧，前端应丢弃这些帧或做平滑处理，直到收到 FlowStopped。
```json
{
  "cmd": "Stop"
}
```

---

## 3. 遥测数据包 (Server -> Client)

后端推送的每一帧 JSON 数据包 (`TelemetryPacket`) 结构如下。

### 3.1 核心结构

```typescript
interface TelemetryPacket {
  /** 
   * [必需] 服务器时间戳 (Unix Milliseconds)
   * 类型: long (e.g., 1706325000123)
   * 规则: 单调递增。重连后不归零。前端可用此计算 DeltaTime 或进行网络延迟补偿。
   */
  t: number; 

  /**
   * [必需] 当前业务步骤名称 (Context Step)
   * 用途: 显示在 UI 顶部，告知用户机器当前在干什么（如 "右侧旋转180"）。
   * 规则: 对应 DSL `Name("...")`。若当前无步骤，为空字符串。
   */
  step: string;

  /** 
   * [可选] 视觉/动画位置增量 (Motion Targets)
   * Key: DeviceID (必须与静态 JSON ID 一致)
   * Value: 物理量 (mm / degrees / width)
   * 
   * 规则 1: **物理单位**。禁止 0.0-1.0 归一化。前端需结合静态配置的 Max/Min 渲染。
   * 规则 2: **增量更新 (Delta/Dirty only)**。
   *         - 后端会进行抖动过滤 (Epsilon > 0.001)。小于此变化的不会发送。
   *         - 前端必须缓存上一帧状态。若 Map 中缺某 ID，则保持上一帧的值。
   * 规则 3: **一对多驱动**。如果 DeviceID 绑定了多个 SceneNode，所有节点同时应用该值。
   */
  m?: Record<string, number>;

  /** 
   * [可选] 真实 IO/传感器状态 (Business Logic Truth)
   * Key: SignalName (e.g., "Cyl_In", "Cyl_Out", "Pressure_1")
   * Value: boolean (数字量) or number (模拟量)
   * 用途: UI 面板指示灯。直接显示，不插值。
   */
  io?: Record<string, boolean | number>;

  /** 
   * [可选] 离散业务事件列表
   * 规则: 事件是有序的。前端必须按数组顺序处理。
   */
  e?: TelemetryEvent[];
}

interface TelemetryEvent {
  type: EventType;
  msg: string;      // 人类可读消息 (Log)
  payload?: any;    // 根据 type 强类型定义
}

type EventType = "FlowStarted" | "FlowStopped" | "Error" | "Attach" | "Detach" | "Spawn";
```

---

## 4. 关键事件 Payload 定义 (Schema)

### 4.1 流程开始 (FlowStarted)
作为 Start 指令的 ACK，包含初始化信息。

```typescript
{
  type: "FlowStarted",
  payload: {
    scenario: "Complex_Rotary_Assembly",
    tickBase: 1706325000000, // 启动时的服务器时间
    schemaVersion: "1.0"
  }
}
```

### 4.2 流程结束 (FlowStopped)
**规则**: 无论是正常完成、报错还是手动停止，流程结束时**必通过此事件通知**。前端收到后应停止动画，允许用户重新 Start。

```typescript
{
  type: "FlowStopped",
  payload: {
    reason: "Complete" | "Error" | "UserStop"
  }
}
```

### 4.3 运行时错误 (Error)
发生错误时发送。**注意**: Error 事件后**一定会**紧跟一个 `FlowStopped(reason="Error")` 事件。

```typescript
{
  type: "Error",
  msg: "滑台未到位！超时。",
  payload: {
    code: "ERR_TIMEOUT",
    source: "Cyl_Middle_Slide"
  }
}
```

### 4.4 物流绑定 (Attach/Detach/Spawn)

**Spawn (生成)**
用于在场景中动态创建物体（如上料）。
```typescript
{
  type: "Spawn",
  payload: {
    id: "Wafer_001",       // 新物体的唯一 ID
    prefab: "Wafer_300mm", // 对应的预制体/模型类型
    initialParent: "Tray_Slot_1" // 初始挂载点 ID
  }
}
```

**Attach (抓取)**
修改父子关系。
```typescript
{
  type: "Attach",
  payload: {
    child: "Wafer_001",      // 被抓物体 ID
    parent: "Cyl_Grips_Left" // 新父节点 ID (设备ID)
  }
}
```

**Detach (释放)**
```typescript
{
  type: "Detach",
  payload: {
    child: "Wafer_001",
    newParent: "Tray_Slot_5" // 目标容器 ID
  }
}
```

---

## 5. 静态模型加载 (Static Model Loading)

前端启动时，需先加载静态描述文件以建立 Scene Graph。

1.  **加载方式**: HTTP GET `http://{server}/api/machine/schema?name={scenario}`
2.  **文件格式**: 参见后端导出的 `*.json` (如 `rotary_lift_assembly.json`)。
3.  **ID 命名空间**: 
    - `DeviceID` 全局唯一。
    - `m` 字段中的 Key 必须与 JSON `deviceRegistry[].id` 完全匹配。

### 5.1 可用场景列表

前端可以通过该接口列出所有可用场景（用于下拉选择/自动补全）：

- HTTP GET `http://{server}/api/machine/scenarios`
- 响应：JSON 字符串数组

```json
["Complex_Rotary_Assembly"]
```

**错误语义**

- `name` 缺失/空白：HTTP 400
- `name` 不存在：HTTP 404
- 错误响应示例：

```json
{
  "code": "ERR_SCENARIO_NOT_FOUND",
  "message": "Unknown scenario 'Foo'.",
  "knownScenarios": ["Complex_Rotary_Assembly"]
}
```

## 6. 前端最佳实践 (Best Practices)

1.  **插值与缓冲**: 建议维护 `RenderingBuffer`。收到 `t=100` 的包时，不要立即渲染，而是与 `t=67` 的包进行插值。保持 ~33ms 的渲染延迟以换取丝滑流畅度。
2.  **全量/增量处理**: 
    - 不要假设每帧都有 `m` 数据。
  - 也不要假设 start 时所有设备都在原点。必须以包含 `FlowStarted` 的首包，以及随后收到的 `m`/`io`（可能包含 Snapshot 全量 `m`）为准。
3.  **容错**: 
    - 如果 WebSocket 断开，自动重连。
    - 重连后，后端可能会发送当前最新的 Snapshot（全量 `m`），前端应能平滑过渡。
